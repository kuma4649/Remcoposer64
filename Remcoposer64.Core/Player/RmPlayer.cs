using Remcomposer64.Core;
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

        private readonly List<IPlayerChannel> _channels = new()
        {
            musicPlayer,
            previewPlayer,
            midiKeyboardPlayer
        };

        private Setting.MidiOut _midiOut;
        private Setting.midiOutInfo[] currentMidiOut;
        private Setting.MidiIn _midiIn;
        private Setting.midiInInfo[] currentMidiIn;
        private readonly RmTimer _timer;
        private NAudio.Midi.MidiOut[] midiOutDev;
        private NAudio.Midi.MidiIn[] midiInDev;

        public enum PlayerState
        {
            Stopped,
            Processing
        }

        public PlayerState State { get; set; } = PlayerState.Stopped;

        public RmPlayer(Setting setting)
        {
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
                midiOutDev = new NAudio.Midi.MidiOut[currentMidiOut.Length];
                for (int i = 0; i < currentMidiOut.Length; i++)
                {
                    try
                    {
                        midiOutDev[i] = _deviceManager.GetMIDIOutDevice(currentMidiOut[i].name);
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
                channel.Initialize(currentMidiOut, midiOutDev, currentMidiIn, midiInDev);

            State = PlayerState.Processing;
            _timer.RequestStart();
        }

        public void PlayMusic(MIDIProject project)
        {
            if (project == null) return;

            // MusicPlayer にプロジェクトのイベントを渡す
            musicPlayer.PlayMusic(project);

        }

        public void StartPlayback()
        {
            musicPlayer.StartPlayback();
            //musicPlayer.FireFrameProcess = true;
            //musicPlayer.IsFinished = false;
        }

        public void StopPlayback()
        {
            musicPlayer.Stop();
            SendAllSoundOff();
        }

        public void FirePreview(List<MIDIEvent> ev)
        {
            //previewPlayer.Enqueue(ev);
            //previewPlayer.FireFrameProcess = true;
            //previewPlayer.IsFinished = false;
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
                        // All Notes Off (CC123)
                        midiOut.Send((0xB0 | ch) | (123 << 8) | (0 << 16));
                        // Hold Pedal Off (CC64)
                        midiOut.Send((0xB0 | ch) | (64 << 8) | (0 << 16));
                        // Reset All Controllers (CC121)
                        midiOut.Send((0xB0 | ch) | (121 << 8) | (0 << 16));
                        // Pitch Bend Center (E0 00 40)
                        midiOut.Send((0xE0 | ch) | (0 << 8) | (64 << 16));
                    }
                    //midiOut.Close();
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

        public PlayerState GetMusicState()
        {
            return musicPlayer.GetState();
        }

        public MIDIProject GetPlayingProject()
        {
            return musicPlayer.GetPlayingProject();
        }
    }
}
