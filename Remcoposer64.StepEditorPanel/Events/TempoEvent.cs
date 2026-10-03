using Remcoposer64.StepEditorPanelControl.Command;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class TempoEvent : Event
    {
        public override EventType Type => EventType.Tempo;
        public const int TEMPO_MAXLEN = 5;          // TEMPO の最大桁数（99999 → 5桁）
        public const string TEMPO_SPACE = "     ";  // 5桁分の空白

        public static readonly int TEMPOMin = 0;
        public static readonly int TEMPOMax = 99999;

        public int TempoValue { get; set; } = 120;
        
        private int backupTempoValue;
        private int backupST;

        private static int oTempoValue;

        public bool IsValidDisplayValue(int v)
        {
                return v >= TEMPOMin && v <= TEMPOMax;
        }

        public TempoEvent(EventManager parent) : base(parent)
        {
            ST = oST;
            TempoValue = oTempoValue;
        }

        public override Event Clone()
        {
            return new TempoEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                TempoValue = this.TempoValue,

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal
            };
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3)
                return 2;   // ST

            return 4;       // TempoValue (VEL列)
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            if (cursor.Position == 2)   // ST
            {
                editVal = ST_SPACE;
                virVal = ST.ToString().PadLeft(ST_MAXLEN);
                backupST = ST;
                return true;
            }
            else if (cursor.Position == 4) // TempoValue
            {
                editVal = TEMPO_SPACE;
                virVal = TempoValue.ToString().PadLeft(TEMPO_MAXLEN);
                backupTempoValue = TempoValue;
                return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            if (!editing) return true;

            if (keyChar == '\b' && editVal.Length > 0)
            {
                editVal = "";
                return false;
            }

            switch (cursorInfo.Position)
            {
                case 2: UpdateActiveTempoST(keyChar, keyPattern, cursorInfo); break;
                case 4: UpdateActiveTempoValue(keyChar, keyPattern, cursorInfo); break;
            }

            if (!editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !editing;
        }

        private void UpdateActiveTempoST(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
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
                        MoveNext_ST2TempoValue(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2TempoValue(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2TempoValue(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2TempoValue(cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2TempoValue(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2TempoValue(cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

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
                    MoveNext_ST2TempoValue(cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2TempoValue(cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2TempoValue(
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
                    2,                          // ST(2) → TempoValue(4) なので +2
                    VEL_SPACE,                  // editVal 初期化
                    TempoValue.ToString()// virVal 初期化
                );

                // ★ ② Undo/Redo 用 old/new 値
                int oldST = backupST;

                // ★ ③ 値を更新
                ST = newST;

                // ★ ④ newEditing=false のとき commit()
                if (!newEditing) commit();

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

        private void UpdateActiveTempoValue(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // ★ Enter押下 → TempoValue確定処理
            if (keyChar == '\r')
            {
                // editVal または virVal を数値化
                if (!Common.TryParseEither(editVal, virVal, out ni) || !IsValidDisplayValue(ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                // Enter / Right → 次イベントへ
                if (keyPattern == CommitKeyPattern.EnterKey ||
                    keyPattern == CommitKeyPattern.RightKey)
                {
                    MoveNext_TempoValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                // Left → 同じフィールドに留まる
                else if (keyPattern == CommitKeyPattern.LeftKey)
                {
                    MoveNext_TempoValue(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                }
                // Down → 次イベント
                else if (keyPattern == CommitKeyPattern.DownKey)
                {
                    MoveNext_TempoValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                // Up → 前イベント
                else if (keyPattern == CommitKeyPattern.UpKey)
                {
                    MoveNext_TempoValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                }

                return;
            }

            // ★ 通常入力処理（数字 or '-'）
            string s = "";
            if (char.IsDigit(keyChar) || keyChar == '-' || keyChar == '+')
            {
                s = (editVal.Trim() + keyChar).PadLeft(TEMPO_MAXLEN);
                s = s.Substring(s.Length - TEMPO_MAXLEN, TEMPO_MAXLEN);
            }
            else if (keyChar == 0) // Deleteキー
            {
                s = TEMPO_SPACE + editVal;
                s = s.Substring(s.Length - TEMPO_MAXLEN - 1, TEMPO_MAXLEN);
            }

            // ★ 入力値の妥当性チェック
            if (s.Trim() == "-" || s.Trim() == "+")
            {
                // 符号単体は許可（まだ数値ではない）
                editVal = s;
            }
            else if (int.TryParse(s, out ni) && IsValidDisplayValue(ni))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == TEMPO_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                MoveNext_TempoValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
            }
        }

        private void MoveNext_TempoValue(CursorInfo cursorInfo, CursorInfo newCursorInfo, int dispValue, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                // カーソル移動
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 0, "", "");

                // Undo/Redo 用 old/new 値
                object oldValue = backupTempoValue;
                object newValue = dispValue;

                // 値を更新
                TempoValue = dispValue;

                if (!newEditing) commit();

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

        private void commit()
        {
            editing = false;

            oST = ST;
            oTempoValue = TempoValue;
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
                    case 2:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_ST2TempoValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 4:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 0;
                            }
                        MoveNext_TempoValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveTempoST('\r', keyPattern, cursorInfo);
                    break;
                case 4:
                    UpdateActiveTempoValue('\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ST.ToString(),
                4 => TempoValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: TempoEvent: {ST}, {TempoValue}";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 2) return null;

            if (!int.TryParse(vals[0], out int st)) return null;
            if (!int.TryParse(vals[1], out int tempo)) return null;

            return new TempoEvent(parent)
            {
                Meas = meas,
                Step = step,
                ST = st,
                TempoValue = tempo
            };
        }

    }
}
