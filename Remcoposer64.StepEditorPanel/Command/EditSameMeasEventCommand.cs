using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.UndoRedoManager;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class EditSameMeasEventCommand : ICommand
    {
        private EventManager eventManager;
        private StepEditorPanel panel;

        private int rowIndex;

        private string oldVal;
        private string newVal;

        private int oldCursorIndex;
        private int newCursorIndex;

        private bool oldEditing;
        private bool newEditing;

        public EditSameMeasEventCommand(
            EventManager eventManager,
            StepEditorPanel panel,
            int rowIndex,
            // --- old block ---
            string oldVal,
            int oldCursorIndex,
            bool oldEditing,
            // --- new block ---
            string newVal,
            int newCursorIndex,
            bool newEditing)
        {
            this.eventManager = eventManager;
            this.panel = panel;
            this.rowIndex = rowIndex;

            this.oldVal = oldVal;
            this.newVal = newVal;

            this.oldCursorIndex = oldCursorIndex;
            this.newCursorIndex = newCursorIndex;

            this.oldEditing = oldEditing;
            this.newEditing = newEditing;
        }

        public void Execute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDISameMeasEvent;
            if (ev == null) return;

            ev.RepeatMeas = int.Parse(newVal) - 1;
            eventManager.SetRepeatMeasNode(ev);
            ev.editVal = newVal;
            ev.editing = newEditing;

            panel.keyb.inputState = newEditing ? Keyboard.InputState.EditEvent : Keyboard.InputState.None;

            panel.cursor.Index = newCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(newCursorIndex);

            panel.RefreshView();
        }

        public void Unexecute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDISameMeasEvent;
            if (ev == null) return;

            ev.RepeatMeas = int.Parse(oldVal) - 1;
            eventManager.SetRepeatMeasNode(ev);
            ev.editVal = oldVal;
            ev.editing = oldEditing;

            panel.keyb.inputState = oldEditing ? Keyboard.InputState.EditEvent : Keyboard.InputState.None;

            panel.cursor.Index = oldCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);

            panel.RefreshView();
        }

        public override string ToString()
        {
            return $"EditSameMeasEventCommand: Row={rowIndex}, OldVal={oldVal}, NewVal={newVal}";
        }
    }
}
