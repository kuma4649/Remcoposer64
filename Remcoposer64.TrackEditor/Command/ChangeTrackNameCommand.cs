using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackNameCommand : ICommand
    {
        private readonly MIDITrack track;
        private readonly string oldValue;
        private readonly string newValue;

        public ChangeTrackNameCommand(
            MIDITrack track,
            string oldValue,
            string newValue)
        {
            this.track = track;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public string Name =>
            $"Track Name : {oldValue} -> {newValue}";

        void ICommand.Execute()
        {
            track.Name = newValue;
        }

        void ICommand.Unexecute()
        {
            track.Name = oldValue;
        }
    }
}
