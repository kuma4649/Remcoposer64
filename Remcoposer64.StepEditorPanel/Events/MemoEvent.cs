using Remcoposer64.StepEditorPanelControl.Command;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class MemoEvent : Event
    {
        public override EventType Type => EventType.Memo;

        public string Text { get; set; } = ""; // 0–16383

        public bool memoBoxSwitch;
        public string memoBoxText = "";
        public int memoBoxDrawX;
        public int memoBoxDrawY;
        public int memoBoxDrawWidth;
        public int memoBoxDrawHeight;
        public int memoBoxCaretIndex = 0;
        public string memoBoxComposition = "";
        public string memoBoxBackupText = "";
        public int memoBoxBackupCaretIndex = 0;
        public int memoBoxBackupCursorIndex = 0;
        internal int IMECursorPos = 0;

        public MemoEvent(EventManager parent) : base(parent)
        {
            virVal = " ";//編集モードに入っていることを示すために、virValを空白に設定
        }


        public override Event Clone()
        {
            return new MemoEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                Text = this.Text,

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal
            };
        }

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            Text = memoBoxText;

            bool oldEditing = this.editing;
            editing = false;
            
            // ApplyUndoRedo 呼び出し
            ApplyUndoRedo(
                cursorInfo,
                cursorInfo,
                memoBoxBackupText,
                memoBoxText,
                memoBoxBackupCaretIndex,
                memoBoxCaretIndex,
                oldEditing,
                false
            );
        }

        private void ApplyUndoRedo(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            string oldText,
            string newText,
            int oldCaretPos,
            int newCaretPos,
            bool oldEditing,
            bool newEditing)
        {
            var cmd = new EditMemoEventCommand(
                parent,
                parent.parent,
                cursorInfo.Index,
                oldText,
                newText,
                cursorInfo.Index,
                oldCaretPos,
                newCursorInfo.Index,
                newCaretPos,
                oldEditing,
                newEditing
            );

            ExecuteUndoRedoCommand(cmd);
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return Text;
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: Memo: {Text}";
        }

        public override bool UpdateActiveEvent(
            char keyChar,
            CommitKeyPattern keyPattern,
            CursorInfo cursor)
        {

            // ① 通常文字入力（Enter / Backspace / ESC 以外）
            if (keyChar != '\r' && keyChar != '\b' && keyChar != '\u001b')
            {
                memoBoxText = memoBoxText.Insert(memoBoxCaretIndex, keyChar.ToString());
                memoBoxCaretIndex++;
                Text = memoBoxText;
            }

            // ② Backspace
            if (keyChar == '\b')
            {
                if (memoBoxCaretIndex > 0)
                {
                    memoBoxText = memoBoxText.Remove(memoBoxCaretIndex - 1, 1);
                    memoBoxCaretIndex--;
                    Text = memoBoxText;
                }
            }

            // ③ 編集終了判定（NoteEvent と同じ）
            // Enter / ↑ / ↓ / その他のキーで確定
            if (keyPattern == CommitKeyPattern.EnterKey
                || keyPattern == CommitKeyPattern.UpKey
                || keyPattern == CommitKeyPattern.DownKey
                //|| keyPattern == CommitKeyPattern.RightKey 
                //|| keyPattern == CommitKeyPattern.LeftKey
                //|| keyPattern == CommitKeyPattern.OtherKey
                )
            {
                editing = false;
                return true; // ★ 編集終了
            }

            // ④ 編集継続
            editing = true;
            return false; // ★ 編集継続
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            keyb.inputState = Keyboard.InputState.EditEvent;
            UpdateMemoBoxInfo(parent.parent);

            memoBoxComposition = "";
            memoBoxSwitch = true;
            memoBoxText = Text ?? "";
            memoBoxCaretIndex = memoBoxText.Length;
            editing = true;
            memoBoxBackupText = memoBoxText;
            memoBoxBackupCaretIndex = memoBoxCaretIndex;
            memoBoxBackupCursorIndex = cursor.Index;

            return true;
        }

        public override void EndEdit(Keyboard keyb)
        {
            memoBoxSwitch = false;

            // composition を破棄して確定
            memoBoxCaretIndex = memoBoxText.Length;

            editing = false;
            keyb.inputState = Keyboard.InputState.None;
        }

        public void UpdateMemoBoxInfo(StepEditorPanel panel)
        {
            int row = panel.cursor.Index - panel.scrollOffset;

            int noteColumnOffset = (panel.columnWidths[1] + panel.columnWidths[3]) * panel.charWidth;

            memoBoxDrawX = panel.iconMergin + noteColumnOffset;
            memoBoxDrawY = (1 + row) * panel.lineHeight;

            memoBoxDrawWidth = 200;        // 必要なら panel から計算
            memoBoxDrawHeight = panel.lineHeight;
        }

    }
}
