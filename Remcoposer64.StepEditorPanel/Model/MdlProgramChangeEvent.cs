using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlProgramChangeEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.ProgramChange;

        public static readonly int PROGRAMNUMBER_MAXLEN = 4;
        public static readonly string PROGRAMNUMBER_SPACE = "    ";

        public MdlProgramChangeEvent(EventManager parent) : base(parent)
        {
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            // 3未満なら 2(=ST) にマッピング
            if (rawPos < 3)
                return 2;

            // 3以上なら 3(=ProgramNumber(GT)) にマッピング
            return 3;
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            MIDIProgramChangeEvent pe = (MIDIProgramChangeEvent)me;
            // 編集開始
            if (cursor.Position == 2)   // ST
            {
                me.editVal = ST_SPACE;
                me.virVal = pe.ST.ToString().PadLeft(ST_MAXLEN);
                pe.backupST = pe.ST;
                return true;
            }
            else if (cursor.Position == 3) // ProgramNumber
            {
                me.editVal = PROGRAMNUMBER_SPACE;
                me.virVal = pe.ProgramNumber.ToString().PadLeft(PROGRAMNUMBER_MAXLEN);
                pe.backupProgramNumber = pe.ProgramNumber;
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
                case 2: UpdateActiveProgramChangeST((MIDIProgramChangeEvent)me, keyChar, keyPattern, cursorInfo); break;
                case 3: UpdateActiveProgramChangeProgramNumber((MIDIProgramChangeEvent)me, keyChar, keyPattern, cursorInfo); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!me.editing)
                if (!parent.parent.OverwriteMode) parent.parent.cursor.Position = 0;

            return !me.editing;
        }

        public void UpdateActiveProgramChangeST(MIDIProgramChangeEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
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
                    // OverwriteMode なら次イベントへ
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0) //Deleteキーが押された場合、editValの末尾を削除(Backspaceキーは直前の処理で処理済み)
            {
                s = ST_SPACE + pe.editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE)) pe.editVal = s;
            else if (s.IsEmptyOrWhite()) pe.editVal = s;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (pe.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();

                int ev = int.Parse(pe.editVal);
                if (parent.parent.OverwriteMode) MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2ProgramNumber(pe, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2ProgramNumber(
            MIDIProgramChangeEvent pe,
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
                    1,                          // 次のカーソル位置
                    PROGRAMNUMBER_SPACE,        // editVal 初期化
                    pe.ProgramNumber.ToString()    // virVal 初期化
                );

                // Undo/Redo 用 old/new 値（更新前に oldState を作る）
                int oldST = pe.backupST;
                // 値を更新（ここで初めて更新する）
                pe.ST = newST;

                if (!newEditing) commit(pe);

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

                var pos = parent.parent.PointToScreenFromCursorInfo(newCursorInfo);
                parent.parent.PCcandidateForm.Location = pos;
                parent.parent.PCcandidateForm.Visible = true;
            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void UpdateActiveProgramChangeProgramNumber(MIDIProgramChangeEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(pe.editVal, pe.virVal, out ni) || !ni.IsBetween(0, 127))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_ProgramNumber(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                {
                    if (parent.parent.OverwriteMode) MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // ★ 通常入力処理（数字のみ）
            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(PROGRAMNUMBER_MAXLEN);
                s = s.Substring(s.Length - PROGRAMNUMBER_MAXLEN, PROGRAMNUMBER_MAXLEN);
            }
            else if (keyChar == 0) //Deleteキーが押された場合、editValの末尾を削除(Backspaceキーは直前の処理で処理済み)
            {
                s = PROGRAMNUMBER_SPACE + pe.editVal;
                s = s.Substring(s.Length - PROGRAMNUMBER_MAXLEN - 1, PROGRAMNUMBER_MAXLEN);
            }

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(s, out ni) && ni.IsBetween(MIDIProgramChangeEvent.ProgramNumber_MINVALUE,MIDIProgramChangeEvent.ProgramNumber_MAXVALUE)) pe.editVal = s;
            else if (s.IsEmptyOrWhite()) pe.editVal = s;
            else WinApi.Beep();

            if (!parent.parent.PCcandidateForm.Visible)
            {
                var pos = parent.parent.PointToScreenFromCursorInfo(cursorInfo);
                parent.parent.PCcandidateForm.Location = pos;
                parent.parent.PCcandidateForm.Visible = true;
            }
            parent.parent.PCcandidateForm.FillCandidates(pe.editVal.Trim());

            // ★ 最大文字数入力時の自動確定処理
            if (pe.editVal.Trim().Length == PROGRAMNUMBER_MAXLEN)
            {
                if (!int.TryParse(pe.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_ProgramNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }

        }

        public void MoveNext_ProgramNumber(MIDIProgramChangeEvent pe, CursorInfo cursorInfo, CursorInfo newCursorInfo, int pcNum, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(pe,cursorInfo, newCursorInfo, action, 0, "", "");

                // Undo/Redo 用 old/new 値
                object oldValue = pe.ProgramNumber;
                object newValue = pcNum;

                // 値を更新
                pe.ProgramNumber = pcNum;

                if (!newEditing) commit(pe);

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

                parent.parent.PCcandidateForm.Visible = false;

            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }

        private void ApplyCursorMove(
            MIDIProgramChangeEvent pe,
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
            MIDIProgramChangeEvent pe,
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
            var cmd = new EditProgramChangeEventCommand(
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

        private void commit(MIDIProgramChangeEvent pe)
        {
            pe.editing = false;

            MIDIProgramChangeEvent.oST = pe.ST;
            MIDIProgramChangeEvent.oProgramNumber = pe.ProgramNumber;
        }

        public override void Commit(MIDIEvent me,CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDIProgramChangeEvent pe = (MIDIProgramChangeEvent)me;
            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 2:
                        if (!int.TryParse(pe.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(pe.editVal) && !int.TryParse(pe.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_ST2ProgramNumber(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 3:
                        if (!int.TryParse(pe.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(pe.editVal) && !int.TryParse(pe.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_ProgramNumber(pe,cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActiveProgramChangeST(pe,'\r', keyPattern, cursorInfo);
                    break;
                case 3:
                    UpdateActiveProgramChangeProgramNumber(pe,'\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ((MIDIProgramChangeEvent)me).ST.ToString(),
                3 => ((MIDIProgramChangeEvent)me).ProgramNumber.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIProgramChangeEvent pe = (MIDIProgramChangeEvent)me;
            return $"{pe.Meas + 1},{pe.Step + 1}: ProgramChange: {pe.ST}, {pe.ProgramNumber}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 2) return null;

            if (!int.TryParse(vals[0], out int st)) { WinApi.Beep(); return null; }
            if (!int.TryParse(vals[1], out int program)) { WinApi.Beep(); return null; }

            return new MIDIProgramChangeEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                ProgramNumber = program
            };
        }

    }
}
