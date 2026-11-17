using Remcoposer64.Common;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using static Remcoposer64.Common.Setting;

namespace Remcoposer64.Core.Player
{
    public class PreviewPlayer : IPlayerChannel
    {
        private readonly Queue<(int, int, MIDIEvent)> _queue = new();
        private double _bpm = 60;//tempo:60
        private double _ppq = 192;//Reso:192
        private double _currentPpq = 0;       // 現在のPPQ
        private int currentST = 0;

        public int FireFrameProcess { get; set; } = 0;
        public bool IsFinished { get; set; } = true;

        private Dictionary<(int, int, MIDIEvent), int> dicNoteOn = new();
        private Setting setting;
        private Setting.midiOutInfo[] midiOutInfo;
        private ThrottledMidiOut[] midiOut;
        private Setting.midiInInfo[] midiInInfo;
        private NAudio.Midi.MidiIn[] midiIn;


        public void Initialize(Setting setting, Setting.midiOutInfo[] midiOutInfo, ThrottledMidiOut[] midiOut, Setting.midiInInfo[] midiInInfo, NAudio.Midi.MidiIn[] midiIn)
        {
            this.setting = setting;
            this.midiOutInfo = midiOutInfo;
            this.midiOut = midiOut;
            this.midiInInfo=midiInInfo;
            this.midiIn = midiIn;

            _queue.Clear();
            FireFrameProcess = 0;
            IsFinished = false;
        }

        public void Enqueue(List<(int, int, MIDIEvent)> events)
        {
            foreach (var ev in events)
                _queue.Enqueue(ev);

            FireFrameProcess = 1;
            IsFinished = false;
        }

        public void Stop()
        {
            _queue.Clear();
            FireFrameProcess = 0;
            IsFinished = true;
        }


        public void UpdateTimer()
        {

            // 1frameあたりのppq進行量
            double ppqPerFrame = (_bpm * _ppq) / 4 / 60.0 / Common.Common.DATA_SEQUENCE_FREQUENCE;
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
            foreach(var v in dicNoteOn.Keys)
            {
                dicNoteOn[v]--;
                if(dicNoteOn[v] == 0)
                {
                    midiOut[v.Item1].Send(
                        (0x90 + v.Item2)
                        | (((MIDINoteEvent)v.Item3).KeyNumber << 8)
                        | (0 << 16));
                    dicNoteOn.Remove(v);
                }
            }

            currentST--;
            if (currentST > 0) return;
            currentST = 0;

            if (_queue.Count == 0)
            {
                FireFrameProcess = 0;
                return;
            }

            // キューから取り出して発音する
            var ev = _queue.Dequeue();

            if (ev.Item3.Type == MIDIEventType.NoteON)
            {
                MIDINoteEvent ne = (MIDINoteEvent)ev.Item3;
                midiOut[ev.Item1].Send(
                      (0x90 + ev.Item2)
                    | (ne.KeyNumber << 8)
                    | (ne.Vel << 16));
                dicNoteOn[ev] = ne.GT;
                currentST = ne.ST;
            }

            if (_queue.Count == 0)
            {
                FireFrameProcess = 0;

                bool flg = false;
                foreach (var v in dicNoteOn.Keys)
                {
                    if (dicNoteOn[v] > 0) { flg = true; break; }
                }

                if (!flg && currentST <= 0) IsFinished = true;
            }
        }
    }
}
