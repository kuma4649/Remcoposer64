using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIControlChangeEvent : MIDIEvent
    {
        public static readonly int Controller_MIN = 0;
        public static readonly int Controller_MAX = 127;

        public int ControllerNumber { get; set; }
        public int ControllerValue { get; set; }
        public int backupControllerNumber;
        public int backupControllerValue;
        public static int oControllerNumber;
        public static int oControllerValue;

        public override MIDIEvent Clone()
        {
            return new MIDIControlChangeEvent()
            {
                Type = MIDIEventType.ControlChange,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                ControllerNumber = this.ControllerNumber,
                ControllerValue = this.ControllerValue,
            };
        }
    }
}
