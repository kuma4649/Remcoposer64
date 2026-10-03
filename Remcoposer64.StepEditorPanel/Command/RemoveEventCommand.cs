using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.UndoRedoManager;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class RemoveEventCommand : ICommand
    {
        private EventManager eventManager;
        private StepEditorPanel panel;
        private MIDIEvent removedEvent;
        private int index;
        private int oldCursorIndex;
        private int newCursorIndex;

        public RemoveEventCommand(EventManager eventManager, StepEditorPanel panel, int index, int cursorIndex)
        {
            this.eventManager = eventManager;
            this.panel = panel;
            this.index = index;

            this.oldCursorIndex = cursorIndex;
            this.newCursorIndex = cursorIndex;// Math.Max(0, cursorIndex - 1);

            //// 削除前にイベントをコピーして保持
            //removedEvent = eventManager.GetEventAt(index).Clone();
        }

        public void Execute()
        {
            removedEvent = eventManager.GetEventAt(index).Clone();
            eventManager.RemoveEventAt(index);

            panel.cursor.Index = newCursorIndex;
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
            eventManager.InsertEventAt(index, removedEvent);

            panel.cursor.Index = oldCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);

            var startNode = eventManager.FindStartNodeForAdjust(
                panel.cursor.EventNode
            );
            eventManager.AdjustMeasureAndStepInCurrentEvent(
                startNode,
                panel.cursor.EventNode.Value.Type
            );

            panel.RefreshView();
        }
    }
}
