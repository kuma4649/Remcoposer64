using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackInChannelCommand : UndoRedoManager.ICommand
    {
        private readonly MIDITrack track;
        private readonly int? oldValue;
        private readonly int? newValue;

        public ChangeTrackInChannelCommand(
            MIDITrack track,
            int? oldValue,
            int? newValue)
        {
            this.track = track;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        void ICommand.Execute()
        {
            track.InChannel = newValue;
        }

        void ICommand.Unexecute()
        {
            track.InChannel = oldValue;
        }
    }
}
