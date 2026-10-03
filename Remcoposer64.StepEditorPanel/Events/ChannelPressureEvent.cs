using Remcoposer64.StepEditorPanelControl.Command;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class ChannelPressureEvent : Event
    {
        public override EventType Type => EventType.ChannelPressure;
        public static readonly int PressureValue_MINVALUE = 0;
        public static readonly int PressureValue_MAXVALUE = 127;

        public int PressureValue { get; set; } = 64;
        private int backupPressureValue;
        private static int oPressureValue;
        private int backupST;


        public ChannelPressureEvent(EventManager parent) : base(parent)
        {
            ST = oST;
            PressureValue = oPressureValue;
        }

        public override Event Clone()
        {
            return new ChannelPressureEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                PressureValue = this.PressureValue,

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal
            };
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            // 3未満なら 2(=ST) にマッピング
            if (rawPos < 3)
                return 2;

            // 3以上なら 3(=PressureValue(GT)) にマッピング
            return 3;
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            // 編集開始
            if (cursor.Position == 2)   // ST
            {
                editVal = ST_SPACE;
                virVal = ST.ToString().PadLeft(ST_MAXLEN);
                backupST = ST;
                return true;
            }
            else if (cursor.Position == 3) // PressureValue
            {
                editVal = GT_SPACE;
                virVal = PressureValue.ToString().PadLeft(GT_MAXLEN);
                backupPressureValue = PressureValue;
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
                return false; // バックスペースまたはスラッシュが押された場合、編集を終了せずに編集値をクリア
            }

            switch (cursorInfo.Position)
            {
                case 2: UpdateActiveChannelPressureST(keyChar, keyPattern, cursorInfo); break;
                case 3: UpdateActiveChannelPressurePressureValue(keyChar, keyPattern, cursorInfo); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!editing)
                if (!parent.parent.OverwriteMode) parent.parent.cursor.Position = 0;

            return !editing;
        }

        public void UpdateActiveChannelPressureST(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
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
                    // OverwriteMode なら次イベントへ
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2PressureValue(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2PressureValue(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2PressureValue(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2PressureValue(cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2PressureValue(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2PressureValue(cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) //Deleteキーが押された場合、editValの末尾を削除(Backspaceキーは直前の処理で処理済み)
            {
                s = ST_SPACE + editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE)) editVal = s;
            else if (s.IsEmptyOrWhite()) editVal = s;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(editVal);
                if (parent.parent.OverwriteMode) MoveNext_ST2PressureValue(cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2PressureValue(cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2PressureValue(
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
                    1,                          // 次のカーソル位置
                    GT_SPACE,                   // editVal 初期化
                    PressureValue.ToString()    // virVal 初期化
                );

                // Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = backupST;
                // 値を更新（ここで初めて更新する）
                ST = newST;

                if (!newEditing) commit();

                // コマンド発行
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

            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void UpdateActiveChannelPressurePressureValue(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni) || !ni.IsBetween(0, 127))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                {
                    if (parent.parent.OverwriteMode) MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(GT_MAXLEN);
                s = s.Substring(s.Length - GT_MAXLEN, GT_MAXLEN);
            }
            else if (keyChar == 0) //Deleteキーが押された場合、editValの末尾を削除(Backspaceキーは直前の処理で処理済み)
            {
                s = GT_SPACE + editVal;
                s = s.Substring(s.Length - GT_MAXLEN - 1, GT_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(PressureValue_MINVALUE, PressureValue_MAXVALUE)) editVal = s;
            else if (s.IsEmptyOrWhite()) editVal = s;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == GT_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }

        }

        public void MoveNext_PressureValue(CursorInfo cursorInfo, CursorInfo newCursorInfo, int pcNum, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 0, "", "");

                // Undo/Redo 用 old/new 値
                object oldValue = PressureValue;
                object newValue = pcNum;

                // 値を更新
                PressureValue = pcNum;

                if (!newEditing) commit();

                // コマンド発行
                ApplyUndoRedo(
                    // old block
                    cursorInfo,
                    oldValue,
                    true,
                    // new block
                    newCursorInfo,
                    newValue,
                    newEditing
                );

            }
            catch (Exception)
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
            var cmd = new EditChannelPressureEventCommand(
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
            oPressureValue = PressureValue;
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
                                ni = 120;
                            }
                        MoveNext_ST2PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 3:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_PressureValue(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveChannelPressureST('\r', keyPattern, cursorInfo);
                    break;
                case 3:
                    UpdateActiveChannelPressurePressureValue('\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ST.ToString(),
                3 => PressureValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: ChannelPressure: {ST}, {PressureValue}";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 2) return null;

            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int pv)) { WinApi.Beep(); return null; }

            return new ChannelPressureEvent(parent)
            {
                Meas = meas,
                Step = step,
                ST = st,
                PressureValue = pv
            };
        }

    }
}
