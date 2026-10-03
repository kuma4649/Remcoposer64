using Remcoposer64.StepEditorPanelControl.Command;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class PolyphonicKeyPressureEvent : Event
    {
        public override EventType Type => EventType.PolyphonicKeyPressure;
        
        public const int KEY_MAXLEN = 3;
        public const string KEY_SPACE = "   ";

        public const int PRESS_MAXLEN = 3;
        public const string PRESS_SPACE = "   ";

        public static readonly int KeyMin = 0;
        public static readonly int KeyMax = 127;

        public static readonly int PressureMin = 0;
        public static readonly int PressureMax = 127;
        
        public int KeyNumber { get; set; } = 60;
        public int PressureValue { get; set; } = 64;

        private int backupST;
        private int backupKeyNumber;
        private int backupPressure;

        private static int oKeyNumber;
        private static int oPressure;


        public PolyphonicKeyPressureEvent(EventManager parent) : base(parent)
        {
            ST = oST;
            KeyNumber = oKeyNumber;
            PressureValue = oPressure;
        }

        public override Event Clone()
        {
            return new PolyphonicKeyPressureEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                KeyNumber = this.KeyNumber,
                PressureValue = this.PressureValue,

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal
            };
        }
        
        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3) return 2;   // ST
            if (rawPos < 4) return 3;   // KeyNumber
            return 4;                   // Pressure
        }

        public override bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            switch (cursor.Position)
            {
                case 2: // ST
                    editVal = ST_SPACE;
                    virVal = ST.ToString().PadLeft(ST_MAXLEN);
                    backupST = ST;
                    return true;
                case 3: // KeyNumber
                    editVal = KEY_SPACE;
                    virVal = KeyNumber.ToString().PadLeft(KEY_MAXLEN);
                    backupKeyNumber = KeyNumber;
                    return true;
                case 4: // PressureValue
                    editVal = PRESS_SPACE;
                    virVal = PressureValue.ToString().PadLeft(PRESS_MAXLEN);
                    backupPressure = PressureValue;
                    return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            if (!editing) return true;

            if (keyChar == '\b' && editVal.Length > 0)
            {
                editVal = "";
                return false;
            }

            switch (cursor.Position)
            {
                case 2: UpdateActivePK_ST(keyChar, keyPattern, cursor); break;
                case 3: UpdateActivePK_KeyNumber(keyChar, keyPattern, cursor); break;
                case 4: UpdateActivePK_Pressure(keyChar, keyPattern, cursor); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !editing;
        }

        private void UpdateActivePK_ST(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;

            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2KeyNumber(cursor, newCursorInfo, newST, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2KeyNumber(cursor, newCursorInfo, newST, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2KeyNumber(cursor, newCursorInfo, newST, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2KeyNumber(cursor, newCursorInfo, newST, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2KeyNumber(cursor, newCursorInfo, newST, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2KeyNumber(cursor, newCursorInfo, newST, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0)
            {
                s = ST_SPACE + editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            if (editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ev = int.Parse(editVal);

                if (parent.parent.OverwriteMode)
                    MoveNext_ST2KeyNumber(cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2KeyNumber(cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2KeyNumber(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newST,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,
                    KEY_SPACE,
                    KeyNumber.ToString()
                );

                int oldST = backupST;
                ST = newST;

                if (!newEditing) commit();

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

        private void UpdateActivePK_KeyNumber(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni) || !ni.IsBetween(KeyMin, KeyMax))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(KEY_MAXLEN);
                s = s.Substring(s.Length - KEY_MAXLEN, KEY_MAXLEN);
            }
            else if (keyChar == 0)
            {
                s = KEY_SPACE + editVal;
                s = s.Substring(s.Length - KEY_MAXLEN - 1, KEY_MAXLEN);
            }

            if (int.TryParse(s, out ni) && ni.IsBetween(KeyMin, KeyMax))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            if (editVal.Trim().Length == KEY_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                MoveNext_KeyNumber2Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_KeyNumber2Pressure(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newKey,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,
                    PRESS_SPACE,
                    PressureValue.ToString()
                );

                int oldKey = backupKeyNumber;
                KeyNumber = newKey;

                parent.parent.RequestPlayNote(newKey);

                if (!newEditing) commit();

                ApplyUndoRedo(
                    cursorInfo,
                    oldKey,
                    true,
                    newCursorInfo,
                    newKey,
                    newEditing
                );
            }
            catch
            {
                WinApi.Beep();
                return;
            }
        }

        private void UpdateActivePK_Pressure(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(editVal, virVal, out ni) || !ni.IsBetween(PressureMin, PressureMax))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_Pressure(cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (editVal.Trim() + keyChar).PadLeft(PRESS_MAXLEN);
                s = s.Substring(s.Length - PRESS_MAXLEN, PRESS_MAXLEN);
            }
            else if (keyChar == 0)
            {
                s = PRESS_SPACE + editVal;
                s = s.Substring(s.Length - PRESS_MAXLEN - 1, PRESS_MAXLEN);
            }

            if (int.TryParse(s, out ni) && ni.IsBetween(PressureMin, PressureMax))
                editVal = s;
            else if (s.IsEmptyOrWhite())
                editVal = s;
            else
                WinApi.Beep();

            if (editVal.Trim().Length == PRESS_MAXLEN)
            {
                if (!int.TryParse(editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode)
                    MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_Pressure(cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_Pressure(
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newPressure,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(cursorInfo, newCursorInfo, action, 0, "", "");

                int oldPressure = backupPressure;
                PressureValue = newPressure;

                if (!newEditing) commit();

                ApplyUndoRedo(
                    cursorInfo,
                    oldPressure,
                    true,
                    newCursorInfo,
                    newPressure,
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
            var cmd = new EditPolyphonicKeyPressureCommand(
                parent,
                parent.parent,
                oldCursor.Index,
                // --- old block ---
                oldVal,
                oldCursor.Index,
                oldCursor.Position,
                oldEditing,
                // --- new block ---
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
            oKeyNumber = KeyNumber;
            oPressure = PressureValue;
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
                        MoveNext_ST2KeyNumber(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 3:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 60;
                            }
                        MoveNext_KeyNumber2Pressure(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
                        break;

                    case 4:
                        if (!int.TryParse(editVal, out ni))
                            if (string.IsNullOrWhiteSpace(editVal) && !int.TryParse(virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 64;
                            }
                        MoveNext_Pressure(cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
            }

            // Enter / Arrow keys の場合は UpdateActiveXXX('\r') を呼ぶ
            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActivePK_ST('\r', keyPattern, cursorInfo);
                    break;

                case 3:
                    UpdateActivePK_KeyNumber('\r', keyPattern, cursorInfo);
                    break;

                case 4:
                    UpdateActivePK_Pressure('\r', keyPattern, cursorInfo);
                    break;
            }

        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ST.ToString(),
                3 => KeyNumber.ToString(),
                4 => PressureValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString()
        {
            return $"{Meas + 1},{Step + 1}: PolyphonicKeyPressure: {ST}, {KeyNumber}, {PressureValue}";
        }

        public static Event FromClipboardBody(EventManager parent,int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 3) return null;

            if (!int.TryParse(vals[0], out int st)) return null;
            if (!int.TryParse(vals[1], out int key)) return null;
            if (!int.TryParse(vals[2], out int pressure)) return null;

            return new PolyphonicKeyPressureEvent(parent)
            {
                Meas = meas,
                Step = step,
                ST = st,
                KeyNumber = key,
                PressureValue = pressure
            };
        }

    }
}
