using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Remcoposer64.UndoRedoManager
{
    public class UndoRedoManager
    {
        private Stack<ICommand> undoStack = new();
        private Stack<ICommand> redoStack = new();

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;


        public void Execute(ICommand cmd)
        {
            cmd.Execute();
            undoStack.Push(cmd);
            redoStack.Clear();
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;
            var cmd = undoStack.Pop();
            cmd.Unexecute();
            redoStack.Push(cmd);
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;
            var cmd = redoStack.Pop();
            cmd.Execute();
            undoStack.Push(cmd);
        }

        public string GetDebugInfo()
        {
            var sb = new StringBuilder();

            sb.AppendLine("=== Undo Stack ===");
            foreach (var cmd in undoStack)
            {
                sb.AppendLine(cmd.ToString());
            }

            sb.AppendLine("=== Redo Stack ===");
            foreach (var cmd in redoStack)
            {
                sb.AppendLine(cmd.ToString());
            }

            return sb.ToString();
        }

    }
}
