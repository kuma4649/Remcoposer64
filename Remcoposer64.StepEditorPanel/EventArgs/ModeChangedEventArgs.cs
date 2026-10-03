using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.EventArgs
{
    public class ModeChangedEventArgs : System.EventArgs
    {
        public bool IsOverwrite { get; }

        public ModeChangedEventArgs(bool isOverwrite)
        {
            IsOverwrite = isOverwrite;
        }
    }
}

