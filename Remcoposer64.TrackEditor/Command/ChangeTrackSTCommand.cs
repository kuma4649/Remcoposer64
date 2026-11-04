using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackSTCommand : ICommand
    {
        private readonly MIDITrack track;
        private readonly int oldValue;
        private readonly int newValue;

        public ChangeTrackSTCommand(
            MIDITrack track,
            int oldValue,
            int newValue)
        {
            this.track = track;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public void Execute()
        {
            track.St = newValue;
        }

        public void Unexecute()
        {
            track.St = oldValue;
        }
    }
}
