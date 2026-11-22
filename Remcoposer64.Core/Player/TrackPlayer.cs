using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using static Remcoposer64.Common.Setting;
using Log = Remcoposer64.Common.Log;

namespace Remcoposer64.Core.Player
{
    public class TrackPlayer : MusicPlayer
    {
        public class MeasurePoint
        {
            public int Tick;
            public LinkedListNode<MIDIEvent> Event;
        }

        private LinkedListNode<MIDITrack> targetTrack;
        private MeasurePoint measStartPoint;
        private MeasurePoint measEndPoint;

        public override void Initialize(
            Setting setting,
            midiOutInfo[] midiOutInfo, ThrottledMidiOut[] midiOutDevice,
            midiInInfo[] midiInInfo, NAudio.Midi.MidiIn[] midiInDevice)
        {
            base.Initialize(setting, midiOutInfo, midiOutDevice, midiInInfo, midiInDevice);
        }

        public void PlayMusic(MIDIProject project, MIDITrack targetTrk, LinkedListNode<MIDIEvent> startEvent, LinkedListNode<MIDIEvent> endEvent)
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

            targetTrack = project.Track.First;
            while (targetTrack != null)
            {
                if (targetTrack.Value == targetTrk) break;
                targetTrack = targetTrack.Next;
            }

            MeasureInfoManager mim = new MeasureInfoManager();
            Dictionary<LinkedListNode<MIDIEvent>, int> ticks = mim.GetTicksFormTrackEvent(project, targetTrack, startEvent, endEvent);
            if (ticks == null || !ticks.ContainsKey(startEvent) || !ticks.ContainsKey(endEvent))
            {
                // 必要なイベントが見つからなかった場合は例外(プログラム的な不具合の場合と考えられるため)
                throw new ArgumentOutOfRangeException();
            }

            measStartPoint = new MeasurePoint()
            {
                Tick = ticks[startEvent],
                Event = startEvent
            };

            measEndPoint = new MeasurePoint()
            {
                Tick = ticks[endEvent],
                Event = endEvent
            };

            SkipFrameCount = measStartPoint.Tick;

            // 再生開始
            StartPlayback();
        }

        public override void ProcessFrame(long seqCounter)
        {
            hasSolo = _project.Track.Any(t => t.Solo);

            UpdateTempo();
            bool zanGT = ProcessNoteOff();
            UpdateTickStepMeas();

            LinkedListNode<MIDITrack> nTrk = targetTrack;
            bool playedAllTracks = true;
            ProcessFrameTrack(nTrk, ref playedAllTracks);

            if (playedAllTracks && !zanGT)
            {
                _project.Information.Played = true;
                IsFinished = true;

                // 終了時のチェック
                if (nTrk.Value.LoopStack.Count > 0)
                    Log.Write(LogLevel.Warning, $"Track:{nTrk.Value.Number + 1} An inconsistency has occurred with the loop command.");

            }

            frameCounter++;
        }

        protected override bool ProcessFramePart(MIDITrack trk, LinkedListNode<MIDIPart> nPrt, ref bool playedAllParts)
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
                // 指定小節迄の演奏スキップ制御及び演奏完了制御
                if (SkipFrameCount > 0)
                {
                    SkipFrameCount--;
                    if (SkipFrameCount < 1) FlushMIDI();// SkipMode解除
                }
                else
                {
                    if (prt.CurrentEvent == measEndPoint?.Event)
                    {
                        trk.Played = true;
                    }
                }

                MIDIEvent evt = prt.CurrentEvent.Value;

                if (frameCounter < prt.CurrentEventFrame + trk.St)
                    return false;

                prt.CurrentEvent = PlayEvent(trk, nPrt, prt.CurrentEvent);
                prt.CurrentEventFrame += evt.ST;

            }

            return true;
        }


    }
}