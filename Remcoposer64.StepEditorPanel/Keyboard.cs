using Remcoposer64.StepEditorPanelControl.Events;
using System.Diagnostics;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;

namespace Remcoposer64.StepEditorPanelControl
{
    public class Keyboard
    {
        private StepEditorPanel parent;
        private bool CancelKeyPress = false;

        // 現在の入力状態
        public InputState inputState = InputState.None;

        public char lastKeyChar { get; private set; }

        public enum InputState
        {
            None,
            EditEvent,
        }

        public Keyboard(StepEditorPanel parent)
        {
            this.parent = parent;
        }


        /// <summary>
        /// このメソッドはHandleKeyPressよりも先に処理される
        /// </summary>
        /// <param name="keyData"></param>
        public void HandleKeyDown(uint keyData)
        {
            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            CancelKeyPress = false;

            // ★モディファイア除去
            Keys key = (Keys)(keyData & (uint)Keys.KeyCode);
            // モディファイアキーの状態を取得
            bool ctrl = (keyData & (uint)Keys.Control) != 0;
            bool shift = (keyData & (uint)Keys.Shift) != 0;
            bool alt = (keyData & (uint)Keys.Alt) != 0;

            Debug.WriteLine($"KeyDown: {key}, Ctrl: {ctrl}, Shift: {shift}, Alt: {alt}");

            //CCのポップアップウィンドウが表示中の場合はカーソルキーなどの情報を渡して処理終了
            var ccForm = parent.CCcandidateForm;
            // ★ 先に候補フォームが表示中かチェック
            if (ccForm != null && ccForm.Visible)
            {
                switch (key)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Enter:
                        ccForm.MoveSelection(key);
                        CancelKeyPress = true;
                        return;
                    case Keys.Escape:
                        ccForm.Visible = false;
                        CancelKeyPress = true;
                        return;
                }
            }

            var pcForm = parent.PCcandidateForm;

            if (pcForm != null && pcForm.Visible)
            {
                switch (key)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Enter:
                        pcForm.MoveSelection(key);
                        CancelKeyPress = true;
                        return;

                    case Keys.Escape:
                        pcForm.Visible = false;
                        CancelKeyPress = true;
                        return;
                }
            }

            switch (key)
            {
                // カーソル移動系
                case Keys.Up:
                    BeginShiftSelectionIfNeeded(shift, ctrl);
                    MoveCursorUp();
                    UpdateShiftSelection(shift);
                    break;
                case Keys.Down:
                    BeginShiftSelectionIfNeeded(shift, ctrl);
                    MoveCursorDown();
                    UpdateShiftSelection(shift);
                    break;
                case Keys.Left: MoveCursorLeft(); break;
                case Keys.Right: MoveCursorRight(); break;
                case Keys.Home:
                    if (ctrl) MoveCursorToFirstEvent();
                    else MoveCursorTopMeasure();
                    break;
                case Keys.End:
                    if (ctrl) MoveCursorToLastEvent();
                    else MoveCursorNextMeasure();
                    break;
                case Keys.PageUp: MoveCursorPageUp(); break;
                case Keys.PageDown: MoveCursorPageDown(); break;
                case Keys.Enter:
                    if (inputState == InputState.None)
                    {
                        var sx = parent.cursor.EventNode.Value as MIDISysExEvent;

                        // ★ SysExEvent の編集開始（上書きモード限定）
                        if (sx != null && parent.OverwriteMode)
                        {
                            MdlSysExEvent me = (MdlSysExEvent)parent.eventManager.GetModel(sx.Type);
                            me.BeginEditExternal(sx, parent.cursor, this);
                            parent.ScreenUpdate();
                            CancelKeyPress = true;   // Enter の通常動作をキャンセル
                            break;
                        }

                        // ★ 通常の Enter 動作（次の行へ移動）
                        MoveCursorDown();
                    }
                    break;

                // モード切替系
                case Keys.Insert: parent.ToggleOverwriteMode(); break;

                // 編集系
                case Keys.Delete:
                    // ★ 通常時はイベント削除
                    DeleteEvent(activeEvent);
                    break;
                case Keys.Back: if (inputState == InputState.None) BackSpaceEvent(activeEvent); break;
                case Keys.Escape: EscapeEvent(activeEvent); break;
                case Keys.Z: // Undo (Ctrl + Z) / Redo (Ctrl + Shift + Z)
                    if (ctrl && shift) {
                        parent.ForceEndEditing();   // ★ 編集モード強制終了
                        parent.Redo();
                        CancelKeyPress = true;
                    }
                    else if (ctrl) {
                        parent.ForceEndEditing();   // ★ 編集モード強制終了
                        parent.Undo();
                        CancelKeyPress = true; 
                    }
                    break;
                case Keys.Y: // Redo (Ctrl + Y)
                    if (ctrl) {
                        parent.ForceEndEditing();   // ★ 編集モード強制終了
                        parent.Redo();
                        CancelKeyPress = true;
                    }
                    break;
                case Keys.C: // ★ コピー（Ctrl + C）
                    if (ctrl) {
                        bool hasRangeSelection =
                            parent.cursor.SelectionStart >= 0 &&
                            parent.cursor.SelectionEnd >= 0 &&
                            parent.cursor.SelectionStart != parent.cursor.SelectionEnd;

                        bool hasMultiSelection =
                            parent.cursor.MultiSelectIndices.Count > 0;

                        if (hasRangeSelection || hasMultiSelection)
                        {
                            parent.CopySelectedRows();
                        }
                        else
                        {
                            parent.CopyCurrentRow();
                        }

                        CancelKeyPress = true;
                    }
                    break;
                case Keys.X: // ★ カット（Ctrl + X）
                    if (ctrl)
                    {
                        bool hasRangeSelection =
                            parent.cursor.SelectionStart >= 0 &&
                            parent.cursor.SelectionEnd >= 0 &&
                            parent.cursor.SelectionStart != parent.cursor.SelectionEnd;

                        bool hasMultiSelection =
                            parent.cursor.MultiSelectIndices.Count > 0;

                        if (hasRangeSelection || hasMultiSelection)
                        {
                            parent.CutSelectedRows();
                        }
                        else
                        {
                            parent.CutCurrentRow();
                        }

                        CancelKeyPress = true;
                    }
                    break;
                case Keys.V: // ★ ペースト（Ctrl + V）
                    if (ctrl) { parent.PasteToCurrentRow(); CancelKeyPress = true; }
                    break;
                case Keys.OemQuestion:   // “？”キー
                case Keys.Divide:   // “／”キー
                    if (inputState == InputState.EditEvent)
                    {
                        CommitActiveEvent(parent.cursor.EventNode.Value, CommitKeyPattern.OtherKey);
                    }
                    if (parent.OverwriteMode) parent.ToggleOverwriteMode();
                    parent.ShowEventContextMenu();
                    CancelKeyPress = true;
                    break;
            }
        }


        public void HandleKeyPress(char keyChar)
        {
            if (CancelKeyPress) { CancelKeyPress = false; return; }

            CommitKeyPattern keyPattern = CommitKeyPattern.OtherKey;
            if (keyChar == '\r') keyPattern = CommitKeyPattern.EnterKey;

            if (inputState == InputState.None)
            {
                HandleKeyPressNone(keyChar, keyPattern);
            }
            else
            {
                HandleKeyPressEditEvent(keyChar, keyPattern);
            }
        }

        private void HandleKeyPressNone(char keyChar, CommitKeyPattern keyPattern)
        {

            // KeyDown で処理されるキーは弾く
            if (keyChar == '\b' // BackSpace
                || keyChar == '\r' // Enter
                || keyChar == '\u001b' // Escape
                ) return;

            MIDIEvent activeEvent;
            MdlEvent mdlActiveEvent;
            if (parent.OverwriteMode)
            {
                activeEvent = parent.cursor.EventNode.Value;
                mdlActiveEvent = parent.eventManager.GetModel(activeEvent.Type);
                // 上書きキャンセル操作向けにイベントのバックアップを取る
                parent.backupEvent = activeEvent.Clone();
                lastKeyChar = keyChar;
                if (!mdlActiveEvent.BeginEdit(activeEvent, parent.cursor, this))
                {
                    WinApi.Beep();
                    return;
                }
            }
            else
            {
                if (!TryDetermineEventType(keyChar, out activeEvent))
                {
                    WinApi.Beep();
                    return;
                }

                InsertEvent(activeEvent);
                inputState = InputState.EditEvent;
            }

            UpdateActiveEvent(activeEvent, keyChar, keyPattern);
        }

        private void HandleKeyPressEditEvent(char keyChar, CommitKeyPattern keyPattern)
        {
            MIDIEvent activeEvent = parent.cursor.EventNode.Value;

            UpdateActiveEvent(activeEvent, keyChar, keyPattern);

            if (!activeEvent.editing)
            {
                CommitAndAdvanceCursor(activeEvent);
                inputState = InputState.None;
            }
        }

        private void CommitAndAdvanceCursor(MIDIEvent activeEvent)
        {
            if (parent.cursor.EventNode.Next == null)
                return;

            var startNode = parent.eventManager.FindStartNodeForAdjust(
                parent.cursor.EventNode
            );

            parent.eventManager.AdjustMeasureAndStepInCurrentEvent(
                startNode,
                parent.cursor.EventNode.Value.Type
            );

            parent.ScreenUpdate();
        }


        private void BeginShiftSelectionIfNeeded(bool shift,bool ctrl)
        {
            if (!shift)
            {
                if (!ctrl) parent.cursor.ClearSelection();
                return;
            }

            if (parent.cursor.SelectionStart < 0)
            {
                parent.cursor.SelectionStart = parent.cursor.Index;
                parent.cursor.SelectionEnd = parent.cursor.Index;
            }
        }

        private void UpdateShiftSelection(bool shift)
        {
            if (!shift) return;

            if (parent.cursor.SelectionStart >= 0)
            {
                parent.cursor.SelectionEnd = parent.cursor.Index;
            }
        }

        private void ToggleCtrlSelection(bool ctrl)
        {
            if (!ctrl) return;

            int row = parent.cursor.Index;

            if (parent.cursor.MultiSelectIndices.Contains(row))
                parent.cursor.MultiSelectIndices.Remove(row);
            else
                parent.cursor.MultiSelectIndices.Add(row);
        }


        private void MoveCursorRight()
        {
            var me = parent.cursor.EventNode.Value as MIDIMemoEvent;
            if (me != null)
            {
                MdlMemoEvent mme = (MdlMemoEvent)parent.eventManager.GetModel(me.Type);
                if (mme.memoBoxSwitch)
                {
                    mme.memoBoxCaretIndex = Math.Min(mme.memoBoxText.Length, mme.memoBoxCaretIndex + 1);
                    mme.UpdateMemoBoxInfo(parent);
                    parent.ScreenUpdate();
                    return;
                }

                // ★ MemoEvent 非編集時に右キーで編集開始
                if (inputState == InputState.None && parent.OverwriteMode)
                {
                    mme.memoBoxSwitch = true;
                    mme.BeginEdit(me, parent.cursor, this);
                    mme.memoBoxCaretIndex = me.Text.Length;   // 右端
                    parent.ScreenUpdate();
                    return;
                }
            }

            if (inputState == InputState.EditEvent)
            {
                if (parent.cursor.Position == 4)
                {
                    CommitActiveEvent(parent.cursor.EventNode.Value, CommitKeyPattern.RightKey);
                    if (!parent.OverwriteMode) parent.cursor.Position = 0;
                }
                else
                {
                    CommitActiveEvent(parent.cursor.EventNode.Value, CommitKeyPattern.RightKey);
                    inputState = InputState.EditEvent;
                }
                parent.ScreenUpdate();
                return;
            }

            MdlEvent m = parent.eventManager.GetModel(parent.cursor.EventNode.Value.Type);
            parent.cursor.Position = m.AdjustCursorPosition(parent.cursor.Position);
            if (parent.cursor.Position < 4)
            {
                parent.cursor.Position++;
                parent.cursor.Position = m.AdjustCursorPosition(parent.cursor.Position);
                parent.cursor.EventNode.Value.editVal =
                parent.cursor.EventNode.Value.virVal = m.GetVal(parent.cursor.EventNode.Value, parent.cursor);
                parent.ScreenUpdate();
            }
        }

        private void MoveCursorLeft()
        {
            var me = parent.cursor.EventNode.Value as MIDIMemoEvent;
            if (me != null)
            {
                MdlMemoEvent mme = (MdlMemoEvent)parent.eventManager.GetModel(me.Type);
                if (mme.memoBoxSwitch)
                {
                    if (parent.imeMode == false)
                    {
                        mme.memoBoxCaretIndex = Math.Max(0, mme.memoBoxCaretIndex - 1);
                        mme.UpdateMemoBoxInfo(parent);
                        parent.ScreenUpdate();
                        return;
                    }
                }
                else
                {
                    // ★ MemoEvent 非編集時に左キーで編集開始
                    if (inputState == InputState.None && parent.OverwriteMode)
                    {
                        mme.memoBoxSwitch = true;
                        mme.BeginEdit(me, parent.cursor, this);
                        mme.memoBoxCaretIndex = Math.Max(0, me.Text.Length - 1); // 右端−1
                        parent.ScreenUpdate();
                        return;
                    }
                }
            }

            MdlEvent m = parent.eventManager.GetModel(parent.cursor.EventNode.Value.Type);
            parent.cursor.Position = m.AdjustCursorPosition(parent.cursor.Position);
            if (parent.cursor.Position > 0)
            {
                parent.cursor.Position--;
                // PitchBendEvent / TempoEvent のときは Position=4 → Position=2 に直接移動させる
                if (parent.cursor.EventNode.Value is MIDIPitchBendEvent
                    || parent.cursor.EventNode.Value is MIDITempoEvent) parent.cursor.Position = 2;
                parent.cursor.Position = m.AdjustCursorPosition(parent.cursor.Position);
                parent.cursor.EventNode.Value.editVal =
                parent.cursor.EventNode.Value.virVal = m.GetVal(parent.cursor.EventNode.Value, parent.cursor);
                parent.ScreenUpdate();
            }
        }

        private void MoveCursorDown()
        {
            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None)
            {
                CommitActiveEvent(activeEvent, CommitKeyPattern.DownKey);
                if (parent.cursor.EventNode.Next == null)
                {
                    WinApi.Beep();
                    return;
                }
            }
            else
            {
                if (parent.cursor.EventNode.Next == null)
                {
                    WinApi.Beep();
                    return;
                }
                parent.cursor.Index++;
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
            }
            parent.ScreenUpdate();
        }

        private void MoveCursorUp()
        {
            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None)
            {
                CommitActiveEvent(activeEvent, CommitKeyPattern.UpKey);
                parent.ScreenUpdate();
                return;
            }

            if (parent.cursor.Index == 0)
            {
                WinApi.Beep();
                return;
            }
            parent.cursor.Index--;
            parent.cursor.EventNode = parent.cursor.EventNode.Previous;
            parent.ScreenUpdate();
        }

        private void MoveCursorToLastEvent()
        {
            //OtherKeyの場合はカーソルの移動制御は自分で行う

            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None) CommitActiveEvent(activeEvent, CommitKeyPattern.OtherKey);

            // 最後のノードまで移動
            while (parent.cursor.EventNode.Next != null)
            {
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
                parent.cursor.Index++;
            }

            parent.ScreenUpdate();
        }

        private void MoveCursorToFirstEvent()
        {
            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None) CommitActiveEvent(activeEvent, CommitKeyPattern.OtherKey);

            while (parent.cursor.EventNode.Previous != null)
            {
                parent.cursor.EventNode = parent.cursor.EventNode.Previous;
                parent.cursor.Index--;
            }

            parent.ScreenUpdate();
        }

        private void MoveCursorNextMeasure()
        {
            //OtherKeyの場合はカーソルの移動制御は自分で行う

            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None) CommitActiveEvent(activeEvent, CommitKeyPattern.OtherKey);

            if (parent.cursor.EventNode.Value.Type == MIDIEventType.MetaEndOfTrack)
            {
                WinApi.Beep();
                return;
            }

            if (parent.cursor.EventNode.Value.Type == MIDIEventType.SameMeas)
            {
                parent.cursor.Index++;
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
                parent.ScreenUpdate();
                return;
            }

            parent.cursor.Index++;
            parent.cursor.EventNode = parent.cursor.EventNode.Next;

            while (parent.cursor.EventNode != null
                && parent.cursor.EventNode.Value.Type != MIDIEventType.BarLine
                && parent.cursor.EventNode.Value.Type != MIDIEventType.SameMeas
                && parent.cursor.EventNode.Value.Type != MIDIEventType.MetaEndOfTrack)
            {
                parent.cursor.Index++;
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
            }

            if (parent.cursor.EventNode.Value.Type == MIDIEventType.BarLine)
            {
                parent.cursor.Index++;
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
            }

            parent.ScreenUpdate();
        }

        private void MoveCursorTopMeasure()
        {
            //OtherKeyの場合はカーソルの移動制御は自分で行う

            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None) CommitActiveEvent(activeEvent, CommitKeyPattern.OtherKey);
            if (parent.cursor.Index == 0)
            {
                WinApi.Beep();
                return;
            }

            if (parent.cursor.EventNode.Previous.Value.Type == MIDIEventType.SameMeas)
            {
                parent.cursor.Index--;
                parent.cursor.EventNode = parent.cursor.EventNode.Previous;
                parent.ScreenUpdate();
                return;
            }

            // カーソルがBarLineの直後にある場合、BarLineに移動する。但し、カーソルがBarLineにある場合は、前のBarLineに移動しない
            if (parent.cursor.EventNode.Previous.Value.Type== MIDIEventType.BarLine)
            {
                if (parent.cursor.EventNode.Value.Type != MIDIEventType.BarLine)
                {
                    parent.cursor.Index--;
                    parent.cursor.EventNode = parent.cursor.EventNode.Previous;
                }
            }

            // とりあえず前のイベントに移動する
            parent.cursor.Index--;
            parent.cursor.EventNode = parent.cursor.EventNode.Previous;

            // その後、BarLineかSameMeasかEndに到達するまで前のイベントに移動する
            while (parent.cursor.Index != 0
                && parent.cursor.EventNode.Value.Type != MIDIEventType.BarLine
                && parent.cursor.EventNode.Value.Type != MIDIEventType.SameMeas
                && parent.cursor.EventNode.Value.Type != MIDIEventType.MetaEndOfTrack)
            {
                parent.cursor.Index--;
                parent.cursor.EventNode = parent.cursor.EventNode.Previous;
            }

            // BarLineに到達した場合は、BarLineの次のイベントに移動する
            if (parent.cursor.EventNode.Value.Type == MIDIEventType.BarLine
                || parent.cursor.EventNode.Value.Type == MIDIEventType.SameMeas)
            {
                parent.cursor.Index++;
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
                if (parent.cursor.EventNode.Value.Type == MIDIEventType.BarLine)
                {
                    parent.cursor.Index--;
                    parent.cursor.EventNode = parent.cursor.EventNode.Previous;
                }
            }

            parent.ScreenUpdate();
        }

        private void MoveCursorPageUp()
        {
            //OtherKeyの場合はカーソルの移動制御は自分で行う

            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None) CommitActiveEvent(activeEvent, CommitKeyPattern.OtherKey);

            for (int i = 0; i < parent.VisibleRows; i++)
            {
                if (parent.cursor.EventNode.Previous == null)
                {
                    WinApi.Beep();
                    break;
                }
                parent.cursor.EventNode = parent.cursor.EventNode.Previous;
                parent.cursor.Index--;
            }

            parent.ScreenUpdate(parent.cursor.Index > 0 ? parent.cursor.Index - 1 : 0);
        }

        private void MoveCursorPageDown()
        {
            //OtherKeyの場合はカーソルの移動制御は自分で行う

            MIDIEvent activeEvent = parent.cursor.EventNode.Value;
            if (inputState != InputState.None) CommitActiveEvent(activeEvent, CommitKeyPattern.OtherKey);

            for (int i = 0; i < parent.VisibleRows; i++)
            {
                if (parent.cursor.EventNode.Next == null)
                {
                    WinApi.Beep();
                    break;
                }
                parent.cursor.EventNode = parent.cursor.EventNode.Next;
                parent.cursor.Index++;
            }

            parent.ScreenUpdate(parent.cursor.Index > 0 ? parent.cursor.Index - 1 : 0);
        }


        private bool TryDetermineEventType(char keyChar, out MIDIEvent ev)
        {
            // keyCharが数値だった場合、EventはNoteイベントになる
            if (char.IsDigit(keyChar))
            {
                ev = new MIDINoteEvent();
                ((MIDINoteEvent)ev).editing = true;
                parent.cursor.Position = 1;
                return true;
            }
            if ((keyChar >= 'a' && keyChar <= 'g')
                || (keyChar >= 'A' && keyChar <= 'G'))// note名を直接入力した場合
            {
                ev = new MIDINoteEvent();
                ((MIDINoteEvent)ev).editing = true;
                parent.cursor.Position = 0;
                return true;
            }

            ev = null;
            return false;
        }

        private void UpdateActiveEvent(MIDIEvent activeEvent, char keyChar, CommitKeyPattern keyPattern)
        {
            MdlEvent me = parent.eventManager.GetModel(activeEvent.Type);
            bool finished = me.UpdateActiveEvent(activeEvent, keyChar, keyPattern, parent.cursor);

            parent.ScreenUpdate();

            if (finished)
            {
                CommitActiveEvent(activeEvent, keyPattern);
                inputState = InputState.None;
            }
        }

        public void CommitActiveEvent(MIDIEvent activeEvent, CommitKeyPattern keyPattern)
        {
            MdlEvent me = parent.eventManager.GetModel(activeEvent.Type);
            me.Commit(activeEvent, parent.cursor, keyPattern);
            if (activeEvent is MIDIMemoEvent)
            {
                ((MdlMemoEvent)me).memoBoxSwitch = false;
            }
            CommitAndAdvanceCursor(activeEvent);
            inputState = InputState.None;
            //Event入力確定
        }

        private void InsertEvent(MIDIEvent activeEvent)
        {
            parent.InsertEventAtCoursor(activeEvent);
        }

        private void DeleteEvent(MIDIEvent activeEvent)
        {
            MdlEvent me = parent.eventManager.GetModel(activeEvent.Type);
            // ★ MemoEvent 編集中なら文字削除にする
            if (activeEvent is MIDIMemoEvent && activeEvent.editing && ((MdlMemoEvent)me).memoBoxSwitch)
            {
                if (((MdlMemoEvent)me).memoBoxCaretIndex < ((MdlMemoEvent)me).memoBoxText.Length)
                {
                    ((MdlMemoEvent)me).memoBoxText = ((MdlMemoEvent)me).memoBoxText.Remove(((MdlMemoEvent)me).memoBoxCaretIndex, 1);
                    ((MIDIMemoEvent)activeEvent).Text = ((MdlMemoEvent)me).memoBoxText;
                    parent.Invalidate();
                }
                return; // ★ イベント削除ロジックに行かせない
            }

            // ★ まず複数選択されているかチェック
            bool hasRangeSelection =
                parent.cursor.SelectionStart >= 0 &&
                parent.cursor.SelectionEnd >= 0 &&
                parent.cursor.SelectionStart != parent.cursor.SelectionEnd;

            bool hasMultiSelection =
                parent.cursor.MultiSelectIndices.Count > 0;
            
            if (inputState == InputState.None)
            {
                // ★ 複数選択削除
                if (hasRangeSelection || hasMultiSelection)
                {
                    parent.DeleteSelectedRows();
                    return;
                }

                if (!parent.OverwriteMode)
                {
                    parent.DeleteEventAtCoursor(activeEvent);
                    return;
                }
                else
                {
                    inputState = InputState.EditEvent;
                    activeEvent.editing = true;
                    activeEvent.editVal = activeEvent.virVal = me.GetVal(activeEvent, parent.cursor);
                }
            }

            UpdateActiveEvent(activeEvent, (char)0, CommitKeyPattern.OtherKey);

            if (!activeEvent.editing)
            {
                CommitAndAdvanceCursor(activeEvent);
                inputState = InputState.None;
            }
        }

        private void BackSpaceEvent(MIDIEvent activeEvent)
        {
            parent.BackSpaceEventAtCoursor(activeEvent);
        }

        private void EscapeEvent(MIDIEvent activeEvent)
        {
            if (inputState != InputState.EditEvent)
            {
                // 編集中ではないときにESCを押した場合は、終了系向けのFireExitEvent(外部イベント)を呼ぶ
                parent.FireExitEvent();
                return;
            }

            if (parent.OverwriteMode)
                // 上書きモード時は、上書き前のイベントに戻す
                parent.RestoreEventAtCoursor(activeEvent);
            else
                // 挿入モード時は、挿入したイベントを消す
                parent.CancelEventAtCoursor(activeEvent);

            // 入力状態は"通常"に戻す
            inputState = InputState.None;
        }

    }
}