namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class BarLineEvent : Event
    {
        public BarLineEvent(EventManager parent) : base(parent)
        {
        }

        public override EventType Type => EventType.BarLine;

        // 小節線は Meas のみ使う
        public int MeasureTotalST { get; set; }

        public override Event Clone()
        {
            return new BarLineEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,
                MeasureTotalST = this.MeasureTotalST
            };
        }

        public override void Commit(CursorInfo cursorInfo,CommitKeyPattern keyPattern)
        {
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return "";
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: BarLine: ";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            return new BarLineEvent(parent)
            {
                Meas = meas,
                Step = step
            };
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            //BarLineは編集するべきものが無いので、編集モードはキャンセルし次の行へ移動する
            cursor.Position = 0;
            if (cursor.EventNode.Next != null)
            {
                cursor.EventNode = cursor.EventNode.Next;
                cursor.Index++;
            }
            editing = false;
            keyb.inputState = Keyboard.InputState.None;

            return true;
        }


    }
}
