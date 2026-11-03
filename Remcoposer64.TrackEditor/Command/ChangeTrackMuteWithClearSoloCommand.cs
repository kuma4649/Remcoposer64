using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class ChangeTrackMuteWithClearSoloCommand : ICommand
    {
        private readonly MIDITrack track;
        private readonly bool oldMute;
        private readonly bool newMute;

        private readonly Dictionary<MIDITrack, bool> oldSoloStates
            = new();

        public ChangeTrackMuteWithClearSoloCommand(
            MIDIProject project,
            MIDITrack track,
            bool oldMute,
            bool newMute)
        {
            this.track = track;
            this.oldMute = oldMute;
            this.newMute = newMute;

            foreach (var trk in project.Track)
            {
                oldSoloStates[trk] = trk.Solo;
            }
        }

        public void Execute()
        {
            foreach (var trk in oldSoloStates.Keys)
            {
                trk.Solo = false;
            }

            track.Mute = newMute;
        }

        public void Unexecute()
        {
            track.Mute = oldMute;

            foreach (var kv in oldSoloStates)
            {
                kv.Key.Solo = kv.Value;
            }
        }
    }
}
