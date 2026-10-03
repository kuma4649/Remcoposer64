using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlBarLineEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.BarLine;

        public MdlBarLineEvent(EventManager parent) : base(parent)
        {
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return "";
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIBarLineEvent be = (MIDIBarLineEvent)me;
            return $"{be.Meas + 1},{be.Step + 1}: BarLine: ";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            return new MIDIBarLineEvent()
            {
                Meas = meas,
                Step = step
            };
        }

        public override bool BeginEdit(MIDIEvent me,CursorInfo cursor, Keyboard keyb)
        {
            //BarLineは編集するべきものが無いので、編集モードはキャンセルし次の行へ移動する
            cursor.Position = 0;
            if (cursor.EventNode.Next != null)
            {
                cursor.EventNode = cursor.EventNode.Next;
                cursor.Index++;
            }
            me.editing = false;
            keyb.inputState = Keyboard.InputState.None;

            return true;
        }


    }
}
