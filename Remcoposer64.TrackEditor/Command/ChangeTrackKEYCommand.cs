using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackKEYCommand : ICommand
    {
        private readonly MIDITrack track;
        private readonly int? oldValue;
        private readonly int? newValue;

        public ChangeTrackKEYCommand(
            MIDITrack track,
            int? oldValue,
            int? newValue)
        {
            this.track = track;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public void Execute()
        {
            track.Key = newValue;
        }

        public void Unexecute()
        {
            track.Key = oldValue;
        }
    }
}
