using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.UndoRedoManager;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class InsertEventCommand : ICommand
    {
        private EventManager eventManager;
        private StepEditorPanel panel;
        private MIDIEvent ev;
        private int index;
        private int oldCursorIndex;
        private int newCursorIndex;


        public InsertEventCommand(EventManager eventManager, StepEditorPanel panel, MIDIEvent ev, int index, int cursorIndex)
        {
            this.eventManager = eventManager;
            this.panel = panel;
            this.ev = ev;
            this.index = index;

            this.oldCursorIndex = cursorIndex;
            this.newCursorIndex = cursorIndex; // 挿入後の位置（必要なら +1）
        }

        public void Execute()
        {
            eventManager.InsertEventAt(index, ev);
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
            eventManager.RemoveEventAt(index);
            panel.cursor.Index = oldCursorIndex;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);
            panel.keyb.inputState = Keyboard.InputState.None;

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
