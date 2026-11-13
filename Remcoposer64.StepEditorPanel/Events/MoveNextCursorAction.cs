using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanel.Events
{
    public enum MoveNextCursorAction
    {
        NextEvent,      // flg = 0
        PreviousEvent,  // flg = 1
        NextField,      // flg = 2
        StayOnField     // flg = 3
    }
}
