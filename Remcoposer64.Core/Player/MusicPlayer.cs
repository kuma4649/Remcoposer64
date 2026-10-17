using NAudio.Midi;
using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using System.Runtime.Intrinsics.Arm;

namespace Remcoposer64.Core.Player
{
    public class MusicPlayer : IPlayerChannel
    {
        private Setting.midiOutInfo[] _midiOutInfo;
        private Setting.midiInInfo[] _midiInInfo;

        // --- Tempo / Timing ---
        private int _ppq = 480;               // デフォルトPPQ（後でプロジェクトから取得）
        private double _bpm = 120;               // 人間が扱うテンポ
        
        private int _meas = 0;
        private int _step = 0;
        private int _tick = 0;
        private int _den = 4;
        private int _mol = 4;

        private double _currentPpq = 0;       // 現在のPPQ

        // --- 状態 ---
        public int FireFrameProcess { get; set; } = 0;
        public bool IsFinished { get; set; } = true;

        private MIDIProject _project = null;
        public long frameCounter;
        // 44100Hz刻みのカウンター
        public long timerCounter=0;
        private Dictionary<int, long>[][] dicNoteOnTable = null;
        private NAudio.Midi.MidiOut[] midiOutDevice;
        private NAudio.Midi.MidiIn[] midiInDevice;

        private byte[] RolEx = new byte[11];

        private Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode = new();


        public void Initialize(Setting.midiOutInfo[] midiOut, MidiOut[] midiOutDevice, Setting.midiInInfo[] midiIn, MidiIn[] midiInDevice)
        {
            _midiOutInfo = midiOut;
            _midiInInfo = midiIn;

            _currentPpq = 0;

            FireFrameProcess = 0;
            IsFinished = true;

            //if (_midiOutInfo != null)
            //{
            //    dicNoteOnTable = new Dictionary<int, long>[_midiOutInfo.Length][];
            //    for (int i = 0; i < _midiOutInfo.Length; i++)
            //    {
            //        dicNoteOnTable[i] = new Dictionary<int, long>[16];
            //        for (int j = 0; j < 16; j++)
            //        {
            //            dicNoteOnTable[i][j] = new Dictionary<int, long>();
            //        }
            //    }
            //}

            this.midiOutDevice = midiOutDevice;
            this.midiInDevice = midiInDevice;
        }
        
        public void PlayMusic(MIDIProject project)
        {
            if (project == null) return;

            _project = project;
            _bpm = project.Information.Tempo;
            _ppq = project.Information.TimeBase;
            dicNoteOnTable = new Dictionary<int, long>[project.Information.MaxLengthMIDIOut][];
            for (int i = 0; i < project.Information.MaxLengthMIDIOut; i++)
            {
                dicNoteOnTable[i] = new Dictionary<int, long>[16];
                for (int j = 0; j < 16; j++)
                {
                    dicNoteOnTable[i][j] = new Dictionary<int, long>();
                }
            }

            // 再生開始
            StartPlayback();
        }

        public void StartPlayback()
        {
            _currentPpq = 0;

            ClearPlayedFlag();
            frameCounter = 0;
            timerCounter = 0;
            _meas = 0;
            _step = 0;
            _tick = 0;
            _den = _project.Information.BeatDen;
            _mol = _project.Information.BeatMol;

            FireFrameProcess = 0;
            IsFinished = false;

            measureStartNode.Clear();

            BuildMeasureStartNode();
        }

        private void BuildMeasureStartNode()
        {
            measureStartNode.Clear();

            LinkedListNode<MIDITrack> nTrk = _project.Track.First;
            for (int t = 0; t < _project.Track.Count; t++)
            {
                var trk = nTrk;

                LinkedListNode<MIDIPart> nPrt = trk.Value.Part.First;
                for (int p = 0; p < trk.Value.Part.Count; p++)
                {
                    var prt = nPrt.Value;
                    var node = prt.Event.First;
                    
                    int meas = 0;

                    measureStartNode[(t, p, meas)] = node;

                    while (node != null)
                    {
                        var evt = node.Value;

                        if (evt.Type == MIDIEventType.BarLine)
                        {
                            meas++;
                            measureStartNode[(t, p, meas)] = node;
                        }
                        else if (evt.Type == MIDIEventType.SameMeas)
                        {
                            if (node.Previous == null || node.Previous.Value.Type != MIDIEventType.BarLine)
                            {
                                meas++;
                                measureStartNode[(t, p, meas)] = node;
                            }
                        }
                        else if (node.Previous != null && node.Previous.Value.Type == MIDIEventType.SameMeas)
                        {
                            meas++;
                            measureStartNode[(t, p, meas)] = node;
                        }

                        node = node.Next;
                    }

                    nPrt = nPrt.Next;
                }

                nTrk = nTrk.Next;
            }
        }


        public void Stop()
        {
            FireFrameProcess = 0;
            IsFinished = true;
        }

        private void ClearPlayedFlag()
        {
            _project.Information.Played = false;
            LinkedListNode<MIDITrack> nTrk = _project.Track.First;
            while (nTrk != null)
            {
                nTrk.Value.Played = false;
                nTrk.Value.LoopStack = new Stack<(LinkedListNode<MIDIEvent>, int?)>();
                nTrk.Value.SameMeasStack = new Stack<LinkedListNode<MIDIEvent>>();
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                while (nPrt != null)
                {
                    //nPrt.Value.Played = false;
                    nPrt.Value.CurrentEvent = nPrt.Value.Event.First;
                    nPrt.Value.CurrentEventFrame = 0;
                    LinkedListNode<MIDIEvent> nEvt = nPrt.Value.Event.First;
                    while (nEvt != null)
                    {
                        nEvt = nEvt.Next;
                    }
                    nPrt = nPrt.Next;
                }
                nTrk = nTrk.Next;
            }
        }


        public void UpdateTimer()
        {
            timerCounter++;
            // 1frameあたりのppq進行量
            double ppqPerFrame = (_bpm * _ppq) / 60.0 / Common.Common.DATA_SEQUENCE_FREQUENCE;
            // 現在ppqを進める
            _currentPpq += ppqPerFrame;

            // 1ppqに達したらイベント処理フラグを立てる
            FireFrameProcess = 0;
            while (_currentPpq >= 1.0)
            {
                FireFrameProcess++;
                _currentPpq -= 1.0;   // 1ppq消費
            }
        }

        public void ProcessFrame(long seqCounter)
        {

            ProcessNoteOff();

            _tick++;
            if (_tick >= _ppq * (4.0 / _den))
            {
                _tick = 0;
                _step++;
                if (_step >= _mol)
                {
                    _step = 0;
                    _meas++;
                }
            }

            LinkedListNode<MIDITrack> nTrk = _project.Track.First;
            bool playedAllTracks = true;
            while (nTrk != null)
            {
                MIDITrack trk = nTrk.Value;
                if (trk.Played)
                {
                    nTrk = nTrk.Next;
                    continue;
                }

                playedAllTracks = false;
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                bool playedAllParts = true;

                while (nPrt != null)
                {
                    MIDIPart prt = nPrt.Value;

                    // 初期化（イベント開始前）
                    if (prt.CurrentEventFrame == -1)
                    {
                        prt.CurrentEventFrame = prt.StartTick;
                        prt.CurrentEvent = prt.Event.First;
                    }

                    if (prt.CurrentEvent == null)
                    {
                        nPrt = nPrt.Next;
                        continue;
                    }

                    playedAllParts = false;

                    if (frameCounter < prt.CurrentEventFrame)
                        break;

                    while (prt.CurrentEvent != null)
                    {
                        MIDIEvent evt = prt.CurrentEvent.Value;

                        if (frameCounter < prt.CurrentEventFrame)
                            break;

                        prt.CurrentEvent = PlayEvent(trk, nPrt, prt.CurrentEvent);
                        prt.CurrentEventFrame += evt.ST;
                    }

                    nPrt = nPrt.Next;
                }

                if (playedAllParts)
                    trk.Played = true;

                nTrk = nTrk.Next;
            }

            if (playedAllTracks)
                _project.Information.Played = true;

            frameCounter++;

        }

        private LinkedListNode<MIDIEvent> PlayEvent(MIDITrack trk, LinkedListNode<MIDIPart> nPrt, LinkedListNode<MIDIEvent> nEvt)
        {
            MIDIPart prt = nPrt.Value;
            MIDIEvent evt = nEvt.Value;

            MidiOut midiOut = null;
            if (midiOutDevice == null) return nEvt.Next;

            if (trk.OutDevice < midiOutDevice.Length)
                midiOut = midiOutDevice[trk.OutDevice];
            int? channel = trk.OutChannel;

            if (evt.Type == MIDIEventType.NoteON)
            {
                if (channel != null)
                {
                    MIDINoteEvent nevt = (MIDINoteEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status = 0x90 | ch; // NoteOn
                    int msg = status
                              + (nevt.KeyNumber << 8)
                              + (nevt.Vel << 16);

                    if (dicNoteOnTable.Length > trk.OutDevice
                        && !dicNoteOnTable[trk.OutDevice][ch].ContainsKey(nevt.KeyNumber)
                        && !trk.Mute
                        && nevt.GT>0)
                        midiOut?.Send(msg);

                    // Gate（持続時間）を NoteOff のために記録
                    SetNoteTable(trk.OutDevice, ch, nevt.KeyNumber, nevt.GT);
                }
            }
            else if (evt.Type == MIDIEventType.NoteOff)
            {
                if (channel != null)
                {
                    MIDINoteEvent nevt = (MIDINoteEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status = 0x80 | ch; // NoteOff
                    int msg = status
                              + (nevt.KeyNumber << 8)
                              + (nevt.Vel << 16);

                    midiOut?.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.MetaTempo)
            {
                MIDITempoEvent tevt = (MIDITempoEvent)evt;

                double bpm = tevt.TempoValue;

                _bpm = bpm;   // ← UpdateTimer() に反映される

                // デバッグ用
                Console.WriteLine($"Tempo Change: {bpm} BPM");
            }
            else if (evt.Type == MIDIEventType.ControlChange)
            {
                if (channel != null)
                {
                    MIDIControlChangeEvent cevt = (MIDIControlChangeEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status = 0xB0 | ch; // CC
                    int msg = status
                              + (cevt.ControllerNumber << 8)
                              + (cevt.ControllerValue << 16);

                    midiOut?.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.ProgramChange)
            {
                if (channel != null)
                {
                    MIDIProgramChangeEvent cevt = (MIDIProgramChangeEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status = 0xC0 | ch; // ProgramChange
                    int msg = status
                              + (cevt.ProgramNumber << 8);

                    midiOut?.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.PitchBend)
            {
                if (channel != null)
                {
                    MIDIPitchBendEvent pevt = (MIDIPitchBendEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int raw = pevt.PitchValue;   // 0〜16383

                    int lsb = raw & 0x7F;
                    int msb = (raw >> 7) & 0x7F;

                    int status = 0xE0 | ch; // PitchBend
                    int msg = status
                              + (lsb << 8)
                              + (msb << 16);

                    midiOut?.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.KeyAfterTouch)
            {
                if (channel != null)
                {
                    MIDIPolyphonicKeyPressureEvent aevt = (MIDIPolyphonicKeyPressureEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status = 0xA0 | ch; // Polyphonic Key Pressure
                    int msg = status
                              + (aevt.KeyNumber << 8)
                              + (aevt.PressureValue << 16);

                    midiOut?.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.ChannelAfterTouch)
            {
                if (channel != null)
                {
                    MIDIChannelPressureEvent cevt = (MIDIChannelPressureEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status = 0xD0 | ch; // Channel Pressure
                    int msg = status
                              + (cevt.PressureValue << 8); // data2 は使わないので 0

                    midiOut?.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.SysExF0)
            {
                MIDISysExEvent sevt = (MIDISysExEvent)evt;
                List<byte> buf = new List<byte>();

                int j = 0;
                int chksum = 0;

                while (j < sevt.Data.Length)
                {
                    byte? n = sevt.Data[j++];

                    switch (n)
                    {
                        case 0x80:
                            n = sevt.GT;
                            break;
                        case 0x81:
                            n = sevt.VEL;
                            break;
                        case 0x82:
                            n = (byte)trk.OutChannel;
                            break;
                        case 0x83:
                            chksum = 0;
                            n = null;
                            break;
                        case 0x84:
                            n = (byte)(128 - (chksum % 128));
                            break;
                    }

                    if (n != null)
                    {
                        buf.Add((byte)n);
                        chksum += (byte)n;
                    }

                    if (n == 0xf7) break;
                }


                // SysEx はそのまま送る
                midiOut?.SendBuffer(buf.ToArray());
            }
            else if (evt.Type == MIDIEventType.RolandDevice)
            {
                MIDIRolandDeviceEvent sevt = (MIDIRolandDeviceEvent)evt;

                trk.RolandDev_gt = sevt.RolandDev_gt;
                trk.RolandDev_vel = sevt.RolandDev_vel;
            }
            else if (evt.Type == MIDIEventType.RolandBase)
            {
                MIDIRolandBaseEvent sevt = (MIDIRolandBaseEvent)evt;

                trk.RolandBase_gt = sevt.RolandBase_gt;
                trk.RolandBase_vel = sevt.RolandBase_vel;
            }
            else if (evt.Type == MIDIEventType.RolandPara)
            {
                MIDIRolandParaEvent sevt = (MIDIRolandParaEvent)evt;

                trk.RolandPara_gt = sevt.RolandPara_gt;
                trk.RolandPara_vel = sevt.RolandPara_vel;

                RolEx[0] = 0xF0;
                RolEx[1] = 0x41;
                RolEx[2] = trk.RolandDev_gt;
                RolEx[3] = trk.RolandDev_vel;
                RolEx[4] = 0x12;
                RolEx[5] = trk.RolandBase_gt;
                RolEx[6] = trk.RolandBase_vel;
                RolEx[7] = trk.RolandPara_gt;
                RolEx[8] = trk.RolandPara_vel;
                RolEx[9] = (byte)((128 - ((trk.RolandBase_gt + trk.RolandBase_vel + trk.RolandPara_gt + trk.RolandPara_vel) % 128)) & 0x7f);
                RolEx[10] = 0xF7;
                midiOut?.SendBuffer(RolEx);

            }
            else if (evt.Type == MIDIEventType.SameMeas)
            {
                if (trk.SameMeasStack.Count > 0)
                {
                    return trk.SameMeasStack.Pop();
                }

                bool first = true;
                while (evt.Type == MIDIEventType.SameMeas)
                {
                    var sevt = (MIDISameMeasEvent)evt;
                    int target = sevt.RepeatMeas;

                    var key = (trk.Number, prt.Number, target);

                    if (!measureStartNode.TryGetValue(key, out var jumpNode))
                        break;

                    if (jumpNode.Value.Type == MIDIEventType.BarLine)
                    {
                        if (jumpNode.Next.Value.Type == MIDIEventType.SameMeas)
                        {
                            if (first) trk.SameMeasStack.Push(nEvt.Next);
                            first = false;

                            nEvt = jumpNode.Next;
                            evt = jumpNode.Next.Value;
                            continue;
                        }

                        // ★ 戻り位置をスタックに積む（元の位置の次イベント）
                        if (first) trk.SameMeasStack.Push(nEvt.Next);
                        return jumpNode.Next;

                    }

                    if (jumpNode.Value.Type == MIDIEventType.SameMeas)
                    {
                        if (first) trk.SameMeasStack.Push(nEvt.Next);
                        first = false;

                        nEvt = jumpNode;
                        evt = jumpNode.Value;
                        continue;
                    }

                    // ★ 戻り位置をスタックに積む（元の位置の次イベント）
                    if (first) trk.SameMeasStack.Push(nEvt.Next);

                    return jumpNode;

                }
            }
            else if(evt.Type==MIDIEventType.BarLine)
            {
                if (trk.SameMeasStack.Count > 0)
                {
                    //evt.Played = false;
                    return trk.SameMeasStack.Pop();
                }
            }
            else if (evt.Type == MIDIEventType.MetaEndOfTrack)
            {
                if (trk.SameMeasStack.Count > 0)
                {
                    return trk.SameMeasStack.Pop();
                }
            }
            else if(evt.Type==MIDIEventType.LoopStart)
            {
                trk.LoopStack.Push((nEvt.Next, null));
            }
            else if (evt.Type == MIDIEventType.LoopEnd)
            {
                if (trk.LoopStack.Count > 0)
                {
                    var (levt, lcnt) = trk.LoopStack.Pop();
                    MIDILoopEndEvent leEvt = (MIDILoopEndEvent)evt;
                    if (lcnt == null)
                    {
                        lcnt = leEvt.LoopCounter;
                    }
                    lcnt--;
                    if (lcnt > 0)
                    {
                        trk.LoopStack.Push((levt, lcnt));
                        return levt;
                    }
                }
            }
            else if (evt.Type == MIDIEventType.BankProgram)
            {
                if (channel != null)
                {
                    MIDIBankProgramEvent bevt = (MIDIBankProgramEvent)evt;
                    int ch = (int)(channel & 0x0F);

                    int status;
                    int msg;

                    status = 0xB0 | ch; // CC
                    msg = status
                              + (0 << 8)
                              + (bevt.ControlChange << 16);

                    midiOut?.Send(msg);

                    status = 0xC0 | ch; // ProgramChange
                    msg = status
                              + (bevt.ProgramNumber << 8);
                    midiOut?.Send(msg);

                }
            }
            else
            {
                //T.B.D.
            }

            return nEvt.Next;
        }

        private void ResetPlayedFlagsForMeasure(LinkedListNode<MIDIEvent> jumpNode)
        {
            var nEvt = jumpNode;
            do
            {
                nEvt = nEvt.Next;
            } while (nEvt.Value.Type != MIDIEventType.BarLine
            && nEvt.Value.Type != MIDIEventType.SameMeas);
        }


        private void SetNoteTable(int device, int channel, int key, long gate)
        {
            if (device >= dicNoteOnTable.Length) return;

            if (dicNoteOnTable[device][channel].ContainsKey(key))
            {
                dicNoteOnTable[device][channel][key] = gate;
                return;
            }

            dicNoteOnTable[device][channel].Add(key, gate);
        }

        private void ProcessNoteOff()
        {
            if (dicNoteOnTable == null) return;
            if (midiOutDevice == null) return;

            for (int device = 0; device < dicNoteOnTable.Length; device++)
            {
                for (int ch = 0; ch < dicNoteOnTable[device].Length; ch++)
                {
                    var table = dicNoteOnTable[device][ch];
                    var keysToRemove = new List<int>();

                    foreach (var kv in table)
                    {
                        int key = kv.Key;
                        long gateRemaining = kv.Value - 1; // 1frame 経過

                        if (gateRemaining <= 0)
                        {
                            // NoteOff を送る
                            if (midiOutDevice.Length > device)
                            {
                                MidiOut midiOut = midiOutDevice[device];
                                int status = 0x80 | ch;
                                int msg = status + (key << 8);

                                midiOut?.Send(msg);
                            }

                            keysToRemove.Add(key);
                        }
                        else
                        {
                            // 残りGateを更新
                            table[key] = gateRemaining;
                        }
                    }

                    // Gateが0になったノートを削除
                    foreach (int key in keysToRemove)
                        table.Remove(key);
                }
            }
        }

        internal int GetTempo()
        {
            if (_project == null || _project.Information == null) return -1;
            return (int)_bpm;
        }

        internal int GetBaseTempo()
        {
            if (_project == null || _project.Information == null) return -1;
            return _project.Information.Tempo;
        }

        internal int GetBeatDen()
        {
            if (_project == null || _project.Information == null) return -1;
            return _den;
        }

        internal int GetBeatMol()
        {
            if (_project == null || _project.Information == null) return -1;
            return _mol;
        }

        internal long GetTimeMSec()
        {
            if (_project == null || _project.Information == null) return -1;
            // 44100Hz → 秒 → ミリ秒
            double sec = (double)timerCounter / Common.Common.DATA_SEQUENCE_FREQUENCE;
            return (long)(sec * 1000.0);
        }

        internal int GetMeas()
        {
            return _meas;
        }

        internal int GetStep()
        {
            return _step;
        }

        internal int GetTick()
        {
            return _tick;
        }
    }

}