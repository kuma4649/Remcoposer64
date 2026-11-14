using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class EditMarkerEventCommand : ICommand
    {
        private EventManager eventManager;
        private StepEditorPanel panel;

        private int rowIndex;

        private string oldText;
        private string newText;

        private int oldCursorIndex;
        private int newCursorIndex;

        private int oldCaretPos;
        private int newCaretPos;

        private bool oldEditing;
        private bool newEditing;

        public EditMarkerEventCommand(
            EventManager eventManager,
            StepEditorPanel panel,
            int rowIndex,
            string oldText,
            string newText,
            int oldCursorIndex,
            int oldCaretPos,
            int newCursorIndex,
            int newCaretPos,
            bool oldEditing,
            bool newEditing)
        {
            this.eventManager = eventManager;
            this.panel = panel;
            this.rowIndex = rowIndex;

            this.oldText = oldText;
            this.newText = newText;

            this.oldCursorIndex = oldCursorIndex;
            this.newCursorIndex = newCursorIndex;

            this.oldCaretPos = oldCaretPos;
            this.newCaretPos = newCaretPos;

            this.oldEditing = oldEditing;
            this.newEditing = newEditing;
        }

        public void Execute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDIMarkerEvent;
            if (ev == null) return;
            var mev = (MdlMarkerEvent)eventManager.GetModel(ev.Type);

            ev.Text = newText;
            ev.editing = newEditing;

            panel.keyb.inputState = newEditing ? Keyboard.InputState.EditEvent : Keyboard.InputState.None;

            panel.cursor.Index = newCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(newCursorIndex);

            mev.memoBoxText = newText;
            mev.memoBoxCaretIndex = newCaretPos;
            mev.memoBoxSwitch = newEditing;

            var startNode = eventManager.FindStartNodeForAdjust(
                panel.cursor.EventNode
            );
            eventManager.AdjustMeasureAndStepInCurrentEvent(
                startNode,
                panel.cursor.EventNode.Value.Type
            );

            panel.RefreshView();
            panel.RefreshTrackEditor();
        }

        public void Unexecute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDIMarkerEvent;
            if (ev == null) return;
            var mev = (MdlMarkerEvent)eventManager.GetModel(ev.Type);

            ev.Text = oldText;
            ev.editing = oldEditing;

            panel.keyb.inputState = oldEditing ? Keyboard.InputState.EditEvent : Keyboard.InputState.None;
            //if (oldEditing)
            //{
            //    eventManager.parent.backupEvent = ev.Clone();
            //    eventManager.parent.backupEvent.editing = false;
            //}

            panel.cursor.Index = oldCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);

            mev.memoBoxText = oldText;
            mev.memoBoxCaretIndex = oldCaretPos;
            mev.memoBoxSwitch = oldEditing;

            // 編集開始時の IME 状態を再構築（NoteEvent の virVal/editVal に相当）
            if (oldEditing)
            {
                // IME composition window の位置を再設定する必要がある場合はここで行う
                mev.UpdateMemoBoxInfo(panel);
            }

            var startNode = eventManager.FindStartNodeForAdjust(
                panel.cursor.EventNode
            );
            eventManager.AdjustMeasureAndStepInCurrentEvent(
                startNode,
                panel.cursor.EventNode.Value.Type
            );

            panel.RefreshView();
            panel.RefreshTrackEditor();
        }

        public override string ToString()
        {
            return $"EditMarkerEventCommand: Row={rowIndex}, OldText=\"{oldText}\", NewText=\"{newText}\", OldCursor={oldCursorIndex}:{oldCaretPos}, NewCursor={newCursorIndex}:{newCaretPos}";
        }
    }
}
