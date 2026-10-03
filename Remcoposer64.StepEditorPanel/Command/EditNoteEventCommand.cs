using Remcoposer64.StepEditorPanelControl.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.UndoRedoManager;
using static Remcoposer64.StepEditorPanelControl.Keyboard;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;

namespace Remcoposer64.StepEditorPanelControl.Command
{
    public class EditNoteEventCommand : ICommand
    {
        private EventManager eventManager;
        private StepEditorPanelControl.StepEditorPanel panel;
        private int rowIndex;
        private int colIndex;

        private int oldCursorIndex;
        private int newCursorIndex;

        private int oldCursorPos;
        private int newCursorPos;

        private object oldState;
        private object newState;
        private bool oldEditing;
        private bool newEditing;


        public EditNoteEventCommand(
            EventManager eventManager,
            StepEditorPanelControl.StepEditorPanel panel,
            int rowIndex,
            int colIndex,
            object oldState,
            object newState,
            int oldCursorIndex,
            int oldCursorPos,
            int newCursorIndex,
            int newCursorPos, bool oldEditing, bool newEditing)
        {
            this.eventManager = eventManager;
            this.panel = panel;
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;

            this.oldState = oldState;
            this.newState = newState;

            this.oldCursorIndex = oldCursorIndex;
            this.newCursorIndex = newCursorIndex;

            this.oldCursorPos = oldCursorPos;
            this.newCursorPos = newCursorPos;

            this.oldEditing = oldEditing;
            this.newEditing = newEditing;
        }

        public void Execute()
        {
            var ev = eventManager.GetEventAt(rowIndex);

            var note = ev as MIDINoteEvent;
            if (note == null) return;
            ApplyValue(note, newState);

            note.editing = newEditing;
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
            var ev = eventManager.GetEventAt(rowIndex);

            var note = ev as MIDINoteEvent;
            if (note == null) return;
            ApplyValue(note, oldState);

            note.editing = oldEditing;
            panel.keyb.inputState = oldEditing ? InputState.EditEvent : InputState.None;
            if (oldEditing)
            {
                eventManager.parent.backupEvent = note.Clone();
                eventManager.parent.backupEvent.editing = false;
            }

            panel.cursor.Index = oldCursorIndex;
            panel.cursor.Position = oldCursorPos;
            panel.cursor.EventNode = eventManager.GetEventNodeAtIndex(oldCursorIndex);

            if (oldEditing && note is MIDINoteEvent ne)
            {
                // ここで「編集開始時と同じ状態」に戻す
                switch (oldCursorPos)
                {
                    case 0:
                        ne.editVal = MdlNoteEvent.NOTENAME_SPACE;
                        ne.virVal = ne.NoteName;
                        break;

                    case 1:
                        ne.editVal = MdlNoteEvent.KEYNUMBER_SPACE;
                        ne.virVal = ne.KeyNumber.ToString().PadLeft(MdlNoteEvent.KEYNUMBER_MAXLEN);
                        break;

                    case 2:
                        ne.editVal = MdlNoteEvent.ST_SPACE;
                        ne.virVal = ne.ST.ToString().PadLeft(MdlNoteEvent.ST_MAXLEN);
                        break;

                    case 3:
                        ne.editVal = MdlNoteEvent.GT_SPACE;
                        ne.virVal = ne.GT.ToString().PadLeft(MdlNoteEvent.GT_MAXLEN);
                        break;

                    case 4:
                        ne.editVal = MdlNoteEvent.Vel_SPACE;
                        ne.virVal = ne.Vel.ToString().PadLeft(MdlNoteEvent.Vel_MAXLEN);
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

        private void ApplyValue(MIDINoteEvent ev, object value)
        {
            switch (colIndex)
            {
                case 0: 
                case 1:
                    var state = (NoteKeyState)value;
                    ev.KeyNumber = state.KeyNumber;
                    ev.NoteName = state.NoteName;
                    break;
                case 2: ev.ST = (int)value; break;
                case 3: ev.GT = (int)value; break;
                case 4: ev.Vel = (int)value; break;
            }
        }

        public override string ToString()
        {
            if (oldState is NoteKeyState os && newState is NoteKeyState ns)
                return $"EditNoteEventCommand: Row={rowIndex}, Col={colIndex}, OldState={os.NoteName}:{os.KeyNumber}, NewState={ns.NoteName}:{ns.KeyNumber}, OldCursor={oldCursorIndex}:{oldCursorPos}, NewCursor={newCursorIndex}:{newCursorPos}";
            else
                return $"EditNoteEventCommand: Row={rowIndex}, Col={colIndex}, OldState={oldState}, NewState={newState}, OldCursor={oldCursorIndex}:{oldCursorPos}, NewCursor={newCursorIndex}:{newCursorPos}";
        }

    }

    public struct NoteKeyState
    {
        public int KeyNumber;
        public string NoteName;

        public NoteKeyState(int keyNumber, string noteName)
        {
            KeyNumber = keyNumber;
            NoteName = noteName;
        }
    }

}
