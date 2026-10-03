using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlChannelPressureEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.ChannelAfterTouch;

        public static readonly int CHANNELPRESSURE_MAXLEN = 4;
        public static readonly string CHANNELPRESSURE_SPACE = "    ";



        public MdlChannelPressureEvent(EventManager parent) : base(parent)
        {
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            // 3未満なら 2(=ST) にマッピング
            if (rawPos < 3)
                return 2;

            // 3以上なら 3(=PressureValue(GT)) にマッピング
            return 3;
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            MIDIChannelPressureEvent ce = (MIDIChannelPressureEvent)me;
            // 編集開始
            if (cursor.Position == 2)   // ST
            {
                me.editVal = ST_SPACE;
                me.virVal = ce.ST.ToString().PadLeft(ST_MAXLEN);
                ce.backupST = ce.ST;
                return true;
            }
            else if (cursor.Position == 3) // PressureValue
            {
                me.editVal = CHANNELPRESSURE_SPACE;
                me.virVal = ce.PressureValue.ToString().PadLeft(CHANNELPRESSURE_MAXLEN);
                ce.backupPressureValue = ce.PressureValue;
                return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(MIDIEvent me, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            if (!me.editing) return true;

            if (keyChar == '\b' && me.editVal.Length > 0)
            {
                me.editVal = "";
                return false; // バックスペースまたはスラッシュが押された場合、編集を終了せずに編集値をクリア
            }

            switch (cursorInfo.Position)
            {
                case 2: UpdateActiveChannelPressureST((MIDIChannelPressureEvent)me, keyChar, keyPattern, cursorInfo); break;
                case 3: UpdateActiveChannelPressurePressureValue((MIDIChannelPressureEvent)me, keyChar, keyPattern, cursorInfo); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!me.editing)
                if (!parent.parent.OverwriteMode) parent.parent.cursor.Position = 0;

            return !me.editing;
        }

        public void UpdateActiveChannelPressureST(MIDIChannelPressureEvent ce, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
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
                    // OverwriteMode なら次イベントへ
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (ce.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) //Deleteキーが押された場合、editValの末尾を削除(Backspaceキーは直前の処理で処理済み)
            {
                s = ST_SPACE + ce.editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE)) ce.editVal = s;
            else if (s.IsEmptyOrWhite()) ce.editVal = s;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (ce.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(ce.editVal);
                if (parent.parent.OverwriteMode) MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2PressureValue(ce, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2PressureValue(
            MIDIChannelPressureEvent ce,
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
                    1,                          // 次のカーソル位置
                    CHANNELPRESSURE_SPACE,                   // editVal 初期化
                    ce.PressureValue.ToString()    // virVal 初期化
                );

                // Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = ce.backupST;
                // 値を更新（ここで初めて更新する）
                ce.ST = newST;

                if (!newEditing) commit(ce);

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

        private void UpdateActiveChannelPressurePressureValue(MIDIChannelPressureEvent ce, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(ce.editVal, ce.virVal, out ni) || !ni.IsBetween(0, 127))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                {
                    if (parent.parent.OverwriteMode) MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (ce.editVal.Trim() + keyChar).PadLeft(CHANNELPRESSURE_MAXLEN);
                s = s.Substring(s.Length - CHANNELPRESSURE_MAXLEN, CHANNELPRESSURE_MAXLEN);
            }
            else if (keyChar == 0) //Deleteキーが押された場合、editValの末尾を削除(Backspaceキーは直前の処理で処理済み)
            {
                s = CHANNELPRESSURE_SPACE + ce.editVal;
                s = s.Substring(s.Length - CHANNELPRESSURE_MAXLEN - 1, CHANNELPRESSURE_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(MIDIChannelPressureEvent.PressureValue_MINVALUE, MIDIChannelPressureEvent.PressureValue_MAXVALUE)) ce.editVal = s;
            else if (s.IsEmptyOrWhite()) ce.editVal = s;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (ce.editVal.Trim().Length == CHANNELPRESSURE_MAXLEN)
            {
                if (!int.TryParse(ce.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }

        }

        public void MoveNext_PressureValue(MIDIChannelPressureEvent ce, CursorInfo cursorInfo, CursorInfo newCursorInfo, int pcNum, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(ce,cursorInfo, newCursorInfo, action, 0, "", "");

                // Undo/Redo 用 old/new 値
                object oldValue = ce.PressureValue;
                object newValue = pcNum;

                // 値を更新
                ce.PressureValue = pcNum;

                if (!newEditing) commit(ce);

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
            MIDIChannelPressureEvent ce,
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

            ApplyEditValAndVirVal(ce, action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MIDIChannelPressureEvent ce,
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

        private void commit(MIDIChannelPressureEvent ce)
        {
            ce.editing = false;

            MIDIChannelPressureEvent.oST = ce.ST;
            MIDIChannelPressureEvent.oPressureValue = ce.PressureValue;
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDIChannelPressureEvent ce = (MIDIChannelPressureEvent)me;
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
                                ni = 120;
                            }
                        MoveNext_ST2PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 3:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_PressureValue(ce, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveChannelPressureST(ce, '\r', keyPattern, cursorInfo);
                    break;
                case 3:
                    UpdateActiveChannelPressurePressureValue(ce, '\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ((MIDIChannelPressureEvent)me).ST.ToString(),
                3 => ((MIDIChannelPressureEvent)me).PressureValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIChannelPressureEvent ce = (MIDIChannelPressureEvent)me;
            return $"{ce.Meas + 1},{ce.Step + 1}: ChannelPressure: {ce.ST}, {ce.PressureValue}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 2) return null;

            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int pv)) { WinApi.Beep(); return null; }

            return new MIDIChannelPressureEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                PressureValue = pv
            };
        }

        public override void CopyFrom(MIDIEvent srcMe, MIDIEvent otherMe, MdlEvent otherMdle)
        {
            base.CopyFrom(srcMe, otherMe, otherMdle);

            var n = (MIDIChannelPressureEvent)otherMe;
            n.CopyFrom(otherMe);

            ((MIDIChannelPressureEvent)srcMe).PressureChannel = n.PressureChannel;
            ((MIDIChannelPressureEvent)srcMe).PressureValue = n.PressureValue;
        }

    }
}
