using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackOutDeviceCommand : UndoRedoManager.ICommand
    {
        private readonly MIDITrack track;
        private readonly int oldValue;
        private readonly int newValue;

        public ChangeTrackOutDeviceCommand(
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
            track.OutDevice = newValue;
        }

        public void Unexecute()
        {
            track.OutDevice = oldValue;
        }

        public override string ToString()
        {
            return $"OutDevice {oldValue}->{newValue}";
        }
    }
}
