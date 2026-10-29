using Remcoposer64.ProjectData;

namespace Remcoposer64.TrackEditor
{
    public class TrackEditorCursor
    {
        public LinkedListNode<MIDITrack>? Track;
        public LinkedListNode<MIDIPart>? Part;

        public TrackEditorFocusArea Area;
        public TrackHeaderColumn HeaderColumn;

        public void Clear()
        {
            Track = null;
            Part = null;
            Area = default;
            HeaderColumn = default;
        }

    }
}
