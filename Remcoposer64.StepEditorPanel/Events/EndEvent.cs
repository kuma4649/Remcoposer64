using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class EndEvent : Event
    {
        public EndEvent(EventManager parent) : base(parent)
        {
        }

        public override EventType Type => EventType.End;

        public override Event Clone()
        {
            return new EndEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum
            };
        }

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPatten)
        {
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return "";
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1 },{Step + 1}: End";
        }

    }
}
