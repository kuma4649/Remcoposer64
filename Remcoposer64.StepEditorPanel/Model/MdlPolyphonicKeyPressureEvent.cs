using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlPolyphonicKeyPressureEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.KeyAfterTouch;

        public const int KEY_MAXLEN = 3;
        public const string KEY_SPACE = "   ";

        public const int PRESS_MAXLEN = 3;
        public const string PRESS_SPACE = "   ";

        public MdlPolyphonicKeyPressureEvent(EventManager parent) : base(parent)
        {
        }

        public override int AdjustCursorPosition(int rawPos)
        {
            if (rawPos < 3) return 2;   // ST
            if (rawPos < 4) return 3;   // KeyNumber
            return 4;                   // Pressure
        }

        public override bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            me.editing = true;

            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = AdjustCursorPosition(cursor.Position);
            if (!parent.parent.OverwriteMode) cursor.Position = 2;

            MIDIPolyphonicKeyPressureEvent pe = (MIDIPolyphonicKeyPressureEvent)me;
            switch (cursor.Position)
            {
                case 2: // ST
                    me.editVal = ST_SPACE;
                    me.virVal = pe.ST.ToString().PadLeft(ST_MAXLEN);
                    pe.backupST = pe.ST;
                    return true;
                case 3: // KeyNumber
                    me.editVal = KEY_SPACE;
                    me.virVal = pe.KeyNumber.ToString().PadLeft(KEY_MAXLEN);
                    pe.backupKeyNumber = pe.KeyNumber;
                    return true;
                case 4: // PressureValue
                    me.editVal = PRESS_SPACE;
                    me.virVal = pe.PressureValue.ToString().PadLeft(PRESS_MAXLEN);
                    pe.backupPressure = pe.PressureValue;
                    return true;
            }

            return false;
        }

        public override bool UpdateActiveEvent(MIDIEvent me, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            if (!me.editing) return true;

            if (keyChar == '\b' && me.editVal.Length > 0)
            {
                me.editVal = "";
                return false;
            }

            switch (cursor.Position)
            {
                case 2: UpdateActivePK_ST((MIDIPolyphonicKeyPressureEvent)me, keyChar, keyPattern, cursor); break;
                case 3: UpdateActivePK_KeyNumber((MIDIPolyphonicKeyPressureEvent)me, keyChar, keyPattern, cursor); break;
                case 4: UpdateActivePK_Pressure((MIDIPolyphonicKeyPressureEvent)me, keyChar, keyPattern, cursor); break;
            }

            //編集完了したとき挿入モードならカーソルのPositionを0(NoteName)の位置に戻す(本イベントのホームポジションであるSTには戻さない。次のイベント向けに0で初期化する。)
            if (!me.editing)
                if (!parent.parent.OverwriteMode)
                    parent.parent.cursor.Position = 0;

            return !me.editing;
        }

        private void UpdateActivePK_ST(MIDIPolyphonicKeyPressureEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;

            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(pe.editVal, pe.virVal, out int newST))
                {
                    WinApi.Beep();
                    return;
                }

                CursorInfo newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, newST, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, newST, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, newST, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, newST, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, newST, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, newST, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(ST_MAXLEN);
                s = s.Substring(s.Length - ST_MAXLEN, ST_MAXLEN);
            }
            else if (keyChar == 0)
            {
                s = ST_SPACE + pe.editVal;
                s = s.Substring(s.Length - ST_MAXLEN - 1, ST_MAXLEN);
            }

            if (int.TryParse(s, out ni) && ni.IsBetween(ST_MINVALUE, ST_MAXVALUE))
                pe.editVal = s;
            else if (s.IsEmptyOrWhite())
                pe.editVal = s;
            else
                WinApi.Beep();

            if (pe.editVal.Trim().Length == ST_MAXLEN)
            {
                CursorInfo newCursorInfo = new CursorInfo();
                int ev = int.Parse(pe.editVal);

                if (parent.parent.OverwriteMode)
                    MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, ev, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_ST2KeyNumber(pe, cursor, newCursorInfo, ev, true, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_ST2KeyNumber(
            MIDIPolyphonicKeyPressureEvent pe,
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newST,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(
                    pe,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,
                    KEY_SPACE,
                    pe.KeyNumber.ToString()
                );

                int oldST = pe.backupST;
                pe.ST = newST;

                if (!newEditing) commit(pe);

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

        private void UpdateActivePK_KeyNumber(MIDIPolyphonicKeyPressureEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(pe.editVal, pe.virVal, out ni) || !ni.IsBetween(MIDIPolyphonicKeyPressureEvent.KeyMin, MIDIPolyphonicKeyPressureEvent.KeyMax))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                    MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.NextField);
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(KEY_MAXLEN);
                s = s.Substring(s.Length - KEY_MAXLEN, KEY_MAXLEN);
            }
            else if (keyChar == 0)
            {
                s = KEY_SPACE + pe.editVal;
                s = s.Substring(s.Length - KEY_MAXLEN - 1, KEY_MAXLEN);
            }

            if (int.TryParse(s, out ni) && ni.IsBetween(MIDIPolyphonicKeyPressureEvent.KeyMin, MIDIPolyphonicKeyPressureEvent.KeyMax))
                pe.editVal = s;
            else if (s.IsEmptyOrWhite())
                pe.editVal = s;
            else
                WinApi.Beep();

            if (pe.editVal.Trim().Length == KEY_MAXLEN)
            {
                if (!int.TryParse(pe.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                MoveNext_KeyNumber2Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_KeyNumber2Pressure(
            MIDIPolyphonicKeyPressureEvent pe,
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newKey,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(
                    pe,
                    cursorInfo,
                    newCursorInfo,
                    action,
                    1,
                    PRESS_SPACE,
                    pe.PressureValue.ToString()
                );

                int oldKey = pe.backupKeyNumber;
                pe.KeyNumber = newKey;

                parent.parent.RequestPlayNote(newKey);

                if (!newEditing) commit(pe);

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

        private void UpdateActivePK_Pressure(MIDIPolyphonicKeyPressureEvent pe, char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            int ni;
            CursorInfo newCursorInfo = null;

            if (keyChar == '\r')
            {
                if (!Common.TryParseEither(pe.editVal, pe.virVal, out ni) || !ni.IsBetween(MIDIPolyphonicKeyPressureEvent.PressureMin, MIDIPolyphonicKeyPressureEvent.PressureMax))
                {
                    WinApi.Beep();
                    return;
                }

                newCursorInfo = new CursorInfo();

                if (keyPattern == CommitKeyPattern.EnterKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.RightKey)
                {
                    if (parent.parent.OverwriteMode)
                        MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                    else
                        MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                }
                else if (keyPattern == CommitKeyPattern.LeftKey)
                    MoveNext_Pressure(pe, cursor, newCursorInfo, ni, true, MoveNextCursorAction.StayOnField);
                else if (keyPattern == CommitKeyPattern.DownKey)
                    MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else if (keyPattern == CommitKeyPattern.UpKey)
                    MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.PreviousEvent);

                return;
            }

            string s = "";
            if (char.IsDigit(keyChar))
            {
                s = (pe.editVal.Trim() + keyChar).PadLeft(PRESS_MAXLEN);
                s = s.Substring(s.Length - PRESS_MAXLEN, PRESS_MAXLEN);
            }
            else if (keyChar == 0)
            {
                s = PRESS_SPACE + pe.editVal;
                s = s.Substring(s.Length - PRESS_MAXLEN - 1, PRESS_MAXLEN);
            }

            if (int.TryParse(s, out ni) && ni.IsBetween(MIDIPolyphonicKeyPressureEvent.PressureMin, MIDIPolyphonicKeyPressureEvent.PressureMax))
                pe.editVal = s;
            else if (s.IsEmptyOrWhite())
                pe.editVal = s;
            else
                WinApi.Beep();

            if (pe.editVal.Trim().Length == PRESS_MAXLEN)
            {
                if (!int.TryParse(pe.editVal, out ni)) return;

                newCursorInfo = new CursorInfo();
                if (parent.parent.OverwriteMode)
                    MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                else
                    MoveNext_Pressure(pe, cursor, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
            }
        }

        private void MoveNext_Pressure(
            MIDIPolyphonicKeyPressureEvent pe,
            CursorInfo cursorInfo,
            CursorInfo newCursorInfo,
            int newPressure,
            bool newEditing,
            MoveNextCursorAction action)
        {
            try
            {
                ApplyCursorMove(pe,cursorInfo, newCursorInfo, action, 0, "", "");

                int oldPressure = pe.backupPressure;
                pe.PressureValue = newPressure;

                if (!newEditing) commit(pe);

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
            MIDIPolyphonicKeyPressureEvent pe,
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
            MIDIPolyphonicKeyPressureEvent pe,
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

        private void commit(MIDIPolyphonicKeyPressureEvent pe)
        {
            pe.editing = false;

            MIDIPolyphonicKeyPressureEvent.oST = pe.ST;
            MIDIPolyphonicKeyPressureEvent.oKeyNumber = pe.KeyNumber;
            MIDIPolyphonicKeyPressureEvent.oPressure = pe.PressureValue;
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern)
        {
            if (!me.editing) return;

            MIDIPolyphonicKeyPressureEvent pe = (MIDIPolyphonicKeyPressureEvent)me;
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
                        MoveNext_ST2KeyNumber(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;

                    case 3:
                        if (!int.TryParse(pe.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(pe.editVal) && !int.TryParse(pe.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 60;
                            }
                        MoveNext_KeyNumber2Pressure(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextField);
                        break;

                    case 4:
                        if (!int.TryParse(pe.editVal, out ni))
                            if (string.IsNullOrWhiteSpace(pe.editVal) && !int.TryParse(pe.virVal, out ni))
                            {
                                WinApi.Beep();
                                ni = 64;
                            }
                        MoveNext_Pressure(pe, cursorInfo, newCursorInfo, ni, false, MoveNextCursorAction.NextEvent);
                        break;
                }
            }

            // Enter / Arrow keys の場合は UpdateActiveXXX('\r') を呼ぶ
            switch (cursorInfo.Position)
            {
                case 2:
                    UpdateActivePK_ST(pe, '\r', keyPattern, cursorInfo);
                    break;

                case 3:
                    UpdateActivePK_KeyNumber(pe, '\r', keyPattern, cursorInfo);
                    break;

                case 4:
                    UpdateActivePK_Pressure(pe, '\r', keyPattern, cursorInfo);
                    break;
            }

        }

        public override string GetVal(MIDIEvent me, CursorInfo cursorInfo)
        {
            return cursorInfo.Position switch
            {
                2 => ((MIDIPolyphonicKeyPressureEvent)me).ST.ToString(),
                3 => ((MIDIPolyphonicKeyPressureEvent)me).KeyNumber.ToString(),
                4 => ((MIDIPolyphonicKeyPressureEvent)me).PressureValue.ToString(),
                _ => ""
            };
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIPolyphonicKeyPressureEvent pe = (MIDIPolyphonicKeyPressureEvent)me;
            return $"{pe.Meas + 1},{pe.Step + 1}: PolyphonicKeyPressure: {pe.ST}, {pe.KeyNumber}, {pe.PressureValue}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',');
            if (vals.Length < 3) return null;

            if (!int.TryParse(vals[0], out int st)) return null;
            if (!int.TryParse(vals[1], out int key)) return null;
            if (!int.TryParse(vals[2], out int pressure)) return null;

            return new MIDIPolyphonicKeyPressureEvent()
            {
                Meas = meas,
                Step = step,
                ST = st,
                KeyNumber = key,
                PressureValue = pressure
            };
        }

        public override void CopyFrom(MIDIEvent srcMe, MIDIEvent otherMe, MdlEvent otherMdle)
        {
            base.CopyFrom(srcMe, otherMe, otherMdle);

            var n = (MIDIPolyphonicKeyPressureEvent)otherMe;
            n.CopyFrom(otherMe);

            ((MIDIPolyphonicKeyPressureEvent)srcMe).KeyNumber = n.KeyNumber;
            ((MIDIPolyphonicKeyPressureEvent)srcMe).PressureValue = n.PressureValue;
        }

    }
}
