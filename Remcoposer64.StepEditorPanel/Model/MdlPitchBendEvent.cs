using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlPitchBendEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.PitchBend;

        public const int PB_MAXLEN = 5;          // PitchBend の最大桁数（Raw 16383 → 5桁）
        public const string PB_SPACE = "     ";  // 5桁分の空白

        public MdlPitchBendEvent(EventManager parent) : base(parent)
        {
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3)
                return 2;   // ST

            return 4;       // PitchValue (VEL列)
        }

        public override bool BeginEdit(MIDIEvent me,CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            MIDIPitchBendEvent pe = (MIDIPitchBendEvent)me;
            if (cursor.Position == 2)   // ST
            {
                me.editVal = ST_SPACE;
                me.virVal = pe.ST.ToString().PadLeft(ST_MAXLEN);
                pe.backupST = pe.ST;
                return true;
            }
            else if (cursor.Position == 4) // PitchValue
            {
                int disp = pe.GetDisplayValue();
                me.editVal = PB_SPACE;
                me.virVal = disp.ToString().PadLeft(PB_MAXLEN);
                pe.backupPitchValue = pe.PitchValue;
                return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(MIDIEvent me,char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            if (!me.editing) return true;

            if (keyChar == '\b' && me.editVal.Length > 0)
            {
                me.editVal = "";
                return false;
            }

            switch (cursorInfo.Position)
            {
                case 2: UpdateActivePitchBendST((MIDIPitchBendEvent)me, keyChar, keyPattern, cursorInfo); break;
                case 4: UpdateActivePitchBendValue((MIDIPitchBendEvent)me, keyChar, keyPattern, cursorInfo); break;
            }

            if (!me.editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !me.editing;
        }

        private void UpdateActivePitchBendST(MIDIPitchBendEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;

            // ★ Enter が押された場合 → ST確定処理へ
            if (keyChar == '\r')
            {
                // ST の妥当性チェック（数字のみ）
                if (!Common.TryParseEither(pe.editVal, pe.virVal, out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                ni = newST;
                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2PitchValue(pe,cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = ST_SPACE + pe.editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE))
                pe.editVal = s;
            else if (s.IsEmptyOrWhite())
                pe.editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (pe.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(pe.editVal);
                if (parent.parent.OverwriteMode)
                    MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2PitchValue(pe, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2PitchValue(
            MIDIPitchBendEvent pe,
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
                    pe,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    2,                          // ST(2) → PitchValue(4) なので +2
                    PB_SPACE,                  // editVal 初期化
                    pe.GetDisplayValue().ToString()// virVal 初期化（Centered対応）
                );

                // ★ ② Undo/Redo 用 old/new 値
                int oldST = pe.backupST;

                // ★ ③ 値を更新
                pe.ST = newST;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit(pe);

                // ★ ⑤ コマンド発行
                ApplyUndoRedo(
                    cursorInfo,
                    oldST,
                    true,
                    newCursorInfo,
                    newST,
                    newEditing
                );
            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void UpdateActivePitchBendValue(MIDIPitchBendEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → PitchValue確定処理
            if (keyChar == '\r')
            {
                // editVal または virVal を数値化
                if (!Common.TryParseEither(pe.editVal, pe.virVal, out ni) || !pe.IsValidDisplayValue(ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                // Enter / Right → 次イベントへ
                if (keyPattern == CommitKeyPattern.EnterKey ||
                    keyPattern == CommitKeyPattern.RightKey)
                {
                    MoveNext_PitchValue(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                // Left → 同じフィールドに留まる
                else if (keyPattern == CommitKeyPattern.LeftKey)
                {
                    MoveNext_PitchValue(pe, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                }
                // Down → 次イベント
                else if (keyPattern == CommitKeyPattern.DownKey)
                {
                    MoveNext_PitchValue(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                // Up → 前イベント
                else if (keyPattern == CommitKeyPattern.UpKey)
                {
                    MoveNext_PitchValue(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                }

                return;
            }

            // ★ 通常入力処理（数字 or '-'）
            string s = "";
            if (char.IsDigit(keyChar) || keyChar == '-' || keyChar == '+')
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(PB_MAXLEN);
                s = s.Substring(s.Length - PB_MAXLEN, PB_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = PB_SPACE + pe.editVal;
                s = s.Substring(s.Length - PB_MAXLEN - 1, PB_MAXLEN);
            }

            // ★ 入力値の妥当性チェック
            if (s.Trim() == "-" || s.Trim() == "+")
            {
                // 符号単体は許可（まだ数値ではない）
                pe.editVal = s;
            }
            else if (int.TryParse(s, out ni) && pe.IsValidDisplayValue(ni))
                pe.editVal = s;
            else if (s.IsEmptyOrWhite())
                pe.editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (pe.editVal.Trim().Length == PB_MAXLEN)
            {
                if (!int.TryParse(pe.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                MoveNext_PitchValue(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
            }
        }

        private void MoveNext_PitchValue(MIDIPitchBendEvent pe, CursorInfo cursorInfo, CursorInfo newCursorInfo, int dispValue, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                // カーソル移動
                ApplyCursorMove(pe,cursorInfo, newCursorInfo, action, 0, "", "");

                // 表示値 → Raw値へ変換
                int rawValue = pe.ToRawValue(dispValue);

                // Undo/Redo 用 old/new 値
                object oldValue = pe.backupPitchValue;
                object newValue = rawValue;

                // 値を更新
                pe.PitchValue = rawValue;

                if (!newEditing) commit(pe);

                // Undo/Redo コマンド発行
                ApplyUndoRedo(
                    cursorInfo,
                    oldValue,
                    true,
                    newCursorInfo,
                    newValue,
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
            MIDIPitchBendEvent pe,
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

            ApplyEditValAndVirVal(pe,action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MIDIPitchBendEvent pe,
            MoveNextCursorAction action,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                case MoveNextCursorAction.PreviousEvent:
                    pe.editVal = "";
                    pe.virVal = "";
                    break;

                case MoveNextCursorAction.NextField:
                    pe.editVal = nextEditVal;
                    pe.virVal = nextVirVal;
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
            var cmd = new EditPitchBendEventCommand(
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

        private void commit(MIDIPitchBendEvent pe)
        {
            pe.editing = false;

            MIDIPitchBendEvent.oST = pe.ST;
            MIDIPitchBendEvent.oPitchValue = pe.PitchValue;
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDIPitchBendEvent pe = (MIDIPitchBendEvent)me;
            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 2:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_ST2PitchValue(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 4:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_PitchValue(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActivePitchBendST(pe,'\r', keyPattern, cursorInfo);
                    break;
                case 4:
                    UpdateActivePitchBendValue(pe,'\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ((MIDIPitchBendEvent)me).ST.ToString(),
                4 => ((MIDIPitchBendEvent)me).GetDisplayValue().ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIPitchBendEvent pe = (MIDIPitchBendEvent)me;
            return $"{pe.Meas + 1},{pe.Step + 1}: PitchBend: {pe.ST},{pe.PitchValue}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 2) return null;

            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int pb)) { WinApi.Beep(); return null; }

            return new MIDIPitchBendEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                PitchValue = pb
            };
        }

    }
}
