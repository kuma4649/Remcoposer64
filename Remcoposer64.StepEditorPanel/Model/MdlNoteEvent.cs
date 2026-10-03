using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlNoteEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.NoteON;

        public static readonly int KEYNUMBER_MAXLEN = 3;
        public static readonly string KEYNUMBER_SPACE = "   ";
        public static readonly int NOTENAME_MAXLEN = 4;
        public static readonly string NOTENAME_SPACE = "    ";

        public static readonly int GT_MAXLEN = 4;
        public static readonly string GT_SPACE = "    ";

        public static readonly int Vel_MAXLEN = 3;
        public static readonly string Vel_SPACE = "   ";


        public MdlNoteEvent(EventManager parent) : base(parent)
        {
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;
            keyb.inputState = Keyboard.InputState.EditEvent;

            if (char.IsDigit(keyb.lastKeyChar))
            {
                MIDINoteEvent ne = (MIDINoteEvent)me;
                SetupNumericEdit(ne);
                return true;
            }

            if (IsNoteName(keyb.lastKeyChar))
            {
                me.editVal = NOTENAME_SPACE;
                me.virVal = ((MIDINoteEvent)me).NoteName;
                cursor.Position = 0;
                return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(MIDIEvent me, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            if (!me.editing) return true; // 編集モードでない場合は何もせずに編集終了

            if (keyChar == '\b' || keyChar == '/')
            {
                me.editVal = "";
                return false; // バックスペースまたはスラッシュが押された場合、編集を終了せずに編集値をクリア
            }

            switch (cursorInfo.Position)
            {
                // 0 NoteName
                case 0: UpdateActiveNoteEventNoteName((MIDINoteEvent)me, keyChar, keyPattern, cursorInfo); break;
                // 1 KeyNumber
                case 1: UpdateActiveNoteEventKeyNumber((MIDINoteEvent)me, keyChar, keyPattern, cursorInfo); break;
                // 2 ST            
                case 2: UpdateActiveNoteEventST((MIDINoteEvent)me, keyChar, keyPattern, cursorInfo); break;
                // 3 GT
                case 3: UpdateActiveNoteEventGT((MIDINoteEvent)me, keyChar, keyPattern, cursorInfo); break;
                // 4 Vel
                case 4: UpdateActiveNoteEventVel((MIDINoteEvent)me, keyChar, keyPattern, cursorInfo); break;
            }

            //編集完了したとき
            if (!me.editing)
            {
                //挿入モードならカーソルのPositionを0(NoteName)の位置に戻す
                if (!parent.parent.OverwriteMode) parent.parent.cursor.Position = 0;
            }

            return !me.editing;
        }



        //
        // UpdateActiveNoteEventNoteName
        //
        //  keyChar 入力された文字
        //  keyPatterm 確定系のキーが押された場合のその種類(keyCharは全て'\r'であること)
        //  cursorInfo 現在のカーソル情報
        //  newCursorInfo 確定系のキーが押された場合、座標とノードのみが設定されたものが返却される。その他の場合はnull
        //  
        //  尚、確定系のキーが押されていなくても、編集中の文字列が最大文字数に達した場合はEnterKeyを押したものとして処理される
        //
        private void UpdateActiveNoteEventNoteName(MIDINoteEvent ne, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            string ev;

            if (keyChar == '\r')
            {
                bool ok = false;
                ok = CheckNoteName(ne.editVal, out ev);
                if (!ok)
                {
                    ev = ne.virVal;
                    if (!CheckNoteName(ne.virVal, out ev))
                    {
                        WinApi.Beep();
                        ev = "C 4";
                        return;
                    }
                }

                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ev, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ev, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            string kc;
            if (keyChar != 0)
            {
                kc = keyChar.ToString().ToUpper();
                ev = (ne.editVal.Trim() + kc + NOTENAME_SPACE).Substring(0, NOTENAME_MAXLEN);
            }
            else
            {
                ev = (ne.editVal.Trim().Length > 0 ? ne.editVal.Trim()[..^1] : "") + NOTENAME_SPACE;
                ev = ev.Substring(0, NOTENAME_MAXLEN);
            }

            ne.editVal = ev;

            // ★ 最大文字数入力時の自動確定処理
            if (ne.editVal.Trim().Length == NOTENAME_MAXLEN)
            {

                CursorInfo newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ne.editVal.Trim(), false, MoveNextCursorAction.NextEvent);
                else MoveNext_NoteName2ST(ne, cursorInfo, newCursorInfo, ne.editVal.Trim(), true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_NoteName2ST(MIDINoteEvent ne, CursorInfo cursorInfo, CursorInfo newCursorInfo, string newNoteName, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(ne, cursorInfo, newCursorInfo, action, 2, ST_SPACE, ne.ST.ToString().PadLeft(ST_MAXLEN));

                if (!newEditing) commit(ne);

                // ★ Undo/Redo 用 old/new 値（更新前に oldState を作る）
                var oldState = new NoteKeyState(ne.KeyNumber, ne.NoteName);

                // 新しい KeyNumber を計算
                int newKeyNumber = MIDINoteEvent.NoteNameToKeyNumber(ref newNoteName);
                var newState = new NoteKeyState(newKeyNumber, newNoteName);

                // ★ 値を更新（ここで初めて更新する）
                ne.NoteName = newNoteName;
                ne.KeyNumber = newKeyNumber;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldState, newState, newEditing);

                parent.parent.RequestPlayNote(newKeyNumber);
            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }

        private bool CheckNoteName(string checkVal, out string noteName)
        {
            noteName = string.Empty;
            if (checkVal.Length < 2 || " +-#b0123456789".IndexOf(checkVal[1]) < 0)
            {
                return false;
            }

            noteName = checkVal;
            if (noteName.Trim() == "C--1")
            {
                return false;
            }
            else if ("0123456789".IndexOf(noteName[1]) >= 0)
            {
                noteName = noteName[0].ToString() + " " + noteName.Substring(1);
            }

            if ("ABCDEFG".IndexOf(noteName[0]) < 0)
            {
                return false;
            }
            if ("+-#b ".IndexOf(noteName[1]) < 0)
            {
                return false;
            }
            if ("0123456789".IndexOf(noteName[2]) < 0)
            {
                return false;
            }
            if (noteName.Length > 3 && " 0123456789".IndexOf(noteName[3]) < 0)
            {
                return false;
            }

            int n = MIDINoteEvent.NoteNameToKeyNumber(ref noteName);
            if (n < 0 || n > 127) return false;

            return true;
        }

        private bool IsNoteName(char keyChar)
        {
            return (keyChar >= 'a' && keyChar <= 'g')
                || (keyChar >= 'A' && keyChar <= 'G');
        }

        private bool SetupNumericEdit(MIDINoteEvent ne)
        {
            ne.editing = true;

            switch (parent.parent.cursor.Position)
            {
                case 0:
                case 1:
                    ne.editVal = NoteEvent.KEYNUMBER_SPACE;
                    ne.virVal = ne.KeyNumber.ToString().PadLeft(NoteEvent.KEYNUMBER_MAXLEN);
                    return true;

                case 2:
                    ne.editVal = NoteEvent.ST_SPACE;
                    ne.virVal = ne.ST.ToString().PadLeft(NoteEvent.ST_MAXLEN);
                    return true;

                case 3:
                    ne.editVal = NoteEvent.GT_SPACE;
                    ne.virVal = ne.GT.ToString().PadLeft(NoteEvent.GT_MAXLEN);
                    return true;

                case 4:
                    ne.editVal = NoteEvent.Vel_SPACE;
                    ne.virVal = ne.Vel.ToString().PadLeft(NoteEvent.Vel_MAXLEN);
                    return true;

                default:
                    ne.editing = false;
                    return false;
            }
        }


        private void UpdateActiveNoteEventKeyNumber(MIDINoteEvent ne, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!int.TryParse(ne.editVal, out ni))
                {
                    if (string.IsNullOrWhiteSpace(ne.editVal) && !int.TryParse(ne.virVal, out ni))
                    {
                        WinApi.Beep();
                        return;
                    }
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = KEYNUMBER_SPACE;
            if (keyChar != 0)
                n += ne.editVal + keyChar.ToString();
            else
                n += ne.editVal.Length > 0 ? ne.editVal[..^1] : "";
            n = n.SafeSubstring(n.Length - KEYNUMBER_MAXLEN, KEYNUMBER_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 127)) ne.editVal = n;
            else if (n.IsEmptyOrWhite()) ne.editVal = n;
            else WinApi.Beep();// 問題ある場合は受け入れずBeepを鳴らす

            // ★ 最大文字数入力時の自動確定処理
            if (ne.editVal.Trim().Length == KEYNUMBER_MAXLEN)
            {
                if (!int.TryParse(ne.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_KeyNumber2ST(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_KeyNumber2ST(MIDINoteEvent ne, CursorInfo cursorInfo, CursorInfo newCursorInfo, int newKeyNumber, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(ne, cursorInfo, newCursorInfo, action, 1, ST_SPACE, ne.ST.ToString().PadLeft(ST_MAXLEN));

                if (!newEditing) commit(ne);

                // ★ Undo/Redo 用 old/new 値（更新前に oldState を作る）
                var oldState = new NoteKeyState(ne.KeyNumber, ne.NoteName);

                // 新しい NoteName を計算
                string newNoteName = NoteEvent.KeyNumberToNoteName(newKeyNumber);
                var newState = new NoteKeyState(newKeyNumber, newNoteName);

                // ★ 値を更新（ここで初めて更新する）
                ne.KeyNumber = newKeyNumber;
                ne.NoteName = newNoteName;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldState, newState, newEditing);

                parent.parent.RequestPlayNote(newKeyNumber);
            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }



        private void UpdateActiveNoteEventST(MIDINoteEvent ne, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(ne.editVal, ne.virVal, out ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = ST_SPACE;
            if (keyChar != 0)
                n += ne.editVal + keyChar.ToString();
            else
                n += ne.editVal.Length > 0 ? ne.editVal[..^1] : "";

            n = n.SafeSubstring(n.Length - ST_MAXLEN, ST_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 9999)) ne.editVal = n;
            else if (n.IsEmptyOrWhite()) ne.editVal = n;
            else WinApi.Beep();// 問題ある場合は受け入れずBeepを鳴らす

            // ★ 最大文字数入力時の自動確定処理
            if (ne.editVal.Trim().Length == ST_MAXLEN)
            {
                if (!int.TryParse(ne.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2GT(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }

        }

        private void MoveNext_ST2GT(MIDINoteEvent ne, CursorInfo cursorInfo, CursorInfo newCursorInfo, int newST, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(ne, cursorInfo, newCursorInfo, action, 1, GT_SPACE, ne.GT.ToString().PadLeft(GT_MAXLEN));

                if (!newEditing) commit(ne);

                // Undo/Redo 用 old/new 値
                object oldValue = ne.ST;
                object newValue = newST;

                // 値を更新
                ne.ST = newST;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldValue, newValue, newEditing);

            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }



        private void UpdateActiveNoteEventGT(MIDINoteEvent ne, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(ne.editVal, ne.virVal, out ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = GT_SPACE;
            if (keyChar != 0)
                n += ne.editVal + keyChar.ToString();
            else
                n += ne.editVal.Length > 0 ? ne.editVal[..^1] : "";
            n = n.SafeSubstring(n.Length - GT_MAXLEN, GT_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 9999)) ne.editVal = n;
            else if (n.IsEmptyOrWhite()) ne.editVal = n;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (ne.editVal.Trim().Length == GT_MAXLEN)
            {
                if (!int.TryParse(ne.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_GT2Vel(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_GT2Vel(MIDINoteEvent ne, CursorInfo cursorInfo, CursorInfo newCursorInfo, int newGT, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(ne, cursorInfo, newCursorInfo, action, 1, Vel_SPACE, ne.Vel.ToString().PadLeft(Vel_MAXLEN));

                if (!newEditing) commit(ne);

                // Undo/Redo 用 old/new 値
                object oldValue = ne.GT;
                object newValue = newGT;

                // 値を更新
                ne.GT = newGT;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldValue, newValue, newEditing);

            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }



        private void UpdateActiveNoteEventVel(MIDINoteEvent ne, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(ne.editVal, ne.virVal, out ni) || !ni.IsBetween(0, 127))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                {
                    if (parent.parent.OverwriteMode) MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = Vel_SPACE;
            if (keyChar != 0)
                n += ne.editVal + keyChar.ToString();
            else
                n += ne.editVal.Length > 0 ? ne.editVal[..^1] : "";
            n = n.Substring(n.Length - Vel_MAXLEN, Vel_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 127)) ne.editVal = n;
            else if (n.IsEmptyOrWhite()) ne.editVal = n;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (ne.editVal.Trim().Length == Vel_MAXLEN)
            {
                if (!int.TryParse(ne.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_Vel(ne, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }

        }

        private void MoveNext_Vel(MIDINoteEvent ne, CursorInfo cursorInfo, CursorInfo newCursorInfo, int newVel, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(ne, cursorInfo, newCursorInfo, action, 0, "", "");

                if (!newEditing) commit(ne);

                // Undo/Redo 用 old/new 値
                object oldValue = ne.Vel;
                object newValue = newVel;

                // 値を更新
                ne.Vel = newVel;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldValue, newValue, newEditing);

            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }



        private void ApplyCursorMove(
            MIDINoteEvent ne,
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

            ApplyEditValAndVirVal(ne, action, nextEditVal, nextVirVal);
        }

        private void ApplyEditValAndVirVal(
            MIDINoteEvent ne,
            MoveNextCursorAction action,
            string nextEditVal,
            string nextVirVal)
        {
            switch (action)
            {
                case MoveNextCursorAction.NextEvent:
                case MoveNextCursorAction.PreviousEvent:
                    ne.editVal = "";
                    ne.virVal = "";
                    break;

                case MoveNextCursorAction.NextField:
                    ne.editVal = nextEditVal;
                    ne.virVal = nextVirVal;
                    break;

                case MoveNextCursorAction.StayOnField:
                    // 何もしない
                    break;
            }
        }


        private void ApplyUndoRedo(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            object oldValue,
            object newValue,
            bool newEditing)
        {
            var cmd = new EditNoteEventCommand(
                parent,
                parent.parent,
                cursorInfo.Index,
                cursorInfo.Position,
                oldValue,
                newValue,
                cursorInfo.Index,
                cursorInfo.Position,
                newCursorInfo.Index,
                newCursorInfo.Position,
                true,
                newEditing
            );

            ExecuteUndoRedoCommand(cmd);
        }

        private void commit(MIDINoteEvent ne)
        {
            ne.editing = false;

            MIDINoteEvent.oNoteName = ne.NoteName;
            MIDINoteEvent.oKeyNumber = ne.KeyNumber;
            MIDINoteEvent.oST = ne.ST;
            MIDINoteEvent.oGT = ne.GT;
            MIDINoteEvent.oVel = ne.Vel;
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 0:
                        bool ok = false;
                        string nn = string.Empty;
                        ok = CheckNoteName(me.editVal.Trim(), out nn);
                        if (!ok)
                        {
                            nn = me.virVal;
                            ok = CheckNoteName(me.virVal, out nn);
                            if (!ok)
                            {
                                WinApi.Beep();
                                nn = "C 4"; // デフォルトのノート名
                            }
                        }
                        MoveNext_NoteName2ST((MIDINoteEvent)me, cursorInfo, newCursorInfo, nn, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 1:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 60;
                            }
                        MoveNext_KeyNumber2ST((MIDINoteEvent)me, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 2:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_ST2GT((MIDINoteEvent)me, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 3:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_GT2Vel((MIDINoteEvent)me, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 4:
                        if (!int.TryParse(me.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(me.editVal) && !int.TryParse(me.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 110;
                            }
                        MoveNext_Vel((MIDINoteEvent)me, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch (cursorInfo.Position)
            {
                case 0:
                    UpdateActiveNoteEventNoteName((MIDINoteEvent)me, '\r', keyPattern, cursorInfo);
                    break;
                case 1:
                    UpdateActiveNoteEventKeyNumber((MIDINoteEvent)me, '\r', keyPattern, cursorInfo);
                    break;
                case 2:
                    UpdateActiveNoteEventST((MIDINoteEvent)me, '\r', keyPattern, cursorInfo);
                    break;
                case 3:
                    UpdateActiveNoteEventGT((MIDINoteEvent)me, '\r', keyPattern, cursorInfo);
                    break;
                case 4:
                    UpdateActiveNoteEventVel((MIDINoteEvent)me, '\r', keyPattern, cursorInfo);
                    break;
            }
        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                0 => ((MIDINoteEvent)me).NoteName,
                1 => ((MIDINoteEvent)me).KeyNumber.ToString(),
                2 => ((MIDINoteEvent)me).ST.ToString(),
                3 => ((MIDINoteEvent)me).GT.ToString(),
                4 => ((MIDINoteEvent)me).Vel.ToString(),
                _ => "?",
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDINoteEvent ne = (MIDINoteEvent)me;
            return $"{ne.Meas + 1},{ne.Step + 1}: NoteEvent: {ne.NoteName},{ne.KeyNumber},{ne.ST},{ne.GT},{ne.Vel}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 5) return null;

            return new MIDINoteEvent()
            {
                Meas = meas,
                Step = step,
                NoteName = vals[0].Trim(),
                KeyNumber = int.Parse(vals[1]),
                ST = int.Parse(vals[2]),
                GT = int.Parse(vals[3]),
                Vel = int.Parse(vals[4])
            };
        }

        public override void CopyFrom(MIDIEvent srcMe, MIDIEvent otherMe, MdlEvent otherMdle)
        {
            base.CopyFrom(srcMe, otherMe, otherMdle);

            var n = (MIDINoteEvent)otherMe;
            n.CopyFrom(otherMe);

            // NoteEvent 固有フィールド
            ((MIDINoteEvent)srcMe).KeyNumber = n.KeyNumber;
            ((MIDINoteEvent)srcMe).NoteName = n.NoteName;
            ((MIDINoteEvent)srcMe).GT = n.GT;
            ((MIDINoteEvent)srcMe).Vel = n.Vel;
            ((MIDINoteEvent)srcMe).Tie = n.Tie;

        }

    }
}