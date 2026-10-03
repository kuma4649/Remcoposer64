using Remcoposer64.StepEditorPanelControl.Command;
using System.Xml.Linq;
using static Remcoposer64.StepEditorPanelControl.Events.NoteEvent;
using static Remcoposer64.StepEditorPanelControl.WinApi;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class ControlChangeEvent : Event
    {
        public override EventType Type => EventType.ControlChange;
        public static readonly int Controller_MIN = 0;
        public static readonly int Controller_MAX = 127;

        public int ControllerNumber { get; set; }
        public int ControllerValue { get; set; }
        private int backupST;
        private int backupControllerNumber;
        private int backupControllerValue;
        private static int oControllerNumber;
        private static int oControllerValue;

        public ControlChangeEvent(EventManager parent) : base(parent)
        {
            ST = oST;
            ControllerNumber = oControllerNumber;
            ControllerValue = oControllerValue;
        }

        public override Event Clone()
        {
            return new ControlChangeEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                ControllerNumber = this.ControllerNumber,
                ControllerValue = this.ControllerValue,

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal
            };
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3) return 2;   // ST
            if (rawPos == 3) return 3;  // ControllerNumber
            return 4;                   // ControllerValue
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            editing = true;
            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            switch (cursor.Position)
            {
                case 2: // ST
                    editVal = ST_SPACE;
                    virVal = ST.ToString().PadLeft(ST_MAXLEN);
                    backupST = ST;
                    return true;

                case 3: // ControllerNumber
                    editVal = GT_SPACE;
                    virVal = ControllerNumber.ToString().PadLeft(GT_MAXLEN);
                    backupControllerNumber = ControllerNumber;
                    return true;

                case 4: // ControllerValue
                    editVal = VEL_SPACE;
                    virVal = ControllerValue.ToString().PadLeft(VEL_MAXLEN);
                    backupControllerValue = ControllerValue;
                    return true;
            }

            return false;
        }
        
        public override bool UpdateActiveEvent(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            if (!editing) return true;

            if (keyChar == '\b' && editVal.Length > 0)
            {
                editVal = "";
                return false;
            }

            switch (cursor.Position)
            {
                case 2: UpdateActiveControlChangeST(keyChar, keyPattern, cursor); break;
                case 3: UpdateActiveControlChangeNumber(keyChar, keyPattern, cursor); break;
                case 4: UpdateActiveControlChangeValue(keyChar, keyPattern, cursor); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !editing;
        }

        public void UpdateActiveControlChangeST(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;

            // ★ Enter が押された場合 → ST確定処理へ
            if (keyChar == '\r')
            {
                // ST の妥当性チェック（数字のみ）
                if (!Common.TryParseEither(editVal, virVal, out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                ni = newST;
                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2Number(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2Number(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2Number(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2Number(cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2Number(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2Number(cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = ST_SPACE + editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(editVal);
                if (parent.parent.OverwriteMode)
                    MoveNext_ST2Number(cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2Number(cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2Number(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newST,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                // ★ ① カーソル移動と editVal/virVal 初期化
                ApplyCursorMove(
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,                          // 次のカーソル位置（ST → ControllerNumber）
                    GT_SPACE,                   // editVal 初期化
                    ControllerNumber.ToString() // virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = backupST;
                // ★ ③ 値を更新（ここで初めて更新する）
                ST = newST;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit();

                // ★ ⑤ コマンド発行
                ApplyUndoRedo(
                    // old block
                    cursorInfo,
                    oldST,
                    true,
                    // new block
                    newCursorInfo,
                    newST,
                    newEditing
                );

                var pos = parent.parent.PointToScreenFromCursorInfo(newCursorInfo);
                parent.parent.CCcandidateForm.Location = pos;
                parent.parent.CCcandidateForm.Visible = true;

            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void UpdateActiveControlChangeNumber(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → ControllerNumber確定処理
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni) || !ni.IsBetween(Controller_MIN, Controller_MAX))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(GT_MAXLEN);
                s = s.Substring(s.Length - GT_MAXLEN, GT_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = GT_SPACE + editVal;
                s = s.Substring(s.Length - GT_MAXLEN - 1, GT_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(Controller_MIN, Controller_MAX))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            if (!parent.parent.CCcandidateForm.Visible)
            {
                var pos = parent.parent.PointToScreenFromCursorInfo(cursorInfo);
                parent.parent.CCcandidateForm.Location = pos;
                parent.parent.CCcandidateForm.Visible = true;
            }
            parent.parent.CCcandidateForm.FillCandidates(editVal.Trim());

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == GT_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();

                if (parent.parent.OverwriteMode)
                    MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }
        }

        public void MoveNext_Number2Value(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newControllerNumber,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                // ★ ① カーソル移動と editVal/virVal 初期化
                ApplyCursorMove(
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,                           // 次のカーソル位置（ControllerNumber → ControllerValue）
                    VEL_SPACE,                   // editVal 初期化
                    ControllerValue.ToString()   // virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldNumber = backupControllerNumber;
                // ★ ③ 値を更新（ここで初めて更新する）
                ControllerNumber = newControllerNumber;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit();

                // ★ ⑤ コマンド発行
                ApplyUndoRedo(
                    // old block
                    cursorInfo,
                    oldNumber,
                    true,
                    // new block
                    newCursorInfo,
                    newControllerNumber,
                    newEditing
                );

                parent.parent.CCcandidateForm.Visible = false;
            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void UpdateActiveControlChangeValue(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → ControllerValue確定処理
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni) || !ni.IsBetween(Controller_MIN, Controller_MAX))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(VEL_MAXLEN);
                s = s.Substring(s.Length - VEL_MAXLEN, VEL_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = VEL_SPACE + editVal;
                s = s.Substring(s.Length - VEL_MAXLEN - 1, VEL_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(Controller_MIN, Controller_MAX))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == VEL_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();

                if (parent.parent.OverwriteMode)
                    MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_Value2NextEvent(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newControllerValue,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                // ★ ① カーソル移動と editVal/virVal 初期化
                ApplyCursorMove(
                    cursorInfo,
                    newCursorInfo,
                    action,
                    0,                          // 次のカーソル位置（次イベントのホームポジション）
                    "",                         // editVal 初期化
                    ""                          // virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldValue = backupControllerValue;

                // ★ ③ 値を更新（ここで初めて更新する）
                ControllerValue = newControllerValue;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit();

                // ★ ⑤ コマンド発行
                ApplyUndoRedo(
                    // old block
                    cursorInfo,
                    oldValue,
                    true,
                    // new block
                    newCursorInfo,
                    newControllerValue,
                    newEditing
                );
            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void ApplyCursorMove(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            MoveNextCursorAction action,
            int nextFieldOffset,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                    newCursorInfo.Index = cursorInfo.Index + 1;
                    newCursorInfo.EventNode = cursorInfo.EventNode.Next;
                    newCursorInfo.Position = cursorInfo.Position;
                    break;

                case MoveNextCursorAction.PreviousEvent:
                    newCursorInfo.Index = cursorInfo.Index - 1;
                    newCursorInfo.EventNode = cursorInfo.EventNode.Previous;
                    newCursorInfo.Position = cursorInfo.Position;
                    break;

                case MoveNextCursorAction.NextField:
                    newCursorInfo.Index = cursorInfo.Index;
                    newCursorInfo.EventNode = cursorInfo.EventNode;
                    newCursorInfo.Position = cursorInfo.Position + nextFieldOffset;
                    break;

                case MoveNextCursorAction.StayOnField:
                    newCursorInfo.Index = cursorInfo.Index;
                    newCursorInfo.EventNode = cursorInfo.EventNode;
                    newCursorInfo.Position = cursorInfo.Position;
                    break;
            }

            ApplyEditValAndVirVal(action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MoveNextCursorAction action,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                case MoveNextCursorAction.PreviousEvent:
                    editVal = "";
                    virVal = "";
                    break;

                case MoveNextCursorAction.NextField:
                    editVal = nextEditVal;
                    virVal = nextVirVal;
                    break;

                case MoveNextCursorAction.StayOnField:
                    // 何もしない
                    break;
            }
        }

        private void ApplyUndoRedo(
            // --- old block ---
            CursorInfo oldCursor,
            object oldVal,
            bool oldEditing,
            // --- new block ---
            CursorInfo newCursor,
            object newVal,
            bool newEditing)
        {
            var cmd = new EditControlChangeEventCommand(
                parent,
                parent.parent,
                oldCursor.Index,
                // old block
                oldVal,
                oldCursor.Index,
                oldCursor.Position,
                oldEditing,
                // new block
                newVal,
                newCursor.Index,
                newCursor.Position,
                newEditing
            );

            ExecuteUndoRedoCommand(cmd);
        }

        private void commit()
        {
            editing = false;

            oST = ST;
            oControllerNumber = ControllerNumber;
            oControllerValue = ControllerValue;
        }

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!editing) return;

            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 2: // ST
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_ST2Number(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 3: // ControllerNumber
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_Number2Value(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 4: // ControllerValue
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_Value2NextEvent(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            // Enter / Arrow keys の場合は UpdateActiveXXX('\r') を呼ぶ
            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveControlChangeST('\r', keyPattern, cursorInfo);
                    break;

                case 3:
                    UpdateActiveControlChangeNumber('\r', keyPattern, cursorInfo);
                    break;

                case 4:
                    UpdateActiveControlChangeValue('\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ST.ToString(),
                3 => ControllerNumber.ToString(),
                4 => ControllerValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: ControlChange: {ST},{ControllerNumber},{ControllerValue}";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 3) return null;
            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int cc)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[2], out int val)) { WinApi.Beep(); return null; }

            return new ControlChangeEvent(parent)
            {
                Meas = meas,
                Step = step,
                ST = st,
                ControllerNumber = cc,
                ControllerValue = val
            };
        }

    }
}
