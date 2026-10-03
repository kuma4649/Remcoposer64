using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class SameMeasEvent : Event
    {
        public SameMeasEvent(EventManager parent) : base(parent)
        {
        }

        public override EventType Type => EventType.SameMeas;

        public LinkedListNode<Event> RepeatMeasBeforeNode {
            get;
            set;
        }

        public int RepeatMeas { get; set; } = 0;
        public int backupRepeatMeas;
        public static readonly string SAMEMEAS_SPACE = "    ";
        public static readonly int SAMEMEAS_MAXLEN = 4;

        public override Event Clone()
        {
            return new SameMeasEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,
                RepeatMeasBeforeNode = this.RepeatMeasBeforeNode,
                RepeatMeas = this.RepeatMeas
            };
        }

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            editing = false;

            // 入力値を反映（-1 はUndoRedoのEditSameMEventCommandでやるので不要）
            if (!int.TryParse(editVal, out int newMeas)) newMeas = 0;
            int oldMeas = backupRepeatMeas;

            // カーソル位置を維持
            var newCursorInfo = new CursorInfo();
            newCursorInfo.Index = cursorInfo.Index;
            newCursorInfo.Position = cursorInfo.Position;

            // Undo/Redo
            ApplyUndoRedo(
                // old block
                cursorInfo,
                oldMeas.ToString(),
                true,
                // new block
                newCursorInfo,
                newMeas.ToString(),
                false
            );
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return "";
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: SameMeas: {RepeatMeas}";
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
            cursor.Position = UIEvent.COL_ST;   // SameMeas の編集カラムは ST に相当
            keyb.inputState = Keyboard.InputState.EditEvent;

            editing = true;

            // 編集値 初期化（空）
            editVal = SAMEMEAS_SPACE;
            // 既存値 初期化（表示は +1）
            virVal = SAMEMEAS_SPACE + (RepeatMeas + 1).ToString();
            virVal = virVal.Substring(virVal.Length - SAMEMEAS_MAXLEN, SAMEMEAS_MAXLEN);

            // Undo 用バックアップ
            backupRepeatMeas = RepeatMeas;

            return true;
        }

        private void ApplyUndoRedo(
            // --- old block ---
            CursorInfo oldCursor,
            string oldVal,
            bool oldEditing,
            // --- new block ---
            CursorInfo newCursor,
            string newVal,
            bool newEditing)
        {
            var cmd = new EditSameMeasEventCommand(
                parent,
                parent.parent,
                oldCursor.Index,
                // old block
                oldVal,
                oldCursor.Index,
                oldEditing,
                // new block
                newVal,
                newCursor.Index,
                newEditing
            );

            ExecuteUndoRedoCommand(cmd);
        }

        public override bool UpdateActiveEvent(
            char keyChar,
            CommitKeyPattern keyPattern,
            CursorInfo cursor)
        {
            // ① 数字入力
            if (char.IsDigit(keyChar))
            {
                editVal += keyChar;
                if (editVal.Length > SAMEMEAS_MAXLEN) editVal = editVal.Substring(1, SAMEMEAS_MAXLEN);
            }

            // ② Backspace
            if (keyChar == '\b')
            {
                editVal = SAMEMEAS_SPACE + editVal;
                editVal = editVal.Substring(editVal.Length - SAMEMEAS_MAXLEN - 1, SAMEMEAS_MAXLEN);
            }

            // ③ 編集終了判定
            if (keyPattern == CommitKeyPattern.EnterKey ||
                keyPattern == CommitKeyPattern.UpKey ||
                keyPattern == CommitKeyPattern.DownKey)
            {
                //editing = false;
                Commit(cursor, keyPattern);
                editing = false;
                return true; // ★ 編集終了
            }

            // ④ 編集継続
            editing = true;
            return false; // ★ 編集継続
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            // 問答無用で 2(=ST) にマッピング
            return 2;
        }


    }
}
