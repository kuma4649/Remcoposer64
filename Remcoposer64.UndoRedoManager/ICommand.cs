using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.UndoRedoManager
{
    public interface ICommand
    {
        void Execute();
        void Unexecute();
    }
}
