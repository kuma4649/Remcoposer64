using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlSameMeasEvent : MdlEvent
    {
        public MdlSameMeasEvent(EventManager parent) : base(parent)
        {
        }

        public override MIDIEventType Type => MIDIEventType.SameMeas;

        public static readonly string SAMEMEAS_SPACE = "    ";
        public static readonly int SAMEMEAS_MAXLEN = 4;


        public override int AdjustCursorPosition(int rawPos)
        {
            // 問答無用で 2(=ST) にマッピング
            return 2;
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            cursor.Position = UIEvent.COL_ST;   // SameMeas の編集カラムは ST に相当
            keyb.inputState = Keyboard.InputState.EditEvent;

            me.editing = true;

            MIDISameMeasEvent se = (MIDISameMeasEvent)me;
            // 編集値 初期化（空）
            me.editVal = SAMEMEAS_SPACE;
            // 既存値 初期化（表示は +1）
            me.virVal = SAMEMEAS_SPACE + (se.RepeatMeas + 1).ToString();
            me.virVal = me.virVal.Substring(me.virVal.Length - SAMEMEAS_MAXLEN, SAMEMEAS_MAXLEN);

            // Undo 用バックアップ
            se.backupRepeatMeas = se.RepeatMeas;

            return true;
        }

        public override bool UpdateActiveEvent(
            MIDIEvent me,
            char keyChar,
            CommitKeyPattern keyPattern,
            CursorInfo cursor)
        {
            // ① 数字入力
            if (char.IsDigit(keyChar))
            {
                me.editVal += keyChar;
                if (me.editVal.Length > SAMEMEAS_MAXLEN) me.editVal = me.editVal.Substring(1, SAMEMEAS_MAXLEN);
            }

            // ② Backspace
            if (keyChar == '\b')
            {
                me.editVal = SAMEMEAS_SPACE + me.editVal;
                me.editVal = me.editVal.Substring(me.editVal.Length - SAMEMEAS_MAXLEN - 1, SAMEMEAS_MAXLEN);
            }

            // ③ 編集終了判定
            if (keyPattern == CommitKeyPattern.EnterKey ||
                keyPattern == CommitKeyPattern.UpKey ||
                keyPattern == CommitKeyPattern.DownKey)
            {
                //editing = false;
                Commit(me, cursor, keyPattern);
                me.editing = false;
                return true; // ★ 編集終了
            }

            // ④ 編集継続
            me.editing = true;
            return false; // ★ 編集継続
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




        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            me.editing = false;

            MIDISameMeasEvent se = (MIDISameMeasEvent)me;
            // 入力値を反映（-1 はUndoRedoのEditSameMEventCommandでやるので不要）
            if (!int.TryParse(me.editVal, out int newMeas)) newMeas = 0;
            int oldMeas = se.backupRepeatMeas;

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

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return "";
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDISameMeasEvent se = (MIDISameMeasEvent)me;
            return $"{se.Meas + 1},{se.Step + 1}: SameMeas: {se.RepeatMeas}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            return new MIDIBarLineEvent()
            {
                Meas = meas,
                Step = step
            };
        }
    }
}
