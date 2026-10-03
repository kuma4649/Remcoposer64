using Remcoposer64.StepEditorPanelControl.Command;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class SysExEvent : Event
    {
        public SysExEvent(EventManager parent) : base(parent)
        {
        }

        public override EventType Type => EventType.SysEx;

        public byte[] Data { get; set; } = Array.Empty<byte>();

        public string DataString => BitConverter.ToString(Data).Replace("-", " ");
        public string Name { get; set; } = "";

        public int backupST;


        public override Event Clone()
        {
            return new SysExEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,
                Data = this.Data,
                Name = this.Name
            };
        }

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!editing) return;

            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                editing = false;

                // 入力値を反映（-1 はUndoRedoのEditSameMEventCommandでやるので不要）
                if (!int.TryParse(editVal, out int newST)) newST = 0;
                int oldST = backupST;

                // カーソル位置を維持
                var newCursorInfo = new CursorInfo();
                newCursorInfo.Index = cursorInfo.Index;
                newCursorInfo.Position = cursorInfo.Position;

                // Undo/Redo
                ApplyUndoRedo(
                    // old block
                    cursorInfo,
                    oldST,
                    true,
                    // new block
                    newCursorInfo,
                    newST,
                    false
                );

                return;
            }

            switch (cursorInfo.Position)
            {
                case 0:
                case 1:
                case 2:
                    UpdateActiveSysExST('\r', keyPattern, cursorInfo);
                    break;
                case 3:
                case 4:
                    //UpdateActiveSysExExclusive('\r', keyPattern, cursorInfo);
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
            var cmd = new EditSysExEventCommand(
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




        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            editing = true;
            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);

            // ST 編集開始
            if (cursor.Position == 2)   // ST
            {
                editVal = ST_SPACE;
                virVal = ST.ToString().PadLeft(ST_MAXLEN);
                backupST = ST;
                return true;
            }
            else if (cursor.Position == 3) // Exclusive
            {
                // Exclusive 編集は後で実装
                editVal = GT_VEL_SPACE;
                virVal = DataString;
                return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            if (!editing) return true;

            // ★ Backspace / スラッシュ
            if (keyChar == '\b' || keyChar == '/')
            {
                editVal = "";
                return false;
            }

            switch (cursor.Position)
            {
                case 2: UpdateActiveSysExST(keyChar, keyPattern, cursor); break;
                case 3:
                    // Exclusive は内部編集しない → 外部編集へ
                    editing = false;
                    parent.parent.keyb.inputState = Keyboard.InputState.None;

                    BeginEditExternal(cursor, parent.parent.keyb);

                    return false; // 内部編集はしない
            }

            // ★ 編集完了したとき
            if (!editing)
            {
                // NoteEvent と同じ動作
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;
            }

            return !editing;
        }

        public void UpdateActiveSysExST(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            string ev;

            // ★ Enter が押された場合 → ST確定処理へ
            if (keyChar == '\r')
            {
                // ST の妥当性チェック（数字のみ）
                if (!int.TryParse(editVal.Trim(), out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                ev = newST.ToString();

                CursorInfo newCursorInfo = new CursorInfo();

                // ★ NoteEvent と同じカーソル移動ロジック
                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    // OverwriteMode なら次イベントへ
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2Exclusive(cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2Exclusive(cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                {
                    MoveNext_ST2Exclusive(cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)
                {
                    MoveNext_ST2Exclusive(cursor, newCursorInfo, ev, true, MoveNextCursorAction.StayOnField);
                }
                else if (keyPattern == CommitKeyPattern.DownKey)
                {
                    MoveNext_ST2Exclusive(cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.UpKey)
                {
                    MoveNext_ST2Exclusive(cursor, newCursorInfo, ev, false, MoveNextCursorAction.PreviousEvent);
                }

                return;
            }

            // ★ 通常入力処理（数字のみ）
            if (char.IsDigit(keyChar))
            {
                ev = (editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                ev = ev.Substring(ev.Length - ST_MAXLEN, ST_MAXLEN);
                editVal = ev;
            }
            else if (keyChar == '\b')
            {
                ev = ST_SPACE + editVal;
                ev = ev.Substring(ev.Length - ST_MAXLEN - 1, ST_MAXLEN);
                editVal = ev;
            }

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                if (parent.parent.OverwriteMode) MoveNext_ST2Exclusive(cursor, newCursorInfo, editVal.Trim(), false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2Exclusive(cursor, newCursorInfo, editVal.Trim(), true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2Exclusive(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            string newST,
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
                    3,                          // Exclusive のカーソル位置
                    GT_VEL_SPACE,               // editVal 初期化（Exclusive は後で実装）
                    DataString                  // virVal 初期化
                );

                // ★ ② newEditing=false のとき commit()
                if (!newEditing)
                    Commit(cursorInfo, CommitKeyPattern.EnterKey);

                // ★ ③ Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = backupST;

                // ★ ④ 新しい ST を計算
                int newST_i = int.Parse(newST);

                // ★ ⑤ 値を更新（ここで初めて更新する）
                ST = newST_i;

                // コマンド発行
                ApplyUndoRedo(
                    // old block
                    cursorInfo,
                    oldST,
                    true,
                    // new block
                    newCursorInfo,
                    newST_i,
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
            int nextFieldPosition,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                    newCursorInfo.Index = cursorInfo.Index + 1;
                    newCursorInfo.EventNode = cursorInfo.EventNode.Next;
                    newCursorInfo.Position = cursorInfo.Position;

                    // ★ Exclusive 内部カーソルはイベント移動でリセット
                    //exclusiveCursorIndex = 0;
                    break;

                case MoveNextCursorAction.PreviousEvent:
                    newCursorInfo.Index = cursorInfo.Index - 1;
                    newCursorInfo.EventNode = cursorInfo.EventNode.Previous;
                    newCursorInfo.Position = cursorInfo.Position;

                    // ★ Exclusive 内部カーソルはイベント移動でリセット
                    //exclusiveCursorIndex = 0;
                    break;

                case MoveNextCursorAction.NextField:
                    newCursorInfo.Index = cursorInfo.Index;
                    newCursorInfo.EventNode = cursorInfo.EventNode;
                    newCursorInfo.Position = nextFieldPosition;

                    if (newCursorInfo.Position == 3)
                    {
                        // 内部編集モードを終了
                        editing = false;
                        parent.parent.keyb.inputState = Keyboard.InputState.None;

                        // 外部 SysEx 編集パネルを開く
                        BeginEditExternal(newCursorInfo, parent.parent.keyb);
                    }

                    break;

                case MoveNextCursorAction.StayOnField:
                    newCursorInfo.Index = cursorInfo.Index;
                    newCursorInfo.EventNode = cursorInfo.EventNode;
                    newCursorInfo.Position = cursorInfo.Position;

                    // ★ Exclusive の内部カーソルだけを動かす
                    //if (cursorInfo.Position == 3)
                      //  MoveExclusiveInternalCursor(nextFieldOffset);

                    break;
            }

            // 値のセットは NoteEvent と同じ抽象化を使う
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


        public bool BeginEditExternal(CursorInfo cursor, Keyboard keyb)
        {
            // 編集開始フラグ（描画側で反転したいなら使う）
            this.editing = true;

            // 外部プログラムに渡すデータ
            var request = new SysExEditRequest
            {
                RawData = this.Data,
                Name = this.Name,
                Measure = this.Meas,
                Step = this.Step
            };

            var result = parent.parent.SysExEditRequested?.Invoke(request);

            if (result == null)
            {
                // 編集キャンセル
                this.editing = false;
                return false;
            }

            // ★ 結果を反映
            this.Data = result.RawData;
            this.Name = result.Name;

            // 編集終了
            this.editing = false;

            return true;
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            // 3未満なら 2(=ST) にマッピング
            if (rawPos < 3)
                return 2;

            // 3以上なら 3(=Exclusive(GT)) にマッピング
            return 3;
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                < 3 => ST.ToString(),
                < 5 => DataString,
                _ => "?"
            };
        }

        public override string ToClipboardString()
        {
            string hex = BitConverter.ToString(Data).Replace("-", " ");
            return $"{Meas + 1},{Step + 1}: SysEx: {ST}, {hex}";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',', 2);
            if (vals.Length < 2) return null;

            int st = int.Parse(vals[0]);
            var bytes = ParseHexBytes(vals[1]);

            return new SysExEvent(parent)
            {
                Meas = meas,
                Step = step,
                ST = st,
                Data = bytes
            };
        }

    }
}
