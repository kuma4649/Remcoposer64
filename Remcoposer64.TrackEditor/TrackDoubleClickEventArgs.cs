using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor
{
    public sealed class TrackDoubleClickEventArgs : EventArgs
    {
        public LinkedListNode<MIDITrack> TrackNode { get; }

        public TrackDoubleClickEventArgs(
            LinkedListNode<MIDITrack> trackNode)
        {
            TrackNode = trackNode;
        }
    }
}
