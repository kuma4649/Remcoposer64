using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class InsertTrackCommand : ICommand
    {
        private readonly MIDIProject project;
        private readonly int insertAfterTrackNo;

        private MIDITrack insertedTrack;

        public InsertTrackCommand(
            MIDIProject project,
            int insertAfterTrackNo)
        {
            this.project = project;
            this.insertAfterTrackNo = insertAfterTrackNo;
        }

        public void Execute()
        {
            insertedTrack ??= new MIDITrack();
            insertedTrack.Part.AddLast(new MIDIPart());
            insertedTrack.Part.First.Value.Event.AddLast(new MIDIEndEvent());

            if (insertAfterTrackNo < 0)
            {
                project.Track.AddFirst(insertedTrack);
            }
            else
            {
                var node = GetTrackNode(insertAfterTrackNo);

                if (node != null)
                    project.Track.AddAfter(node, insertedTrack);
                else
                    project.Track.AddLast(insertedTrack);
            }

            Renumber();
        }

        public void Unexecute()
        {
            project.Track.Remove(insertedTrack);

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