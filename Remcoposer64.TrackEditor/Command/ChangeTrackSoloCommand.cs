using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor.Command
{
    internal class ChangeTrackSoloCommand : ICommand
    {
        private MIDITrack track;
        private bool oldValue;
        private bool newValue;

        public ChangeTrackSoloCommand(
            MIDITrack track,
            bool oldValue,
            bool newValue)
        {
            this.track = track;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public void Execute()
        {
            track.Solo = newValue;
        }

        public void Unexecute()
        {
            track.Solo = oldValue;
        }
    }
}
