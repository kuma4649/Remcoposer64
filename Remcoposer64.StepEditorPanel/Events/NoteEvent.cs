using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Command;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class NoteEvent : Event
    {

        public override EventType Type => EventType.Note;

        private int _keyNumber;
        private string _noteName = "C 4";

        public int KeyNumber
        {
            get => _keyNumber;
            set
            {
                _keyNumber = Math.Clamp(value, 0, 127);
                _noteName = KeyNumberToNoteName(_keyNumber);
            }
        }
        public static readonly int KEYNUMBER_MAXLEN = 3;
        public static readonly string KEYNUMBER_SPACE = "   ";
        public static int oKeyNumber = -1;
        public string NoteName
        {
            get => _noteName;
            set
            {
                _noteName = value;
                _keyNumber = NoteNameToKeyNumber(ref _noteName);
            }
        }
        public static readonly int NOTENAME_MAXLEN = 4;
        public static readonly string NOTENAME_SPACE = "    ";
        public static string oNoteName = "C 4";

        public int GT { get; set; }
        public static int oGT { get; set; }
        public static readonly int GT_MAXLEN = 4;
        public static readonly string GT_SPACE = "    ";

        public int Vel { get; set; }
        public static int oVel { get; set; }
        public static readonly int Vel_MAXLEN = 3;
        public static readonly string Vel_SPACE = "   ";

        public bool Tie { get; set; } = false;

        // 12音階
        private static readonly string[] NoteNames =
        {
        "C ", "C#", "D ", "D#", "E ", "F ",
        "F#", "G ", "G#", "A ", "A#", "B "
        };

        public NoteEvent(EventManager parent) : base(parent)
        {
            NoteName = oNoteName;
            KeyNumber = oKeyNumber;
            ST = oST;
            GT = oGT;
            Vel = oVel;
        }

        public override Event Clone()
        {
            return new NoteEvent(parent)
            {
                // Event 基底クラスのフィールド
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal,

                // NoteEvent 固有フィールド
                KeyNumber = this.KeyNumber,
                NoteName = this.NoteName,
                GT = this.GT,
                Vel = this.Vel,
                Tie = this.Tie
            };
        }

        public static string KeyNumberToNoteName(int key)
        {
            int note = key % 12;
            int octave = (key / 12) - 1;
            return $"{NoteNames[note]}{octave}";
        }

        public static int NoteNameToKeyNumber(ref string name)
        {
            try
            {
                string s = name.ToUpper();
                if (s[1] == '+') s = s.Substring(0, 1) + '#' + s.Substring(2);
                if (s[1] == '-') s = s.Substring(0, 1) + 'b' + s.Substring(2);

                if (int.TryParse(s.Substring(2), out int octave) == false)
                {
                    octave = 4;// デフォルトは 4 オクターブ
                    s = s.Substring(0, 2) + octave.ToString();
                    name = s;
                }
                string notePart = s.Substring(0, 2);

                int noteIndex = ParseNoteName(notePart);
                if (noteIndex < -1)
                    throw new ArgumentException($"Invalid note name: {name}");

                return (octave + 1) * 12 + noteIndex;
            }
            catch
            {
                return -1;
            }
        }

        private static int ParseNoteName(string note)
        {
            // 特殊ケース：B# → 12
            if (note == "B#") return 12;
            // 特殊ケース：Cb → -1
            if (note == "Cb") return -1;
            // 特殊ケース：E# → F
            if (note == "E#") return Array.IndexOf(NoteNames, "F ");
            // 特殊ケース：Fb → E
            if (note == "Fb") return Array.IndexOf(NoteNames, "E ");

            // 通常の NoteNames から検索
            int idx = Array.IndexOf(NoteNames, note);
            if (idx >= 0) return idx;

            // フラット（b）対応：Db, Eb, Gb, Ab, Bb
            // 大文字化して "b" を "B" に統一
            string upper = note.ToUpper();

            if (upper.EndsWith("B"))
            {
                string natural = upper[..^1] + " ";   // "D" → "D "
                int naturalIndex = Array.IndexOf(NoteNames, natural);

                if (naturalIndex >= 0)
                    return (naturalIndex + 11) % 12;  // 半音下げる
            }

            // ここまで来たら不正
            return -2;
        }

        public override bool UpdateActiveEvent(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            if (!editing) return true; // 編集モードでない場合は何もせずに編集終了

            if (keyChar == '\b' || keyChar == '/')
            {
                editVal = "";
                return false; // バックスペースまたはスラッシュが押された場合、編集を終了せずに編集値をクリア
            }

            switch (cursorInfo.Position)
            {
                // 0 NoteName
                case 0: UpdateActiveNoteEventNoteName(keyChar, keyPattern, cursorInfo); break;
                // 1 KeyNumber
                case 1: UpdateActiveNoteEventKeyNumber(keyChar, keyPattern, cursorInfo); break;
                // 2 ST            
                case 2: UpdateActiveNoteEventST(keyChar, keyPattern, cursorInfo); break;
                // 3 GT
                case 3: UpdateActiveNoteEventGT(keyChar, keyPattern, cursorInfo); break;
                // 4 Vel
                case 4: UpdateActiveNoteEventVel(keyChar, keyPattern, cursorInfo); break;
            }

            //編集完了したとき
            if (!editing)
            {
                //挿入モードならカーソルのPositionを0(NoteName)の位置に戻す
                if (!parent.parent.OverwriteMode) parent.parent.cursor.Position = 0;
            }

            return !editing;
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
        private void UpdateActiveNoteEventNoteName(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            string ev;

            if (keyChar == '\r')
            {
                bool ok = false;
                ok = CheckNoteName(editVal, out ev);
                if(!ok)
                {
                    ev = virVal;
                    if (!CheckNoteName(virVal, out ev))
                    {
                        WinApi.Beep();
                        ev = "C 4";
                        return;
                    }
                }

                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_NoteName2ST(cursorInfo, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_NoteName2ST(cursorInfo, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_NoteName2ST(cursorInfo, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_NoteName2ST(cursorInfo, newCursorInfo, ev, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_NoteName2ST(cursorInfo, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_NoteName2ST(cursorInfo, newCursorInfo, ev, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            string kc;
            if (keyChar != 0)
            {
                kc = keyChar.ToString().ToUpper();
                ev = (editVal.Trim() + kc + NOTENAME_SPACE).Substring(0, NOTENAME_MAXLEN);
            }
            else
            {
                ev = (editVal.Trim().Length > 0 ? editVal.Trim()[..^1] : "") + NOTENAME_SPACE;
                ev = ev.Substring(0, NOTENAME_MAXLEN);
            }

            editVal = ev;

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == NOTENAME_MAXLEN)
            {

                CursorInfo newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_NoteName2ST(cursorInfo, newCursorInfo, editVal.Trim(), false, MoveNextCursorAction.NextEvent);
                else MoveNext_NoteName2ST(cursorInfo, newCursorInfo, editVal.Trim(), true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_NoteName2ST(CursorInfo cursorInfo, CursorInfo newCursorInfo, string newNoteName, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 2, ST_SPACE, ST.ToString().PadLeft(ST_MAXLEN));

                if (!newEditing) commit();

                // ★ Undo/Redo 用 old/new 値（更新前に oldState を作る）
                var oldState = new NoteKeyState(KeyNumber, NoteName);

                // 新しい KeyNumber を計算
                int newKeyNumber = NoteNameToKeyNumber(ref newNoteName);
                var newState = new NoteKeyState(newKeyNumber, newNoteName);

                // ★ 値を更新（ここで初めて更新する）
                NoteName = newNoteName;
                KeyNumber = newKeyNumber;

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

            int n = NoteNameToKeyNumber(ref noteName);
            if (n < 0 || n > 127) return false;

            return true;
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            editing = true;
            keyb.inputState = Keyboard.InputState.EditEvent;

            if (char.IsDigit(keyb.lastKeyChar))
            {
                SetupNumericEdit();
                return true;
            }

            if (IsNoteName(keyb.lastKeyChar))
            {
                editVal = NOTENAME_SPACE;
                virVal = NoteName;
                cursor.Position = 0;
                return true;
            }

            return false;
        }

        private bool IsNoteName(char keyChar)
        {
            return (keyChar >= 'a' && keyChar <= 'g')
                || (keyChar >= 'A' && keyChar <= 'G');
        }

        private bool SetupNumericEdit()
        {
            editing = true;

            switch (parent.parent.cursor.Position)
            {
                case 0:
                case 1:
                    editVal = NoteEvent.KEYNUMBER_SPACE;
                    virVal = KeyNumber.ToString().PadLeft(NoteEvent.KEYNUMBER_MAXLEN);
                    return true;

                case 2:
                    editVal = NoteEvent.ST_SPACE;
                    virVal = ST.ToString().PadLeft(NoteEvent.ST_MAXLEN);
                    return true;

                case 3:
                    editVal = NoteEvent.GT_SPACE;
                    virVal = GT.ToString().PadLeft(NoteEvent.GT_MAXLEN);
                    return true;

                case 4:
                    editVal = NoteEvent.Vel_SPACE;
                    virVal = Vel.ToString().PadLeft(NoteEvent.Vel_MAXLEN);
                    return true;

                default:
                    editing = false;
                    return false;
            }
        }


        private void UpdateActiveNoteEventKeyNumber(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!int.TryParse(editVal, out ni))
                {
                    if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                    {
                        WinApi.Beep();
                        return;
                    }
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = KEYNUMBER_SPACE;
            if (keyChar != 0)
                n += editVal + keyChar.ToString();
            else
                n += editVal.Length > 0 ? editVal[..^1] : "";
            n = n.SafeSubstring(n.Length - KEYNUMBER_MAXLEN, KEYNUMBER_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 127)) editVal = n;
            else if (n.IsEmptyOrWhite()) editVal = n;
            else WinApi.Beep();// 問題ある場合は受け入れずBeepを鳴らす

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == KEYNUMBER_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_KeyNumber2ST(CursorInfo cursorInfo, CursorInfo newCursorInfo, int newKeyNumber,bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 1, ST_SPACE, ST.ToString().PadLeft(ST_MAXLEN));

                if (!newEditing) commit();

                // ★ Undo/Redo 用 old/new 値（更新前に oldState を作る）
                var oldState = new NoteKeyState(KeyNumber, NoteName);

                // 新しい NoteName を計算
                string newNoteName = NoteEvent.KeyNumberToNoteName(newKeyNumber);
                var newState = new NoteKeyState(newKeyNumber, newNoteName);

                // ★ 値を更新（ここで初めて更新する）
                KeyNumber = newKeyNumber;
                NoteName = newNoteName;

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



        private void UpdateActiveNoteEventST(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = ST_SPACE;
            if (keyChar != 0)
                n += editVal + keyChar.ToString();
            else
                n += editVal.Length > 0 ? editVal[..^1] : "";

            n = n.SafeSubstring(n.Length - ST_MAXLEN, ST_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 9999)) editVal = n;
            else if (n.IsEmptyOrWhite()) editVal = n;
            else WinApi.Beep();// 問題ある場合は受け入れずBeepを鳴らす

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == ST_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }

        }

        private void MoveNext_ST2GT(CursorInfo cursorInfo, CursorInfo newCursorInfo, int newST, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 1, GT_SPACE, GT.ToString().PadLeft(GT_MAXLEN));

                if (!newEditing) commit();

                // Undo/Redo 用 old/new 値
                object oldValue = ST;
                object newValue = newST;

                // 値を更新
                ST = newST;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldValue, newValue, newEditing);

            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }



        private void UpdateActiveNoteEventGT(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            // Enter押下
            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)//Enter
                {
                    if (parent.parent.OverwriteMode) MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                    MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = GT_SPACE;
            if (keyChar != 0)
                n += editVal + keyChar.ToString();
            else
                n += editVal.Length > 0 ? editVal[..^1] : "";
            n = n.SafeSubstring(n.Length - GT_MAXLEN, GT_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 9999)) editVal = n;
            else if (n.IsEmptyOrWhite()) editVal = n;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == GT_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_GT2Vel(CursorInfo cursorInfo, CursorInfo newCursorInfo, int newGT, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 1, Vel_SPACE, Vel.ToString().PadLeft(Vel_MAXLEN));

                if (!newEditing) commit();

                // Undo/Redo 用 old/new 値
                object oldValue = GT;
                object newValue = newGT;

                // 値を更新
                GT = newGT;

                // コマンド発行
                ApplyUndoRedo(cursorInfo, newCursorInfo, oldValue, newValue, newEditing);

            }
            catch (Exception)
            {
                WinApi.Beep();
                return;
            }
        }



        private void UpdateActiveNoteEventVel(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursorInfo)
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
                    if (parent.parent.OverwriteMode) MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)//Right
                {
                    if (parent.parent.OverwriteMode) MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)//Left
                    MoveNext_Vel(cursorInfo, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)//Down
                    MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)//Up
                    MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);
                return;
            }

            // 入力値を一旦受け入れる
            string n = Vel_SPACE;
            if (keyChar != 0)
                n += editVal + keyChar.ToString();
            else
                n += editVal.Length > 0 ? editVal[..^1] : "";
            n = n.Substring(n.Length - Vel_MAXLEN, Vel_MAXLEN);

            // 数値として問題無いか、空白かを判定して正式に受け入れる
            if (int.TryParse(n, out ni) && ni.IsBetween(0, 127)) editVal = n;
            else if (n.IsEmptyOrWhite()) editVal = n;
            else WinApi.Beep();

            // ★ 最大文字数入力時の自動確定処理
            if (editVal.Trim().Length == Vel_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode) MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }

        }

        private void MoveNext_Vel(CursorInfo cursorInfo, CursorInfo newCursorInfo, int newVel, bool newEditing, MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 0, "", "");

                if (!newEditing) commit();

                // Undo/Redo 用 old/new 値
                object oldValue = Vel;
                object newValue = newVel;

                // 値を更新
                Vel = newVel;

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

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!editing) return;

            if (keyPattern == CommitKeyPattern.OtherKey)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ni;

                switch (cursorInfo.Position)
                {
                    case 0:
                        bool ok = false;
                        string nn = string.Empty;
                        ok = CheckNoteName(editVal.Trim(), out nn);
                        if (!ok)
                        {
                            nn = virVal;
                            ok = CheckNoteName(virVal, out nn);
                            if (!ok)
                            {
                                WinApi.Beep();
                                nn = "C 4"; // デフォルトのノート名
                            }
                        }
                        MoveNext_NoteName2ST(cursorInfo, newCursorInfo, nn, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 1:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 60;
                            }
                        MoveNext_KeyNumber2ST(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 2:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_ST2GT(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 3:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 120;
                            }
                        MoveNext_GT2Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                    case 4:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 110;
                            }
                        MoveNext_Vel(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
                return;
            }

            switch(cursorInfo.Position)
            {
                case 0:
                    UpdateActiveNoteEventNoteName('\r', keyPattern, cursorInfo);
                    break;
                case 1:
                    UpdateActiveNoteEventKeyNumber('\r', keyPattern, cursorInfo);
                    break;
                case 2:
                    UpdateActiveNoteEventST('\r', keyPattern, cursorInfo);
                    break;
                case 3:
                    UpdateActiveNoteEventGT('\r', keyPattern, cursorInfo);
                    break;
                case 4:
                    UpdateActiveNoteEventVel('\r', keyPattern, cursorInfo);
                    break;
            }
        }

        private void commit()
        {
            editing = false;

            oNoteName = NoteName;
            oKeyNumber = KeyNumber;
            oST = ST;
            oGT = GT;
            oVel = Vel;
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                0 => NoteName,
                1 => KeyNumber.ToString(),
                2 => ST.ToString(),
                3 => GT.ToString(),
                4 => Vel.ToString(),
                _ => "?",
            };
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: NoteEvent: {NoteName},{KeyNumber},{ST},{GT},{Vel}";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 5) return null;

            return new NoteEvent(parent)
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

        public override void CopyFrom(Event other)
        {
            base.CopyFrom(other);

            var n = (NoteEvent)other;

            // NoteEvent 固有フィールド
            this.KeyNumber = n.KeyNumber;
            this.NoteName = n.NoteName;
            this.GT = n.GT;
            this.Vel = n.Vel;
            this.Tie = n.Tie;

            // 編集状態系
            this.editing = n.editing;
            this.editVal = n.editVal;
            this.virVal = n.virVal;

            // STsum は Event 基底クラス側のフィールドなので base.CopyFrom ではコピーされない
            this.STsum = n.STsum;
        }

        public override void EndEdit(Keyboard keyb)
        {
            editing = false;
            keyb.inputState = Keyboard.InputState.None;
        }


    }
}
