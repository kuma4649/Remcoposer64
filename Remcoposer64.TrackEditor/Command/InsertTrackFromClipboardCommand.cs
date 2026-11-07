    using Remcoposer64.ProjectData;
    using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.TrackEditor.Command
{
    public class InsertTrackFromClipboardCommand : ICommand
    {
        private readonly MIDIProject project;
        private readonly int insertAfterTrackNo;
        private readonly MIDITrack clipboardTrack;

        private MIDITrack insertedTrack;

        public InsertTrackFromClipboardCommand(
            MIDIProject project,
            int insertAfterTrackNo,
            MIDITrack clipboardTrack)
        {
            this.project = project;
            this.insertAfterTrackNo = insertAfterTrackNo;
            this.clipboardTrack = clipboardTrack;
        }

        public void Execute()
        {
            if (clipboardTrack == null)
                return;

            insertedTrack ??=
                clipboardTrack.Clone();

            if (insertAfterTrackNo < 0)
            {
                project.Track.AddFirst(insertedTrack);
            }
            else
            {
                var node =
                    GetTrackNode(insertAfterTrackNo);

                if (node != null)
                    project.Track.AddAfter(node, insertedTrack);
                else
                    project.Track.AddLast(insertedTrack);
            }

            RenumberTracks();
        }

        public void Unexecute()
        {
            if (insertedTrack == null)
                return;

            project.Track.Remove(insertedTrack);

            RenumberTracks();
        }

        private LinkedListNode<MIDITrack>
            GetTrackNode(int index)
        {
            var node = project.Track.First;

            for (int i = 0; i < index && node != null; i++)
                node = node.Next;

            return node;
        }

        private void RenumberTracks()
        {
            int no = 0;

            foreach (var trk in project.Track)
            {
                trk.Number = no++;
            }
        }
    }
}
