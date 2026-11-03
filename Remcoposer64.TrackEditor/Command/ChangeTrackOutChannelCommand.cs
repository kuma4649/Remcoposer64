using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackOutChannelCommand : UndoRedoManager.ICommand
    {
        private readonly MIDITrack track;
        private readonly int? oldValue;
        private readonly int? newValue;

        public ChangeTrackOutChannelCommand(
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
            track.OutChannel = newValue;
        }

        void ICommand.Unexecute()
        {
            track.OutChannel = oldValue;
        }
    }
}