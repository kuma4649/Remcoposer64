using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlTempoEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.MetaTempo;
        public const int TEMPO_MAXLEN = 5;          // TEMPO の最大桁数（99999 → 5桁）
        public const string TEMPO_SPACE = "     ";  // 5桁分の空白

        public MdlTempoEvent(EventManager parent) : base(parent)
        {
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3)
                return 2;   // ST

            return 4;       // TempoValue (VEL列)
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            MIDITempoEvent te = (MIDITempoEvent)me;
            if (cursor.Position == 2)   // ST
            {
                me.editVal = ST_SPACE;
                me.virVal = te.ST.ToString().PadLeft(ST_MAXLEN);
                te.backupST = te.ST;
                return true;
            }
            else if (cursor.Position == 4) // TempoValue
            {
                me.editVal = TEMPO_SPACE;
                me.virVal = te.TempoValue.ToString().PadLeft(TEMPO_MAXLEN);
                te.backupTempoValue = te.TempoValue;
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
                case 2: UpdateActiveTempoST((MIDITempoEvent)me, keyChar, keyPattern, cursorInfo); break;
                case 4: UpdateActiveTempoValue((MIDITempoEvent)me, keyChar, keyPattern, cursorInfo); break;
            }

            if (!me.editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !me.editing;
        }

        private void UpdateActiveTempoST(MIDITempoEvent te, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;

            // ★ Enter が押された場合 → ST確定処理へ
            if (keyChar == '\r')
            {
                // ST の妥当性チェック（数字のみ）
                if (!Common.TryParseEither(te.editVal, te.virVal, out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                ni = newST;
                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2TempoValue(te,cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (te.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = ST_SPACE + te.editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE))
                te.editVal = s;
            else if (s.IsEmptyOrWhite())
                te.editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (te.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(te.editVal);
                if (parent.parent.OverwriteMode)
                    MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2TempoValue(te, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2TempoValue(
            MIDITempoEvent te,
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
                    te,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    2,                          // ST(2) → TempoValue(4) なので +2
                    TEMPO_SPACE,                  // editVal 初期化
                    te.TempoValue.ToString()// virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値
                int oldST = te.backupST;

                // ★ ③ 値を更新
                te.ST = newST;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit(te);

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

        private void UpdateActiveTempoValue(MIDITempoEvent te, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → TempoValue確定処理
            if (keyChar == '\r')
            {
                // editVal または virVal を数値化
                if (!Common.TryParseEither(te.editVal, te.virVal, out ni) || !te.IsValidDisplayValue(ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                // Enter / Right → 次イベントへ
                if (keyPattern == CommitKeyPattern.EnterKey ||
                    keyPattern == CommitKeyPattern.RightKey)
                {
                    MoveNext_TempoValue(te,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                // Left → 同じフィールドに留まる
                else if (keyPattern == CommitKeyPattern.LeftKey)
                {
                    MoveNext_TempoValue(te, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                }
                // Down → 次イベント
                else if (keyPattern == CommitKeyPattern.DownKey)
                {
                    MoveNext_TempoValue(te, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                // Up → 前イベント
                else if (keyPattern == CommitKeyPattern.UpKey)
                {
                    MoveNext_TempoValue(te, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                }

                return;
            }

            // ★ 通常入力処理（数字 or '-'）
            string s = "";
            if (char.IsDigit(keyChar) || keyChar == '-' || keyChar == '+')
            {
                s = (te.editVal.Trim() + keyChar).PadLeft(TEMPO_MAXLEN);
                s = s.Substring(s.Length - TEMPO_MAXLEN, TEMPO_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = TEMPO_SPACE + te.editVal;
                s = s.Substring(s.Length - TEMPO_MAXLEN - 1, TEMPO_MAXLEN);
            }

            // ★ 入力値の妥当性チェック
            if (s.Trim() == "-" || s.Trim() == "+")
            {
                // 符号単体は許可（まだ数値ではない）
                te.editVal = s;
            }
            else if (int.TryParse(s, out ni) && te.IsValidDisplayValue(ni))
                te.editVal = s;
            else if (s.IsEmptyOrWhite())
                te.editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (te.editVal.Trim().Length == TEMPO_MAXLEN)
            {
                if (!int.TryParse(te.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                MoveNext_TempoValue(te, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
            }
        }

        private void MoveNext_TempoValue(MIDITempoEvent te, CursorInfo cursorInfo, CursorInfo newCursorInfo, int dispValue, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                // カーソル移動
                ApplyCursorMove(te,cursorInfo, newCursorInfo, action, 0, "", "");

                // Undo/Redo 用 old/new 値
                object oldValue = te.backupTempoValue;
                object newValue = dispValue;

                // 値を更新
                te.TempoValue = dispValue;

                if (!newEditing) commit(te);

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
            MIDITempoEvent te,
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

            ApplyEditValAndVirVal(te,action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MIDITempoEvent te,
            MoveNextCursorAction action,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                case MoveNextCursorAction.PreviousEvent:
                    te.editVal = "";
                    te.virVal = "";
                    break;

                case MoveNextCursorAction.NextField:
                    te.editVal = nextEditVal;
                    te.virVal = nextVirVal;
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
            var cmd = new EditTempoEventCommand(
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

        private void commit(MIDITempoEvent te)
        {
            te.editing = false;

            MIDITempoEvent. oST = te.ST;
            MIDITempoEvent.oTempoValue = te.TempoValue;
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDITempoEvent te = (MIDITempoEvent)me;
            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 2:
                        if (!int.TryParse(te.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(te.editVal) && !int.TryParse(te.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_ST2TempoValue(te,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 4:
                        if (!int.TryParse(te.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(te.editVal) && !int.TryParse(te.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_TempoValue(te, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveTempoST(te, '\r', keyPattern, cursorInfo);
                    break;
                case 4:
                    UpdateActiveTempoValue(te, '\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ((MIDITempoEvent)me).ST.ToString(),
                4 => ((MIDITempoEvent)me).TempoValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDITempoEvent te = (MIDITempoEvent)me;
            return $"{te.Meas + 1},{te.Step + 1}: TempoEvent: {te.ST}, {te.TempoValue}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 2) return null;

            if (!int.TryParse(vals[0], out int st)) return null;
            if (!int.TryParse(vals[1], out int tempo)) return null;

            return new MIDITempoEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                TempoValue = tempo
            };
        }

    }

}
