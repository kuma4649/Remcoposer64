using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlEndEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.MetaEndOfTrack;

        public MdlEndEvent(EventManager parent) : base(parent)
        {
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPatten)
        {
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return "";
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIEndEvent ee = (MIDIEndEvent)me;
            return $"{ee.Meas + 1},{ee.Step + 1}: End";
        }

    }
}
