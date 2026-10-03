using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.EventArgs
{
    public class PlayNoteEventArgs : System.EventArgs
    {
        public int KeyNumber { get; }

        public PlayNoteEventArgs(int keyNumber)
        {
            KeyNumber = keyNumber;
        }
    }
}
