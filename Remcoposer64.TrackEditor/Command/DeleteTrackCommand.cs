using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class DeleteTrackCommand : ICommand
    {
        private readonly MIDIProject project;
        private readonly int trackNo;

        private MIDITrack deletedTrack;
        private int restoreIndex;

        public DeleteTrackCommand(
            MIDIProject project,
            int trackNo)
        {
            this.project = project;
            this.trackNo = trackNo;
        }

        public void Execute()
        {
            var node = GetTrackNode(trackNo);

            if (node == null)
                return;

            deletedTrack = node.Value;
            restoreIndex = trackNo;

            project.Track.Remove(node);

            Renumber();
        }

        public void Unexecute()
        {
            if (deletedTrack == null)
                return;

            if (restoreIndex <= 0)
            {
                project.Track.AddFirst(deletedTrack);
            }
            else
            {
                var prev = GetTrackNode(restoreIndex - 1);

                if (prev != null)
                    project.Track.AddAfter(prev, deletedTrack);
                else
                    project.Track.AddLast(deletedTrack);
            }

            Renumber();
        }

        private LinkedListNode<MIDITrack> GetTrackNode(int index)
        {
            var node = project.Track.First;

            for (int i = 0; i < index && node != null; i++)
                node = node.Next;

            return node;
        }

        private void Renumber()
        {
            int no = 0;

            foreach (var trk in project.Track)
                trk.Number = no++;
        }
    }
}