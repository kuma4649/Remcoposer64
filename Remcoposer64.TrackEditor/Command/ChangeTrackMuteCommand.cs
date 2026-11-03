using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackMuteCommand : ICommand
    {
        private MIDITrack track;
        private bool oldValue;
        private bool newValue;

        public ChangeTrackMuteCommand(
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
            track.Mute = newValue;
        }

        public void Unexecute()
        {
            track.Mute = oldValue;
        }
    }
}
