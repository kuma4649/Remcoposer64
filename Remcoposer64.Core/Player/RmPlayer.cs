using Remcoposer64.Core;
using Remcoposer64.Common;
using Remcoposer64.Core.Timer;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using System.Diagnostics;
using static Remcoposer64.Common.Setting;

namespace Remcoposer64.Core.Player
{
    public class RmPlayer
    {
        private MidiDeviceManager _deviceManager;

        private static MusicPlayer musicPlayer = new();
        private static PreviewPlayer previewPlayer = new();
        private static MidiKeyboardPlayer midiKeyboardPlayer = new();
        private static TrackPlayer trackPlayer = new();

        private readonly List<IPlayerChannel> _channels = new()
        {
            musicPlayer,
            previewPlayer,
            midiKeyboardPlayer,
            trackPlayer
        };

        private Setting setting;
        private Setting.MidiOut _midiOut;
        private Setting.midiOutInfo[] currentMidiOut;
        private Setting.MidiIn _midiIn;
        private Setting.midiInInfo[] currentMidiIn;
        private readonly RmTimer _timer;
        private ThrottledMidiOut[] midiOutDev;
        //private List<MIDIParam> midiParamList = new List<MIDIParam>();
        private NAudio.Midi.MidiIn[] midiInDev;

        public enum PlayerState
        {
            Stopped,
            Processing
        }

        public PlayerState State { get; set; } = PlayerState.Stopped;

        public RmPlayer(Setting setting)
        {
            this.setting = setting;
            _deviceManager = new MidiDeviceManager();

            if (setting.midiOut == null) 
                setting.midiOut = new();
            _midiOut = setting.midiOut;
            if (_midiOut.lstMidiOutInfo == null)
            {
                _midiOut.lstMidiOutInfo = new List<midiOutInfo[]>();
                currentMidiOut = null;
            }
            else
            {
                if (_midiOut.lstMidiOutInfo.Count > _midiOut.CurrentDev)
                    currentMidiOut = _midiOut.lstMidiOutInfo[_midiOut.CurrentDev];
                else
                    currentMidiOut = null;
            }

            if (setting.midiIn == null) 
                setting.midiIn = new();
            _midiIn = setting.midiIn;
            if (_midiIn.lstMidiInInfo == null)
            {
                _midiIn.lstMidiInInfo = new List<midiInInfo[]>();
                currentMidiIn = null;
            }
            else
            {
                if (_midiIn.lstMidiInInfo.Count > _midiIn.CurrentDev)
                    currentMidiIn = _midiIn.lstMidiInInfo[_midiIn.CurrentDev];
            }

            State = PlayerState.Stopped;

            if (currentMidiOut != null && currentMidiOut.Length > 0)
            {
                midiOutDev = new ThrottledMidiOut[currentMidiOut.Length];
                for (int i = 0; i < currentMidiOut.Length; i++)
                {
                    try
                    {
                        midiOutDev[i] = _deviceManager.GetMIDIOutDevice(currentMidiOut[i]);
                    }
                    catch
                    {
                        midiOutDev[i] = null;
                    }
                }
            }

            if (currentMidiIn != null && currentMidiIn.Length > 0)
            {
                midiInDev = new NAudio.Midi.MidiIn[currentMidiIn.Length];
                for (int i = 0; i < currentMidiIn.Length; i++)
                {
                    try
                    {
                        midiInDev[i] = _deviceManager.GetMIDIInDevice(currentMidiIn[i].name);
                    }
                    catch
                    {
                        midiInDev[i] = null;
                    }
                }
            }

            RmTimerContext ctx = new RmTimerContext
            {
                GetStepCounter = () => 0,
                SetStepCounter = v => { },

                IsInterrupted = () => false,
                GetCurrentMode = () => SendMode.RealTime,

                SendFrameData = SendFrameData,
                SendStopFrame = SendStopFrame,

                WaitSync = WaitSync
            };
            ctx.Handler = new RmHandler(ctx, _midiOut, _midiIn, _channels);
            _timer = new RmTimer(ctx);

        }

        public void InitialPlay()
        {
            foreach (var channel in _channels)
                channel.Initialize(setting, currentMidiOut, midiOutDev, currentMidiIn, midiInDev);

            State = PlayerState.Processing;
            _timer.RequestStart();
        }

        public void PlayMusic(MIDIProject project, MarkerPoint mp = null)
        {
            if (project == null) return;

            // MusicPlayer にプロジェクトのイベントを渡す
            SendBeforeReset();

            int maxMIDIOut = 0;
            foreach (var trk in project.Track)
            {
                maxMIDIOut = Math.Max(maxMIDIOut, trk.OutDevice);
            }
            project.Information.MaxLengthMIDIOut = maxMIDIOut + 1;

            musicPlayer.PlayMusic(project, mp);
        }

        public void PlayTrack(MIDIProject project,
            bool isMeasureMode,
            MIDITrack targetTrack,
            LinkedListNode<MIDIEvent> startEvent,
            LinkedListNode<MIDIEvent> endEvent)
        {
            if (project == null) return;

            bool flg = true;
            if (isMeasureMode && setting.player.QuickMeasure) flg = false;
            if(flg) SendBeforeReset();

            // MusicPlayer にプロジェクトのイベントを渡す

            int maxMIDIOut = 0;
            foreach (var trk in project.Track)
            {
                maxMIDIOut = Math.Max(maxMIDIOut, trk.OutDevice);
            }
            project.Information.MaxLengthMIDIOut = maxMIDIOut + 1;

            //MIDITrack trgTrk = project.Track.First.Next.Value;
            //LinkedListNode<MIDIEvent> sEvt = project.Track.First.Next.Value.Part.First.Value.Event.First;
            //LinkedListNode<MIDIEvent> eEvt = sEvt.Next.Next;
            trackPlayer.PlayMusic(project, isMeasureMode, setting.player.QuickMeasure, targetTrack, startEvent, endEvent);
        }

        public void StartPlayback()
        {
            SendBeforeReset();
            musicPlayer.StartPlayback();
        }

        public void StopPlayback(bool sendAllSoundOff=true)
        {
            musicPlayer.Stop();
            trackPlayer.Stop();
            if(sendAllSoundOff) SendAllSoundOff();
        }

        public void FirePreview(List<(int, int, MIDIEvent)> ev)
        {
            previewPlayer.Enqueue(ev);
            previewPlayer.IsFinished = false;
        }

        public void FireMidiKeyboard(List<MIDIEvent> ev)
        {
            //midiKeyboardPlayer.Enqueue(ev);
            //midiKeyboardPlayer.FireFrameProcess = true;
            //midiKeyboardPlayer.IsFinished = false;
        }


        private void SendFrameData()
        {
        }

        private int SendStopFrame()
        {
            SendAllSoundOff();
            Debug.WriteLine("SendStopFrame");
            return 0;
        }

        private void WaitSync()
        {
            Debug.WriteLine("WaitSync");
        }

        public void SendAllSoundOff()
        {
            if(currentMidiOut == null || currentMidiOut.Length < 1) return;
            if (midiOutDev == null) return;
            foreach (var midiOut in midiOutDev)
            {
                if (midiOut == null) continue;
                
                try
                {
                    for (int ch = 0; ch < 16; ch++)
                    {
                        // All Sound Off (CC120)
                        midiOut.Send((0xB0 | ch) | (120 << 8) | (0 << 16));
                        midiOut.Flush2();
                        // All Notes Off (CC123)
                        midiOut.Send((0xB0 | ch) | (123 << 8) | (0 << 16));
                        midiOut.Flush2();
                        // Hold Pedal Off (CC64)
                        midiOut.Send((0xB0 | ch) | (64 << 8) | (0 << 16));
                        midiOut.Flush2();
                        // Reverb 0 (CC91)
                        midiOut.Send((0xB0 | ch) | (91 << 8) | (0 << 16));
                        midiOut.Flush2();
                        // Chorus 0 (CC93)
                        midiOut.Send((0xB0 | ch) | (93 << 8) | (0 << 16));
                        midiOut.Flush2();
                        // Reset All Controllers (CC121)
                        midiOut.Send((0xB0 | ch) | (121 << 8) | (0 << 16));
                        midiOut.Flush2();
                        // Pitch Bend Center (E0 00 40)
                        midiOut.Send((0xE0 | ch) | (0 << 8) | (64 << 16));
                        midiOut.Flush2();
                    }
                }
                catch(NAudio.MmException)
                {
                    ;//この例外は無視する
                }
                catch (Exception ex)
                {
                    Common.Log.ForcedWrite($"Error sending All Sound Off to MIDI Out {midiOut}: {ex.Message}");
                }
                finally
                {
                    //try { midiOut?.Close(); } catch { }
                }
            }
            Thread.Sleep(500);
        }

        public void SendBeforeReset()
        {
            if (currentMidiOut == null || currentMidiOut.Length < 1) return;
            if (midiOutDev == null) return;
            foreach (var midiOut in midiOutDev)
            {
                if (midiOut == null) continue;

                try
                {
                    List<byte> buf = MakeBeforeSendCommand(midiOut.midiOutInfo.beforeSendType);
                    if (buf == null) continue;
                    midiOut.SendSysEx(buf.ToArray());
                    midiOut.Flush2();
                }
                catch (NAudio.MmException)
                {
                    ;//この例外は無視する
                }
                catch (Exception ex)
                {
                    Common.Log.ForcedWrite($"Error SendBeforeReset to MIDI Out {midiOut}: {ex.Message}");
                }
                finally
                {
                }
            }
        }

        public void Close()
        {
            try
            {
                StopPlayback();
            }
            catch { }

            _deviceManager.CloseAll();
            midiOutDev = null;
            midiInDev = null;

            _timer.Dispose();
        }

        public int GetTempo()
        {
            return musicPlayer.GetTempo();
        }

        public int GetBaseTempo()
        {
            return musicPlayer.GetBaseTempo();
        }

        public int GetBeatDen()
        {
            return musicPlayer.GetBeatDen();
        }

        public int GetBeatMol()
        {
            return musicPlayer.GetBeatMol();
        }

        public long GetTimeMSec()
        {
            return musicPlayer.GetTimeMSec();
        }

        public int GetMeas()
        {
            return musicPlayer.GetMeas();
        }

        public int GetStep()
        {
            return musicPlayer.GetStep();
        }

        public int GetTick()
        {
            return musicPlayer.GetTick();
        }

        public long GetFrameCounter()
        {
            return musicPlayer.GetFrameCounter();
        }

        public long GetSkipCounter()
        {
            return musicPlayer.SkipFrameCount;
        }

        public PlayerState GetMusicState()
        {
            return musicPlayer.GetState();
        }

        public PlayerState GetTrackPlayerState()
        {
            return trackPlayer.GetState();
        }

        public MIDIProject GetPlayingProject()
        {
            return musicPlayer.GetPlayingProject();
        }

        public void SetFirstFrame()
        {
            musicPlayer.SetFirstFrame();
        }

        public MIDIParam GetMIDIParam(int devNo)
        {
            if (midiOutDev == null) return null;
            return midiOutDev[devNo]?.midiParam;
        }

        private List<byte> MakeBeforeSendCommand(int beforeSendType)
        {
            try
            {
                List<byte> buf = null;

                switch (beforeSendType)
                {
                    case 0://None
                        break;
                    case 1://GM Reset
                        buf = GetCtlSysexFromText(setting.midiOut.GMReset);
                        break;
                    case 2://XG Reset
                        buf = GetCtlSysexFromText(setting.midiOut.XGReset);
                        break;
                    case 3://GS Reset
                        buf = GetCtlSysexFromText(setting.midiOut.GSReset);
                        break;
                    case 4://Custom
                        buf = GetCtlSysexFromText(setting.midiOut.Custom);
                        break;
                }

                return buf;
            }
            catch
            {
                return null;
            }
        }

        private List<byte> GetCtlSysexFromText(string text)
        {
            if (text == null || text.Length < 1) return null;

            List<byte> buf = new List<byte>();
            string[] cmds = text.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string cmd in cmds)
            {
                byte dat = (byte)Convert.ToInt16(cmd, 16);
                buf.Add(dat);
            }

            return buf;
        }

    }
}
