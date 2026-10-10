using NAudio.Midi;
using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.Core.Player
{
    public class MusicPlayer : IPlayerChannel
    {
        private Setting.midiOutInfo[] _midiOutInfo;
        private Setting.midiInInfo[] _midiInInfo;

        // --- Tempo / Timing ---
        private int _ppq = 480;               // デフォルトPPQ（後でプロジェクトから取得）
        private double _bpm = 120;               // 人間が扱うテンポ
        private double _currentPpq = 0;       // 現在のPPQ

        // --- 状態 ---
        public int FireFrameProcess { get; set; } = 0;
        public bool IsFinished { get; set; } = true;

        private MIDIProject _project = null;
        private long frameCounter;
        private Dictionary<int, long>[][] dicNoteOnTable = null;
        private NAudio.Midi.MidiOut[] midiOutDevice;
        private NAudio.Midi.MidiIn[] midiInDevice;

        private byte[] RolEx = new byte[11];

        public void Initialize(Setting.midiOutInfo[] midiOut, MidiOut[] midiOutDevice, Setting.midiInInfo[] midiIn, MidiIn[] midiInDevice)
        {
            _midiOutInfo = midiOut;
            _midiInInfo = midiIn;

            _currentPpq = 0;

            FireFrameProcess = 0;
            IsFinished = true;

            dicNoteOnTable = new Dictionary<int, long>[_midiOutInfo.Length][];
            for(int i = 0; i < _midiOutInfo.Length; i++)
            {
                dicNoteOnTable[i] = new Dictionary<int, long>[16];
                for (int j = 0; j < 16; j++)
                {
                    dicNoteOnTable[i][j] = new Dictionary<int, long>();
                }
            }

            this.midiOutDevice = midiOutDevice;
            this.midiInDevice = midiInDevice;
        }
        
        public void PlayMusic(MIDIProject project)
        {
            if (project == null) return;

            _project = project;
            _bpm = project.Information.Tempo;
            _ppq = project.Information.TimeBase;

            // 再生開始
            StartPlayback();
        }

        public void StartPlayback()
        {
            _currentPpq = 0;

            ClearPlayedFlag();
            frameCounter = 0;
            FireFrameProcess = 0;
            IsFinished = false;
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
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                while (nPrt != null)
                {
                    nPrt.Value.Played = false;
                    LinkedListNode<MIDIEvent> nEvt = nPrt.Value.Event.First;
                    while (nEvt != null)
                    {
                        nEvt.Value.Played = false;
                        nEvt = nEvt.Next;
                    }
                    nPrt = nPrt.Next;
                }
                nTrk = nTrk.Next;
            }
        }


        public void UpdateTimer()
        {
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

                    if (prt.Played)
                    {
                        nPrt = nPrt.Next;
                        continue;
                    }

                    playedAllParts = false;
                    long partStartFrame = prt.StartTick;

                    if (frameCounter < partStartFrame)
                        break;

                    LinkedListNode<MIDIEvent> nEvt = nPrt.Value.Event.First;
                    bool playedAllEvents = true;

                    long eventStartFrame = prt.StartTick;
                    while (nEvt != null)
                    {
                        MIDIEvent evt = nEvt.Value;

                        if (frameCounter < eventStartFrame)
                        {
                            playedAllEvents = false;
                            break;
                        }

                        if (!evt.Played)
                        {
                            //T.B.D.
                            PlayEvent(trk, prt, evt);
                            evt.Played = true;
                        }

                        eventStartFrame += evt.ST;
                        nEvt = nEvt.Next;
                    }

                    if (playedAllEvents)
                        prt.Played = true;

                    nPrt = nPrt.Next;
                }

                if (playedAllParts)
                    trk.Played = true;

                nTrk = nTrk.Next;
            }

            if (playedAllTracks)
                _project.Information.Played = true;

            ProcessNoteOff();
            frameCounter++;

        }

        private void PlayEvent(MIDITrack trk, MIDIPart prt, MIDIEvent evt)
        {
            MidiOut midiOut = null;
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

                    if (!dicNoteOnTable[trk.OutDevice][ch].ContainsKey(nevt.KeyNumber)
                        && !trk.Mute) 
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

                    midiOut.Send(msg);
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

                    midiOut.Send(msg);
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

                    midiOut.Send(msg);
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

                    midiOut.Send(msg);
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

                    midiOut.Send(msg);
                }
            }
            else if (evt.Type == MIDIEventType.SysExF0)
            {
                MIDISysExEvent sevt = (MIDISysExEvent)evt;

                // SysEx はそのまま送る
                midiOut?.SendBuffer(sevt.Data);
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
            else
            {
                //T.B.D.
            }
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
                            MidiOut midiOut = midiOutDevice[device];
                            int status = 0x80 | ch;
                            int msg = status + (key << 8);

                            midiOut?.Send(msg);

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

    }

}