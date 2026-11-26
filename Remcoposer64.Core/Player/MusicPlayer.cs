using NAudio.Midi;
using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.Core.Player
{
    public class MusicPlayer : IPlayerChannel
    {
        private Setting _setting;
        private Setting.midiOutInfo[] _midiOutInfo;
        private Setting.midiInInfo[] _midiInInfo;
        private ThrottledMidiOut[] midiOutDevice;
        private MidiIn[] midiInDevice;

        // --- 状態 ---
        public int FireFrameProcess { get; set; } = 0;
        public bool IsFinished { get; set; } = true;

        // --- Tempo / Timing ---
        protected int _ppq = 480;               // デフォルトPPQ（後でプロジェクトから取得）
        protected double _bpm = 120;               // 人間が扱うテンポ
        protected bool RelativeTempoChangeSW = false;
        protected double RelativeTempoChangeTargetTempo = 120.0;
        public double RelativeTempoChangeTickSlice = 1.0;

        protected int _meas = 0;
        protected int _step = 0;
        protected int _tick = 0;
        protected int _den = 4;
        protected int _mol = 4;

        private double _currentPpq = 0;       // 現在のPPQ

        protected MIDIProject _project = null;
        // フレーム(Tick)刻みのカウンター
        public long frameCounter;
        // 44100Hz刻みのカウンター
        public long timerCounter = 0;
        public double timerCounterD = 0.0;
        protected int[][][] aryNoteOnTable = null;

        private byte[] RolEx = new byte[11];

        private Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode = new();
        protected bool hasSolo = false;
        public long SkipFrameCount;
        protected int SkipModeType = 2;

        public virtual void Initialize(
            Setting setting,
            Setting.midiOutInfo[] midiOutInfo, ThrottledMidiOut[] midiOutDevice,
            Setting.midiInInfo[] midiInInfo, MidiIn[] midiInDevice)
        {
            FireFrameProcess = 0;
            IsFinished = true;

            _setting = setting;
            _midiOutInfo = midiOutInfo;
            _midiInInfo = midiInInfo;

            this.midiOutDevice = midiOutDevice;
            this.midiInDevice = midiInDevice;

            _currentPpq = 0;
            RelativeTempoChangeSW = false;
        }

        public void PlayMusic(MIDIProject project, MarkerPoint mp)
        {
            if (project == null) return;

            _project = project;
            _bpm = project.Information.Tempo;
            _ppq = project.Information.TimeBase;

            aryNoteOnTable = new int[project.Information.MaxLengthMIDIOut][][];
            for (int i = 0; i < project.Information.MaxLengthMIDIOut; i++)
            {
                aryNoteOnTable[i] = new int[16][];
                for (int j = 0; j < 16; j++)
                {
                    aryNoteOnTable[i][j] = new int[128];
                }
            }

            if (mp != null) SkipFrameCount = mp.Tick;
            else SkipFrameCount = -1;

            // 再生開始
            StartPlayback();
        }

        public void StartPlayback()
        {
            SkipModeType = _setting.player.SkipModeType;

            _currentPpq = 0;

            Remcoposer64.ProjectData.Common.ClearPlayedFlag(_project);

            int minSt = 0;
            foreach (var trk in _project.Track)
            {
                minSt = Math.Min(minSt, trk.St);
            }

            frameCounter = minSt;
            timerCounter = 0;
            timerCounterD = 0;
            _meas = 0;
            _step = 0;
            _tick = minSt;
            _den = _project.Information.BeatDen; //初期値をセットする
            _mol = _project.Information.BeatNum; //初期値をセットする


            InitMIDI();
            FireFrameProcess = 0;
            if (SkipFrameCount > 0 && SkipModeType == 0)
            {
                SkipModeONMIDI();
                FireFrameProcess = (int)SkipFrameCount;
            }
            else
                SkipModeOFFMIDI();

            IsFinished = false;

            measureStartNode.Clear();

            Remcoposer64.ProjectData.Common.BuildMeasureStartNode(_project, measureStartNode);
        }


        public void Stop()
        {
            FireFrameProcess = 0;
            IsFinished = true;
        }

        public void UpdateTimer()
        {
            double spd = _setting.player.playSpeed;
            if (SkipFrameCount > 0)
            {
                //spd = 100.0;
                FireFrameProcess++;

                if (SkipModeType == 2)
                {
                    foreach (var midiout in midiOutDevice)
                    {
                        midiout.Flush2();
                    }
                }

                return;
            }

            timerCounterD += _setting.player.playSpeed;
            while (timerCounterD >= 1.0)
            {
                timerCounter++;
                timerCounterD -= 1.0;
            }

            // 1frameあたりのppq進行量
            double ppqPerFrame = (_bpm * _ppq) / 60.0 / Common.Common.DATA_SEQUENCE_FREQUENCE * spd;
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

        public virtual void ProcessFrame(long seqCounter)
        {
            if (SkipFrameCount > 0)
            {
                SkipFrameCount--;
                if (SkipFrameCount == 0 && SkipModeType == 0) FlushMIDI();
            }

            hasSolo = _project.Track.Any(t => t.Solo);

            UpdateTempo();
            bool zanGT = ProcessNoteOff();
            UpdateTickStepMeas();

            LinkedListNode<MIDITrack> nTrk = _project.Track.First;
            bool playedAllTracks = true;
            while (nTrk != null)
            {
                ProcessFrameTrack(nTrk, ref playedAllTracks);
                nTrk = nTrk.Next;
            }

            if (playedAllTracks && !zanGT)
            {
                _project.Information.Played = true;
                IsFinished = true;

                // 終了時のチェック
                foreach (var trk in _project.Track)
                {
                    if (trk.LoopStack.Count > 0)
                        Log.Write(LogLevel.Warning, $"Track:{trk.Number + 1} An inconsistency has occurred with the loop command.");
                }

            }

            frameCounter++;
        }

        protected virtual void ProcessFrameTrack(LinkedListNode<MIDITrack> nTrk, ref bool playedAllTracks)
        {
            MIDITrack trk = nTrk.Value;
            if (trk.Played)
            {
                return;
            }

            playedAllTracks = false;

            LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
            bool playedAllParts = true;
            while (nPrt != null)
            {
                trk.CurrentPart = nPrt;
                if (!ProcessFramePart(trk, nPrt, ref playedAllParts)) break;
                nPrt = nPrt.Next;
            }
            if (playedAllParts) trk.Played = true;

        }

        protected virtual bool ProcessFramePart(MIDITrack trk, LinkedListNode<MIDIPart> nPrt, ref bool playedAllParts)
        {
            MIDIPart prt = nPrt.Value;

            // 初期化（イベント開始前）
            if (prt.CurrentEventFrame == -1)
            {
                prt.CurrentEventFrame = prt.StartTick;
                prt.CurrentEvent = prt.Event.First;
            }
            if (prt.CurrentEvent == null) return true;

            playedAllParts = false;

            if (frameCounter < prt.CurrentEventFrame + trk.St) return false;
            while (prt.CurrentEvent != null)
            {
                MIDIEvent evt = prt.CurrentEvent.Value;

                if (frameCounter < prt.CurrentEventFrame + trk.St)
                    return false;

                prt.CurrentEvent = PlayEvent(trk, nPrt, prt.CurrentEvent);
                prt.CurrentEventFrame += evt.ST;
            }

            return true;
        }

        protected void UpdateTempo()
        {
            if (!RelativeTempoChangeSW) return;

            double RelativeTempoChangeNowTempo = _bpm;
            RelativeTempoChangeNowTempo += RelativeTempoChangeTickSlice * 1;// Tick.Sabun;
            if (
                (RelativeTempoChangeTickSlice > 0.0 && RelativeTempoChangeNowTempo >= RelativeTempoChangeTargetTempo)
                || (RelativeTempoChangeTickSlice < 0.0 && RelativeTempoChangeNowTempo <= RelativeTempoChangeTargetTempo)
                )
            {
                RelativeTempoChangeSW = false;
                RelativeTempoChangeNowTempo = RelativeTempoChangeTargetTempo;
            }

            _bpm = RelativeTempoChangeNowTempo;
        }

        protected void UpdateTickStepMeas()
        {
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
        }

        private void InitMIDI()
        {
            if (midiOutDevice == null) return;
            foreach (var midiout in midiOutDevice)
            {
                midiout?.Init();
            }
        }

        private void SkipModeONMIDI()
        {
            if (midiOutDevice == null) return;
            foreach (var midiout in midiOutDevice)
            {
                midiout?.SkipModeOn(_setting.player.QuickMeasure);
            }
        }
        protected void SkipModeOFFMIDI()
        {
            if (midiOutDevice == null) return;
            foreach (var midiout in midiOutDevice)
            {
                midiout?.SkipModeOff();
            }
        }

        protected void FlushMIDI()
        {
            int maxCount = 0;
            foreach (var midiout in midiOutDevice)
            {
                int n = 0;
                n = (midiout == null ? 0 : midiout.FlushMIDI());
                maxCount = Math.Max(n, maxCount);
            }
            Thread.Sleep(maxCount);// * 5);
        }

        protected void FlushMIDI(int outDevice,int outChannel)
        {
            if (midiOutDevice == null) return;
            int maxCount = 0;
            ThrottledMidiOut midiout = null;
            midiout = outDevice < midiOutDevice.Length ? midiOutDevice[outDevice] : null;
            int n = 0;
            n = (midiout == null ? 0 : midiout.FlushMIDI(outChannel));
            maxCount = Math.Max(n, maxCount);

            Thread.Sleep(maxCount);// * 5);
            //Thread.Sleep(1000+maxCount * 3);
        }

        protected LinkedListNode<MIDIEvent> PlayEvent(MIDITrack trk, LinkedListNode<MIDIPart> nPrt, LinkedListNode<MIDIEvent> nEvt)
        {
            MIDIPart prt = nPrt.Value;
            MIDIEvent evt = nEvt.Value;

            ThrottledMidiOut midiOut = null;
            if (midiOutDevice == null) return nEvt.Next;
            if (trk.OutDevice < 0 || trk.OutDevice >= midiOutDevice.Length) return nEvt.Next;

            midiOut = midiOutDevice[trk.OutDevice];

            int? channel = trk.OutChannel;
            int transpose = trk.Key ?? 0;
            bool send1 = hasSolo ? trk.Solo : !trk.Mute;
            bool send2 = true;
            if (_setting.player.RCPMuteMode == true)
            {
                send2 = hasSolo ? trk.Solo : !trk.Mute;
            }

            switch (evt.Type)
            {
                case MIDIEventType.NoteON:
                    {
                        if (channel != null)
                        {
                            MIDINoteEvent nevt = (MIDINoteEvent)evt;
                            int ch = (int)(channel & 0x0F);

                            int status = 0x90 | ch;
                            byte key = (byte)Math.Clamp((nevt.KeyNumber + transpose), 0, 127);

                            int msg = status
                                + (key << 8)
                                + (nevt.Vel << 16);

                            if (aryNoteOnTable.Length > trk.OutDevice
                                && aryNoteOnTable[trk.OutDevice][ch][key] == 0
                                && send1
                                && nevt.GT > 0
                                && SkipFrameCount < 1)
                            {
                                midiOut?.Send(msg);
                            }

                            if (nevt.GT != 0 && nevt.Vel != 0)
                                SetNoteTable(trk.OutDevice, ch, key, nevt.GT);
                        }
                        break;
                    }

                case MIDIEventType.NoteOff:
                    {
                        if (channel != null)
                        {
                            MIDINoteEvent nevt = (MIDINoteEvent)evt;
                            int ch = (int)(channel & 0x0F);

                            int status = 0x80 | ch;

                            byte key = (byte)Math.Clamp(
                                nevt.KeyNumber + transpose,
                                0,
                                127);

                            int msg = status
                                + (key << 8)
                                + (nevt.Vel << 16);

                            if (send2)
                                midiOut?.Send(msg);
                        }
                        break;
                    }

                case MIDIEventType.MetaTempo:
                    {
                        MIDITempoEvent tevt = (MIDITempoEvent)evt;

                        if (!tevt.RelativeTempoChange)
                        {
                            double bpm = tevt.TempoValue;
                            _bpm = bpm;

                            Common.Log.Write(
                                LogLevel.Debug,
                                $"Tempo Change: {bpm} BPM");
                        }
                        else
                        {
                            int Tempo = (int)(_bpm * tevt.TempoValue);

                            if (Tempo < 10)
                                Tempo = 10;

                            double s =
                                (Tempo - _bpm)
                                * 256.0
                                / ((256.0 - tevt.RelativeTempoChangeValue) * _ppq);

                            RelativeTempoChangeTargetTempo = Tempo;
                            RelativeTempoChangeTickSlice = (_bpm < Tempo)
                                ? s
                                : -s;

                            RelativeTempoChangeSW = true;
                            Common.Log.Write(LogLevel.Debug, $"Tempo Change ( Relative ) : {Tempo} BPM   TickSlice : {RelativeTempoChangeTickSlice}");
                        }

                        break;
                    }

                case MIDIEventType.ControlChange:
                    {
                        if (channel != null)
                        {
                            MIDIControlChangeEvent cevt = (MIDIControlChangeEvent)evt;
                            int ch = (int)(channel & 0x0F);

                            int status = 0xB0 | ch; // CC
                            int msg = status
                                      + (cevt.ControllerNumber << 8)
                                      + (cevt.ControllerValue << 16);

                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }
                        }
                        break;
                    }

                case MIDIEventType.ProgramChange:
                    {
                        if (channel != null)
                        {
                            MIDIProgramChangeEvent cevt = (MIDIProgramChangeEvent)evt;
                            int ch = (int)(channel & 0x0F);

                            int status = 0xC0 | ch; // ProgramChange
                            int msg = status
                                      + (cevt.ProgramNumber << 8);

                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }
                        }
                        break;
                    }

                case MIDIEventType.PitchBend:
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

                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }
                        }
                        break;
                    }

                case MIDIEventType.KeyAfterTouch:
                    {
                        if (channel != null)
                        {
                            MIDIPolyphonicKeyPressureEvent aevt = (MIDIPolyphonicKeyPressureEvent)evt;
                            int ch = (int)(channel & 0x0F);

                            int status = 0xA0 | ch; // Polyphonic Key Pressure
                            int msg = status
                                      + (aevt.KeyNumber << 8)
                                      + (aevt.PressureValue << 16);

                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }
                        }
                        break;
                    }

                case MIDIEventType.ChannelAfterTouch:
                    {
                        if (channel != null)
                        {
                            MIDIChannelPressureEvent cevt = (MIDIChannelPressureEvent)evt;
                            int ch = (int)(channel & 0x0F);

                            int status = 0xD0 | ch; // Channel Pressure
                            int msg = status
                                      + (cevt.PressureValue << 8); // data2 は使わないので 0

                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }
                        }
                        break;
                    }

                case MIDIEventType.SysExF0:
                    {
                        MIDISysExEvent sevt = (MIDISysExEvent)evt;
                        if (send2) SendSysEx(midiOut, sevt.Data, sevt.GT, sevt.VEL, channel);
                        break;
                    }

                case MIDIEventType.RolandDevice:
                    {
                        MIDIRolandDeviceEvent sevt = (MIDIRolandDeviceEvent)evt;

                        trk.RolandDev_gt = sevt.RolandDev_gt;
                        trk.RolandDev_vel = sevt.RolandDev_vel;
                        break;
                    }

                case MIDIEventType.RolandBase:
                    {
                        MIDIRolandBaseEvent sevt = (MIDIRolandBaseEvent)evt;

                        trk.RolandBase_gt = sevt.RolandBase_gt;
                        trk.RolandBase_vel = sevt.RolandBase_vel;
                        break;
                    }

                case MIDIEventType.RolandPara:
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
                        if (send2)
                        {
                            midiOut?.SendSysEx(RolEx);
                        }
                        break;
                    }

                case MIDIEventType.SameMeas:
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
                                return nEvt.Next;
                            }

                            if (jumpNode.Value.Type == MIDIEventType.SameMeas)
                            {
                                if (nEvt == jumpNode)
                                {
                                    Log.Write(
                                        LogLevel.Error,
                                        $"MusicPlayer.cs: same measureEvent: Self-reference Case 4 : Part Name:{prt.Name} Event:{((MIDISameMeasEvent)nEvt.Value).RepeatMeas}");

                                    break;
                                }

                                if (first)
                                    trk.SameMeasStack.Push(nEvt.Next);

                                first = false;

                                nEvt = jumpNode;
                                evt = jumpNode.Value;
                                continue;
                            }

                            if (first)
                                trk.SameMeasStack.Push(nEvt.Next);

                            return jumpNode;
                        }

                        break;
                    }
                case MIDIEventType.BarLine:
                    {
                        if (trk.SameMeasStack.Count > 0)
                        {
                            return trk.SameMeasStack.Pop();
                        }

                        //MIDIBarLineEvent ble = (MIDIBarLineEvent)evt;
                        //if (ble.BeatNum != 0 || ble.BeatDen != 0)
                        //{
                        //    _mol = ble.BeatNum;
                        //    _den = ble.BeatDen;
                        //}

                        break;
                    }
                case MIDIEventType.MetaEndOfTrack:
                    {
                        if (trk.SameMeasStack.Count > 0)
                        {
                            return trk.SameMeasStack.Pop();
                        }
                        break;
                    }

                case MIDIEventType.LoopStart:
                    {
                        trk.LoopStack.Push((nEvt.Next, null));
                        break;
                    }

                case MIDIEventType.LoopEnd:
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
                        else
                        {
                            Log.Write(LogLevel.Warning, $"Track:{trk.Number + 1} An inconsistency has occurred with the loop command.");
                        }
                        break;
                    }

                case MIDIEventType.BankProgram:
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

                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }

                            status = 0xC0 | ch; // ProgramChange
                            msg = status
                                      + (bevt.ProgramNumber << 8);
                            if (send2)
                            {
                                midiOut?.Send(msg);
                            }

                        }
                        break;
                    }

                case MIDIEventType.Memo:
                    {
                        trk.Memo = ((MIDIMemoEvent)evt).Text;
                        trk.MemoWait = 60;
                        Common.Log.Write(LogLevel.Information, $"Track {trk.Number + 1}:Memo:{((MIDIMemoEvent)evt).Text}");
                        break;
                    }

                case MIDIEventType.MetaLyric:
                    {
                        //trk.Memo = ((MIDIMemoEvent)evt).Text;
                        //trk.MemoWait = 60;
                        //Common.Log.Write(LogLevel.Information, $"Track {trk.Number + 1}:Memo:{((MIDIMemoEvent)evt).Text}");
                        break;
                    }

                case MIDIEventType.MetaMarker:
                    {
                        ;//演奏中のMetaMarkerは 何もしない
                        break;
                    }

                case MIDIEventType.UserExclusive1:
                    {
                        MIDIUsrExEvent sevt = (MIDIUsrExEvent)evt;
                        byte[] dat = _project.UserExclusive[sevt.MIDIMessage[0] - 0x90].Exclusive;
                        if (send2) SendSysEx(midiOut, dat, sevt.GT, sevt.VEL, channel);
                        break;
                    }

                case MIDIEventType.MetaPortPrefix:
                    {
                        MIDIPortPrefixEvent pfe = (MIDIPortPrefixEvent)evt;
                        trk.OutDevice = pfe.PortNumber;
                        break;
                    }

                case MIDIEventType.MetaSequencerSpecific:
                    {
                        //参考URL: https://moondoldo.com/wordpress/?p=14483#%E8%B5%B7%E5%8B%95%E3%81%A8%E8%A8%AD%E5%AE%9A
                        if (evt.MIDIMessage.Length > 5
                            && evt.MIDIMessage[1] == 0x04
                            && evt.MIDIMessage[2] == 0x43
                            && evt.MIDIMessage[3] == 0x00
                            && evt.MIDIMessage[4] == 0x01)
                        {
                            //YAMAHA独自のMIDIポート指定
                            trk.OutDevice = evt.MIDIMessage[5];
                        }
                        else
                        {
                            Common.Log.Write(LogLevel.Warning, $" MusicPlayer.cs : Unknown MetaSequencerSpecific");
                        }
                        break;
                    }

                default:
                    {
                        Common.Log.Write(
                            LogLevel.Warning,
                            $" MusicPlayer.cs : TBD {evt.Type}");
                        break;
                    }
            }

            return nEvt.Next;

        }

        private void SendSysEx(ThrottledMidiOut midiOut, byte[] dat, byte GT, byte Vel, int? trkOutChannel)
        {
            List<byte> buf = new List<byte>();

            int j = 0;
            int chksum = 0;
            bool err = false;

            while (j < dat.Length)
            {
                byte? n = dat[j++];

                switch (n)
                {
                    case 0x80:
                        n = GT;
                        break;
                    case 0x81:
                        n = Vel;
                        break;
                    case 0x82:
                        // OutChannelが指定されていないトラックにも関わらず0x82が指定されているExはエラーとして送信しない
                        if (trkOutChannel == null)
                        {
                            Log.Write(LogLevel.Error, "A SysEx with the value 0x82 was detected, even though no OutChannel was specified for that track; therefore, it was not sent.");
                            err = true;
                        }
                        else
                            n = (byte)trkOutChannel;
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
            if (!err)
            {
                byte[] ary = buf.ToArray();
                midiOut?.SendSysEx(ary);
            }

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
            if (device >= aryNoteOnTable.Length) return;

            aryNoteOnTable[device][channel][key] = (int)gate;
        }

        private const int MidiNoteCount = 128;

        protected bool ProcessNoteOff()
        {
            if (aryNoteOnTable == null) return false;
            if (midiOutDevice == null) return false;

            bool ret = false;

            for (int device = 0; device < aryNoteOnTable.Length; device++)
            {
                if (device >= midiOutDevice.Length)
                    continue;

                var midiOut = midiOutDevice[device];
                if (midiOut == null)
                    continue;

                var deviceTable = aryNoteOnTable[device];

                for (int ch = 0; ch < deviceTable.Length; ch++)
                {
                    var table = deviceTable[ch];
                    int status = 0x80 | ch;

                    for (int note = 0; note < MidiNoteCount; note++)
                    {
                        int gate = table[note];
                        if (gate <= 0)
                            continue;

                        gate--;
                        table[note] = gate;

                        if (gate > 0)
                        {
                            ret = true;
                            continue;
                        }

                        midiOut.Send(status | (note << 8));
                    }
                }
            }

            return ret;
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

        internal long GetFrameCounter()
        {
            return frameCounter;
        }

        public virtual RmPlayer.PlayerState GetState()
        {
            return IsFinished ? RmPlayer.PlayerState.Stopped : RmPlayer.PlayerState.Processing;
        }

        internal MIDIProject GetPlayingProject()
        {
            return _project;
        }

        internal void SetFirstFrame()
        {
            frameCounter = 0;
            timerCounter = 0;
            timerCounterD = 0;
            _meas = 0;
            _step = 0;
            _tick = 0;
            Remcoposer64.ProjectData.Common.ClearPlayedFlag(_project);
        }
    }

}