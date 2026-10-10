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
            _midiOut = setting.midiOut;
            currentMidiOut = _midiOut.lstMidiOutInfo[_midiOut.CurrentDev];
            _midiIn = setting.midiIn;
            currentMidiIn = _midiIn.lstMidiInInfo[_midiIn.CurrentDev];
            State = PlayerState.Stopped;

            midiOutDev = new NAudio.Midi.MidiOut[currentMidiOut.Length];
            for (int i = 0; i < currentMidiOut.Length; i++)
            {
                try
                {
                    midiOutDev[i] = new NAudio.Midi.MidiOut(currentMidiOut[i].id);
                }
                catch
                {
                    midiOutDev[i] = null;
                }
            }
            midiInDev = new NAudio.Midi.MidiIn[currentMidiIn.Length];
            for (int i = 0; i < currentMidiIn.Length; i++)
            {
                try
                {
                    midiInDev[i] = new NAudio.Midi.MidiIn(currentMidiIn[i].id);
                }catch
                {
                    midiInDev[i] = null;
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
                    midiOut.Close();
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
                    try { midiOut?.Close(); } catch { }
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

            for (int i = 0; i < midiOutDev.Length; i++)
            {
                midiOutDev[i].Dispose();
            }
            for (int i = 0; i < midiInDev.Length; i++)
            {
                midiInDev[i].Dispose();
            }

            _timer.Dispose();
        }
    }
}
