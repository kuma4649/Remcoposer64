using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;
using System;

namespace Remcoposer64.TrackEditor.Command
{
    public class MoveTrackCommand : ICommand
    {
        private readonly MIDIProject project;
        private readonly int sourceIndex;
        private readonly int targetIndex;

        public MoveTrackCommand(
            MIDIProject project,
            int sourceIndex,
            int targetIndex)
        {
            this.project = project;
            this.sourceIndex = sourceIndex;
            this.targetIndex = targetIndex;
        }

        public void Execute()
        {
            Move(sourceIndex, targetIndex);
        }

        public void Unexecute()
        {
            Move(targetIndex, sourceIndex);
        }

        /// <summary>
        /// 隣接トラックの移動専用
        /// from と to は ±1 のみ想定
        /// </summary>
        private void Move(int from, int to)
        {
            if (Math.Abs(from - to) != 1)
                return;

            if (from < 0 || from >= project.Track.Count)
                return;

            if (to < 0 || to >= project.Track.Count)
                return;

            if (from == to)
                return;

            var node = GetTrackNode(from);
            if (node == null)
                return;

            if (from < to)
            {
                var next = node.Next;
                if (next == null) return;

                project.Track.Remove(node);
                project.Track.AddAfter(next, node.Value);
            }
            else
            {
                var prev = node.Previous;
                if (prev == null) return;

                project.Track.Remove(node);
                project.Track.AddBefore(prev, node.Value);
            }

            RenumberTracks();
        }

        private LinkedListNode<MIDITrack> GetTrackNode(int index)
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
