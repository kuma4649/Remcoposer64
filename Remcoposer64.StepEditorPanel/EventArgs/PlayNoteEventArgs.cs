using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.EventArgs
{
    public class PlayNoteEventArgs : System.EventArgs
    {
        public int DeviceNumber { get; }
        public int Channel { get; }
        public int KeyNumber { get; }

        public PlayNoteEventArgs(int deviceNumber,int channel,int keyNumber)
        {
            DeviceNumber = deviceNumber;
            Channel = channel; 
            KeyNumber = keyNumber;
        }
    }
}
