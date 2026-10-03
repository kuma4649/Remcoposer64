using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;


namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlSysExEvent :MdlEvent
    {
        public static readonly string GT_VEL_SPACE = "        ";


        public MdlSysExEvent(EventManager parent) : base(parent)
        {
        }

        public override MIDIEventType Type => MIDIEventType.SysExF0;

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDISysExEvent se = (MIDISysExEvent)me;
            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                me.editing = false;

                // 入力値を反映（-1 はUndoRedoのEditSameMEventCommandでやるので不要）
                if (!int.TryParse(me.editVal, out int newST)) newST = 0;
                int oldST = se.backupST;

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
                    UpdateActiveSysExST(se,'\r', keyPattern, cursorInfo);
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




        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;
            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);

            MIDISysExEvent se = (MIDISysExEvent)me;
            // ST 編集開始
            if (cursor.Position == 2)   // ST
            {
                me.editVal = ST_SPACE;
                me.virVal = se.ST.ToString().PadLeft(ST_MAXLEN);
                se.backupST = se.ST;
                return true;
            }
            else if (cursor.Position == 3) // Exclusive
            {
                // Exclusive 編集は後で実装
                me.editVal = GT_VEL_SPACE;
                me.virVal = se.DataString;
                return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(MIDIEvent me, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            if (!me.editing) return true;

            // ★ Backspace / スラッシュ
            if (keyChar == '\b' || keyChar == '/')
            {
                me.editVal = "";
                return false;
            }

            switch (cursor.Position)
            {
                case 2: UpdateActiveSysExST((MIDISysExEvent)me, keyChar, keyPattern, cursor); break;
                case 3:
                    // Exclusive は内部編集しない → 外部編集へ
                    me.editing = false;
                    parent.parent.keyb.inputState = Keyboard.InputState.None;

                    BeginEditExternal((MIDISysExEvent)me, cursor, parent.parent.keyb);

                    return false; // 内部編集はしない
            }

            // ★ 編集完了したとき
            if (!me.editing)
            {
                // NoteEvent と同じ動作
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;
            }

            return !me.editing;
        }

        public void UpdateActiveSysExST(MIDISysExEvent se, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            string ev;

            // ★ Enter が押された場合 → ST確定処理へ
            if (keyChar == '\r')
            {
                // ST の妥当性チェック（数字のみ）
                if (!int.TryParse(se.editVal.Trim(), out int newST))
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
                        MoveNext_ST2Exclusive(se,cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2Exclusive(se, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                {
                    MoveNext_ST2Exclusive(se, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)
                {
                    MoveNext_ST2Exclusive(se, cursor, newCursorInfo, ev, true, MoveNextCursorAction.StayOnField);
                }
                else if (keyPattern == CommitKeyPattern.DownKey)
                {
                    MoveNext_ST2Exclusive(se, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.UpKey)
                {
                    MoveNext_ST2Exclusive(se, cursor, newCursorInfo, ev, false, MoveNextCursorAction.PreviousEvent);
                }

                return;
            }

            // ★ 通常入力処理（数字のみ）
            if (char.IsDigit(keyChar))
            {
                ev = (se.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                ev = ev.Substring(ev.Length - ST_MAXLEN, ST_MAXLEN);
                se.editVal = ev;
            }
            else if (keyChar == '\b')
            {
                ev = ST_SPACE + se.editVal;
                ev = ev.Substring(ev.Length - ST_MAXLEN - 1, ST_MAXLEN);
                se.editVal = ev;
            }

            // ★ 最大文字数入力時の自動確定処理
            if (se.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                if (parent.parent.OverwriteMode) MoveNext_ST2Exclusive(se, cursor, newCursorInfo, se.editVal.Trim(), false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2Exclusive(se, cursor, newCursorInfo, se.editVal.Trim(), true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2Exclusive(
            MIDISysExEvent se,
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
                    se,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    3,                          // Exclusive のカーソル位置
                    GT_VEL_SPACE,               // editVal 初期化（Exclusive は後で実装）
                    se.DataString                  // virVal 初期化
                );

                // ★ ② newEditing=false のとき commit()
                if (!newEditing)
                    Commit(se, cursorInfo, CommitKeyPattern.EnterKey);

                // ★ ③ Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = se.backupST;

                // ★ ④ 新しい ST を計算
                int newST_i = int.Parse(newST);

                // ★ ⑤ 値を更新（ここで初めて更新する）
                se.ST = newST_i;

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
            MIDISysExEvent se,
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
                        se.editing = false;
                        parent.parent.keyb.inputState = Keyboard.InputState.None;

                        // 外部 SysEx 編集パネルを開く
                        BeginEditExternal(se, newCursorInfo, parent.parent.keyb);
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
            ApplyEditValAndVirVal(se,action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MIDISysExEvent se,
            MoveNextCursorAction action,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                case MoveNextCursorAction.PreviousEvent:
                    se.editVal = "";
                    se.virVal = "";
                    break;

                case MoveNextCursorAction.NextField:
                    se.editVal = nextEditVal;
                    se.virVal = nextVirVal;
                    break;

                case MoveNextCursorAction.StayOnField:
                    // 何もしない
                    break;
            }
        }


        public bool BeginEditExternal(MIDISysExEvent se, CursorInfo cursor, Keyboard keyb)
        {
            // 編集開始フラグ（描画側で反転したいなら使う）
            se.editing = true;

            // 外部プログラムに渡すデータ
            var request = new SysExEditRequest
            {
                RawData = se.Data,
                Name = se.Name,
                Measure = se.Meas,
                Step = se.Step
            };

            var result = parent.parent.SysExEditRequested?.Invoke(request);

            if (result == null)
            {
                // 編集キャンセル
                se.editing = false;
                return false;
            }

            // ★ 結果を反映
            se.Data = result.RawData;
            se.Name = result.Name;

            // 編集終了
            se.editing = false;

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

        public override string GetVal(MIDIEvent me,CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                < 3 => ((MIDISysExEvent)me).ST.ToString(),
                < 5 => ((MIDISysExEvent)me).DataString,
                _ => "?"
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDISysExEvent se = (MIDISysExEvent)me;
            string hex = BitConverter.ToString(se.Data).Replace("-", " ");
            return $"{se.Meas + 1},{se.Step + 1}: SysEx: {se.ST}, {hex}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',', 2);
            if (vals.Length < 2) return null;

            int st = int.Parse(vals[0]);
            var bytes = ParseHexBytes(vals[1]);

            return new MIDISysExEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                Data = bytes
            };
        }

    }
}
