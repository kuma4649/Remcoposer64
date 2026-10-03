using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlMemoEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.Memo;

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

        public MdlMemoEvent(EventManager parent) : base(parent)
        {
            //virVal = " ";//編集モードに入っていることを示すために、virValを空白に設定
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            keyb.inputState = Keyboard.InputState.EditEvent;
            UpdateMemoBoxInfo(parent.parent);

            memoBoxComposition = "";
            memoBoxSwitch = true;
            memoBoxText = ((MIDIMemoEvent)me).Text ?? "";
            memoBoxCaretIndex = memoBoxText.Length;
            me.editing = true;
            memoBoxBackupText = memoBoxText;
            memoBoxBackupCaretIndex = memoBoxCaretIndex;
            memoBoxBackupCursorIndex = cursor.Index;

            return true;
        }

        public override bool UpdateActiveEvent(
            MIDIEvent me,
            char keyChar,
            CommitKeyPattern keyPattern,
            CursorInfo cursor)
        {

            MIDIMemoEvent mme = (MIDIMemoEvent)me;
            // ① 通常文字入力（Enter / Backspace / ESC 以外）
            if (keyChar != '\r' && keyChar != '\b' && keyChar != '\u001b')
            {
                memoBoxText = memoBoxText.Insert(memoBoxCaretIndex, keyChar.ToString());
                memoBoxCaretIndex++;
                mme.Text = memoBoxText;
            }

            // ② Backspace
            if (keyChar == '\b')
            {
                if (memoBoxCaretIndex > 0)
                {
                    memoBoxText = memoBoxText.Remove(memoBoxCaretIndex - 1, 1);
                    memoBoxCaretIndex--;
                    mme.Text = memoBoxText;
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
                me.editing = false;
                return true; // ★ 編集終了
            }

            // ④ 編集継続
            me.editing = true;
            return false; // ★ 編集継続
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

        public override void Commit(MIDIEvent me,CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            ((MIDIMemoEvent)me).Text = memoBoxText;

            bool oldEditing = me.editing;
            me.editing = false;

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

        public override string GetVal(MIDIEvent me,CursorInfo cursorInfo)
        {
            return ((MIDIMemoEvent)me).Text;
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIMemoEvent mme = (MIDIMemoEvent)me;
            return $"{mme.Meas + 1},{mme.Step + 1}: Memo: {mme.Text}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 3) return null;
            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int cc)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[2], out int val)) { WinApi.Beep(); return null; }

            return new MIDIMemoEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                Text = body,
            };
        }

        public override void CopyFrom(MIDIEvent srcMe, MIDIEvent otherMe, MdlEvent otherMdle)
        {
            base.CopyFrom(srcMe, otherMe, otherMdle);

            var n = (MIDIMemoEvent)otherMe;
            n.CopyFrom(otherMe);

            //((MIDIMemoEvent)srcMe).GT = n.GT;
            ((MIDIMemoEvent)srcMe).Text = n.Text;
        }

        public override void EndEdit(MIDIEvent me, Keyboard keyb)
        {
            memoBoxSwitch = false;
            // composition を破棄して確定
            memoBoxCaretIndex = memoBoxText.Length;

            base.EndEdit(me, keyb);
        }

        public void UpdateMemoBoxInfo(StepEditorPanelControl.StepEditorPanel panel)
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
