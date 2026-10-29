using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor
{
    public class TrackEditorSelection
    {
        public HashSet<LinkedListNode<MIDITrack>> Tracks
            = new();

        public HashSet<LinkedListNode<MIDIPart>> Parts
            = new();

        public void Clear()
        {
            Tracks.Clear();
            Parts.Clear();
        }

    }
}
