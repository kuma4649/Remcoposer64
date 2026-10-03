using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlControlChangeEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.ControlChange;

        public static readonly int CONTROLLERNUMBER_MAXLEN = 4;
        public static readonly string CONTROLLERNUMBER_SPACE = "    ";
        public static readonly int CONTROLLERVALUE_MAXLEN = 3;
        public static readonly string CONTROLLERVALUE_SPACE = "    ";

        public MdlControlChangeEvent(EventManager parent) : base(parent)
        {
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3) return 2;   // ST
            if (rawPos == 3) return 3;  // ControllerNumber
            return 4;                   // ControllerValue
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;
            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            MIDIControlChangeEvent ce = (MIDIControlChangeEvent)me;
            switch (cursor.Position)
            {
                case 2: // ST
                    me.editVal = ST_SPACE;
                    me.virVal = ce.ST.ToString().PadLeft(ST_MAXLEN);
                    ce.backupST = ce.ST;
                    return true;

                case 3: // ControllerNumber
                    me.editVal = CONTROLLERNUMBER_SPACE;
                    me.virVal = ce.ControllerNumber.ToString().PadLeft(CONTROLLERNUMBER_MAXLEN);
                    ce.backupControllerNumber = ce.ControllerNumber;
                    return true;

                case 4: // ControllerValue
                    me.editVal = CONTROLLERVALUE_SPACE;
                    me.virVal = ce.ControllerValue.ToString().PadLeft(CONTROLLERVALUE_MAXLEN);
                    ce.backupControllerValue = ce.ControllerValue;
                    return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(MIDIEvent me, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            if (!me.editing) return true;

            if (keyChar == '\b' && me.editVal.Length > 0)
            {
                me.editVal = "";
                return false;
            }

            switch (cursor.Position)
            {
                case 2: UpdateActiveControlChangeST((MIDIControlChangeEvent)me, keyChar, keyPattern, cursor); break;
                case 3: UpdateActiveControlChangeNumber((MIDIControlChangeEvent)me, keyChar, keyPattern, cursor); break;
                case 4: UpdateActiveControlChangeValue((MIDIControlChangeEvent)me, keyChar, keyPattern, cursor); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!me.editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !me.editing;
        }

        public void UpdateActiveControlChangeST(MIDIControlChangeEvent ce, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;

            // ★ Enter が押された場合 → ST確定処理へ
            if (keyChar == '\r')
            {
                // ST の妥当性チェック（数字のみ）
                if (!Common.TryParseEither(ce.editVal, ce.virVal, out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                ni = newST;
                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2Number(ce, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2Number(ce, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2Number(ce, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2Number(ce, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2Number(ce, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2Number(ce, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (ce.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = ST_SPACE + ce.editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE))
                ce.editVal = s;
            else if (s.IsEmptyOrWhite())
                ce.editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (ce.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(ce.editVal);
                if (parent.parent.OverwriteMode)
                    MoveNext_ST2Number(ce, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2Number(ce, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2Number(
            MIDIControlChangeEvent ce,
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
                    ce,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,                             // 次のカーソル位置（ST → ControllerNumber）
                    CONTROLLERNUMBER_SPACE,        // editVal 初期化
                    ce.ControllerNumber.ToString() // virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = ce.backupST;
                // ★ ③ 値を更新（ここで初めて更新する）
                ce.ST = newST;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit(ce);

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

        private void UpdateActiveControlChangeNumber(MIDIControlChangeEvent ce, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → ControllerNumber確定処理
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(ce.editVal, ce.virVal, out ni) || !ni.IsBetween(MIDIControlChangeEvent.Controller_MIN, MIDIControlChangeEvent.Controller_MAX))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (ce.editVal.Trim() + keyChar).PadLeft(CONTROLLERNUMBER_MAXLEN);
                s = s.Substring(s.Length - CONTROLLERNUMBER_MAXLEN, CONTROLLERNUMBER_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = CONTROLLERNUMBER_SPACE + ce.editVal;
                s = s.Substring(s.Length - CONTROLLERNUMBER_MAXLEN - 1, CONTROLLERNUMBER_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(MIDIControlChangeEvent.Controller_MIN, MIDIControlChangeEvent.Controller_MAX))
                ce.editVal = s;
            else if (s.IsEmptyOrWhite())
                ce.editVal = s;
            else
                WinApi.Beep();

            if (!parent.parent.CCcandidateForm.Visible)
            {
                var pos = parent.parent.PointToScreenFromCursorInfo(cursorInfo);
                parent.parent.CCcandidateForm.Location = pos;
                parent.parent.CCcandidateForm.Visible = true;
            }
            parent.parent.CCcandidateForm.FillCandidates(ce.editVal.Trim());

            // ★ 最大文字数入力時の自動確定処理
            if (ce.editVal.Trim().Length == CONTROLLERNUMBER_MAXLEN)
            {
                if (!int.TryParse(ce.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();

                if (parent.parent.OverwriteMode)
                    MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }
        }

        public void MoveNext_Number2Value(
            MIDIControlChangeEvent ce,
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
                    ce,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,                              // 次のカーソル位置（ControllerNumber → ControllerValue）
                    CONTROLLERVALUE_SPACE,          // editVal 初期化
                    ce.ControllerValue.ToString()   // virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldNumber = ce.backupControllerNumber;
                // ★ ③ 値を更新（ここで初めて更新する）
                ce.ControllerNumber = newControllerNumber;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit(ce);

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

        private void UpdateActiveControlChangeValue(MIDIControlChangeEvent ce, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → ControllerValue確定処理
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(ce.editVal, ce.virVal, out ni) || !ni.IsBetween(MIDIControlChangeEvent.Controller_MIN, MIDIControlChangeEvent.Controller_MAX))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (ce.editVal.Trim() + keyChar).PadLeft(CONTROLLERVALUE_MAXLEN);
                s = s.Substring(s.Length - CONTROLLERVALUE_MAXLEN, CONTROLLERVALUE_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = CONTROLLERVALUE_SPACE + ce.editVal;
                s = s.Substring(s.Length - CONTROLLERVALUE_MAXLEN - 1, CONTROLLERVALUE_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(MIDIControlChangeEvent.Controller_MIN, MIDIControlChangeEvent.Controller_MAX))
                ce.editVal = s;
            else if (s.IsEmptyOrWhite())
                ce.editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (ce.editVal.Trim().Length == CONTROLLERVALUE_MAXLEN)
            {
                if (!int.TryParse(ce.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();

                if (parent.parent.OverwriteMode)
                    MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_Value2NextEvent(
            MIDIControlChangeEvent ce,
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
                    ce,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    0,                          // 次のカーソル位置（次イベントのホームポジション）
                    "",                         // editVal 初期化
                    ""                          // virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldValue = ce.backupControllerValue;

                // ★ ③ 値を更新（ここで初めて更新する）
                ce.ControllerValue = newControllerValue;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit(ce);

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
            MIDIControlChangeEvent ce,
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

            ApplyEditValAndVirVal(ce,action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MIDIControlChangeEvent ce,
            MoveNextCursorAction action,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                case MoveNextCursorAction.PreviousEvent:
                    ce.editVal = "";
                    ce.virVal = "";
                    break;

                case MoveNextCursorAction.NextField:
                    ce.editVal = nextEditVal;
                    ce.virVal = nextVirVal;
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

        private void commit(MIDIControlChangeEvent ce)
        {
            ce.editing = false;

            MIDIControlChangeEvent.oST = ce.ST;
            MIDIControlChangeEvent.oControllerNumber = ce.ControllerNumber;
            MIDIControlChangeEvent.oControllerValue = ce.ControllerValue;
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDIControlChangeEvent ce = (MIDIControlChangeEvent)me;
            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 2: // ST
                        if (!int.TryParse(ce.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(ce.editVal) && !int.TryParse(ce.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_ST2Number(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 3: // ControllerNumber
                        if (!int.TryParse(ce.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(ce.editVal) && !int.TryParse(ce.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_Number2Value(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 4: // ControllerValue
                        if (!int.TryParse(ce.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(ce.editVal) && !int.TryParse(ce.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_Value2NextEvent(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            // Enter / Arrow keys の場合は UpdateActiveXXX('\r') を呼ぶ
            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveControlChangeST(ce, '\r', keyPattern, cursorInfo);
                    break;

                case 3:
                    UpdateActiveControlChangeNumber(ce, '\r', keyPattern, cursorInfo);
                    break;

                case 4:
                    UpdateActiveControlChangeValue(ce, '\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ((MIDIControlChangeEvent)me).ST.ToString(),
                3 => ((MIDIControlChangeEvent)me).ControllerNumber.ToString(),
                4 => ((MIDIControlChangeEvent)me).ControllerValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIControlChangeEvent ce = (MIDIControlChangeEvent)me;
            return $"{ce.Meas + 1},{ce.Step + 1}: ControlChange: {ce.ST},{ce.ControllerNumber},{ce.ControllerValue}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 3) return null;
            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int cc)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[2], out int val)) { WinApi.Beep(); return null; }

            return new MIDIControlChangeEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                ControllerNumber = cc,
                ControllerValue = val
            };
        }

        public override void CopyFrom(MIDIEvent srcMe, MIDIEvent otherMe, MdlEvent otherMdle)
        {
            base.CopyFrom(srcMe, otherMe, otherMdle);

            var n = (MIDIControlChangeEvent)otherMe;
            n.CopyFrom(otherMe);

            //((MIDIControlChangeEvent)srcMe).GT = n.GT;
            ((MIDIControlChangeEvent)srcMe).ControllerNumber = n.ControllerNumber;
            ((MIDIControlChangeEvent)srcMe).ControllerValue = n.ControllerValue;
        }

    }

}
