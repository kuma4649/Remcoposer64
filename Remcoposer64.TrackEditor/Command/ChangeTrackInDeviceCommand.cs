using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackInDeviceCommand : UndoRedoManager.ICommand
    {
        private readonly MIDITrack track;
        private readonly int oldValue;
        private readonly int newValue;

        public ChangeTrackInDeviceCommand(
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
            track.InDevice = newValue;
        }

        public void Unexecute()
        {
            track.InDevice = oldValue;
        }

        public override string ToString()
        {
            return $"InDevice {oldValue}->{newValue}";
        }
    }
}
