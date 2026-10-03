using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.UndoRedoManager;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class EditSysExEventCommand : ICommand
    {
        private EventManager eventManager;
        private StepEditorPanel panel;

        private int rowIndex;

        private object oldVal;
        private object newVal;

        private int oldCursorIndex;
        private int newCursorIndex;
        private int oldCursorPos;
        private int newCursorPos;

        private bool oldEditing;
        private bool newEditing;

        public EditSysExEventCommand(
            EventManager eventManager,
            StepEditorPanel panel,
            int rowIndex,
            // --- old block ---
            object oldVal,
            int oldCursorIndex,
            int oldCursorPos,
            bool oldEditing,
            // --- new block ---
            object newVal,
            int newCursorIndex,
            int newCursorPos,
            bool newEditing)
        {
            this.eventManager = eventManager;
            this.panel = panel;
            this.rowIndex = rowIndex;

            this.oldVal = oldVal;
            this.newVal = newVal;

            this.oldCursorIndex = oldCursorIndex;
            this.newCursorIndex = newCursorIndex;

            this.oldCursorPos = oldCursorPos;
            this.newCursorPos = newCursorPos;

            this.oldEditing = oldEditing;
            this.newEditing = newEditing;
        }

        public void Execute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDISysExEvent;
            if (ev == null) return;

            switch(newVal)
            {
                case int st:
                    // ★ ST 編集
                    ev.ST = st;
                    ev.editVal = st.ToString();
                    break;

                case byte[] raw:
                    // ★ Exclusive 編集
                    ev.Data = raw;
                    ev.editVal = ev.DataString;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported SysExEvent value type");
            }

            ev.editing = newEditing;

            panel.keyb.inputState = newEditing ? Keyboard.InputState.EditEvent : Keyboard.InputState.None;

            panel.cursor.Index = newCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(newCursorIndex);
            panel.cursor.Position = newCursorPos;

            panel.RefreshView();
        }

        public void Unexecute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDISysExEvent;
            if (ev == null) return;

            switch (oldVal)
            {
                case int st:
                    ev.ST = st;
                    ev.editVal = st.ToString();
                    break;

                case byte[] raw:
                    ev.Data = raw;
                    ev.editVal = ev.DataString;
                    break;
            }

            ev.editing = oldEditing;

            panel.keyb.inputState = oldEditing ? Keyboard.InputState.EditEvent : Keyboard.InputState.None;

            panel.cursor.Index = oldCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);
            panel.cursor.Position = oldCursorPos;

            panel.RefreshView();
        }

        public override string ToString()
        {
            return $"EditSysExEventCommand: Row={rowIndex}, OldPos={oldCursorPos}, OldVal={oldVal}, NewPos={newCursorPos}, NewVal={newVal}";
        }
    }
}
