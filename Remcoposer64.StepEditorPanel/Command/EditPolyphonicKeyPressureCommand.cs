using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.UndoRedoManager;
using static Remcoposer64.StepEditorPanelControl.Keyboard;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class EditPolyphonicKeyPressureCommand : ICommand
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

        public EditPolyphonicKeyPressureCommand(
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
            var ev = eventManager.GetEventAt(rowIndex) as MIDIPolyphonicKeyPressureEvent;
            if (ev == null) return;

            ApplyValue(ev, newCursorPos, newVal);

            ev.editing = newEditing;

            panel.keyb.inputState = newEditing ? InputState.EditEvent : InputState.None;

            panel.cursor.Index = newCursorIndex;
            panel.cursor.Position = newCursorPos;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(newCursorIndex);

            var startNode = eventManager.FindStartNodeForAdjust(
                panel.cursor.EventNode
            );
            eventManager.AdjustMeasureAndStepInCurrentEvent(
                startNode,
                panel.cursor.EventNode.Value.Type
            );

            panel.RefreshView();
        }

        public void Unexecute()
        {
            var ev = eventManager.GetEventAt(rowIndex) as MIDIPolyphonicKeyPressureEvent;
            if (ev == null) return;

            ApplyValue(ev, oldCursorPos, oldVal);

            ev.editing = oldEditing;

            panel.keyb.inputState = oldEditing ? InputState.EditEvent : InputState.None;
            if (oldEditing)
            {
                eventManager.parent.backupEvent = ev.Clone();
                eventManager.parent.backupEvent.editing = false;
            }

            panel.cursor.Index = oldCursorIndex;
            panel.cursor.Position = oldCursorPos;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);

            if (oldEditing)
            {
                // ここで「編集開始時と同じ状態」に戻す
                switch (oldCursorPos)
                {
                    case 2: // ST
                        ev.editVal = MdlPolyphonicKeyPressureEvent.ST_SPACE;
                        ev.virVal = ev.ST.ToString().PadLeft(MdlPolyphonicKeyPressureEvent.ST_MAXLEN);
                        break;

                    case 3: // KeyNumber
                        ev.editVal = MdlPolyphonicKeyPressureEvent.KEY_SPACE;
                        ev.virVal = ev.KeyNumber.ToString().PadLeft(MdlPolyphonicKeyPressureEvent.KEY_MAXLEN);
                        break;

                    case 4: // Pressure
                        ev.editVal = MdlPolyphonicKeyPressureEvent.PRESS_SPACE;
                        ev.virVal = ev.PressureValue.ToString().PadLeft(MdlPolyphonicKeyPressureEvent.PRESS_MAXLEN);
                        break;
                }
            }

            var startNode = eventManager.FindStartNodeForAdjust(
                panel.cursor.EventNode
            );
            eventManager.AdjustMeasureAndStepInCurrentEvent(
                startNode,
                panel.cursor.EventNode.Value.Type
            );

            panel.RefreshView();
        }

        private void ApplyValue(MIDIPolyphonicKeyPressureEvent ev, int pos, object value)
        {
            switch (pos)
            {
                case 2: // ST
                    ev.ST = (int)value;
                    break;

                case 3: // KeyNumber
                    ev.KeyNumber = (int)value;
                    break;

                case 4: // Pressure
                    ev.PressureValue = (int)value;
                    break;

                default:
                    throw new InvalidOperationException("Unsupported PolyphonicKeyPressureEvent value type");
            }
        }

        public override string ToString()
        {
            return $"EditPolyphonicKeyPressureCommand: Row={rowIndex}, OldPos={oldCursorPos}, OldVal={oldVal}, NewPos={newCursorPos}, NewVal={newVal}";
        }
    }
}
