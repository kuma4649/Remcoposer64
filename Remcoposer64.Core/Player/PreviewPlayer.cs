using Remcoposer64.Common;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.Core.Player
{
    public class PreviewPlayer : IPlayerChannel
    {
        private readonly Queue<MIDIEvent> _queue = new();

        public int FireFrameProcess { get; set; } = 0;
        public bool IsFinished { get; set; } = true;

        public void Initialize(Setting.midiOutInfo[] midiOutInfo, NAudio.Midi.MidiOut[] midiOut, Setting.midiInInfo[] midiInInfo, NAudio.Midi.MidiIn[] midiIn)
        {
            _queue.Clear();
            FireFrameProcess = 0;
            IsFinished = true;
        }

        public void Enqueue(List<MIDIEvent> events)
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
            // プレビューは即時発音なので特に何もしない場合もある
        }

        public void ProcessFrame(long seqCounter)
        {
            if (_queue.Count == 0)
            {
                FireFrameProcess = 0;
                IsFinished = true;
                return;
            }

            // キューから取り出して発音する
            var ev = _queue.Dequeue();
            // TODO: MIDIOut.Send(ev)

            if (_queue.Count == 0)
            {
                FireFrameProcess = 0;
                IsFinished = true;
            }
        }
    }
}
