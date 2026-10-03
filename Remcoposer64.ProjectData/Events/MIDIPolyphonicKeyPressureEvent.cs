using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIPolyphonicKeyPressureEvent : MIDIEvent
    {
        public static readonly int KeyMin = 0;
        public static readonly int KeyMax = 127;

        public static readonly int PressureMin = 0;
        public static readonly int PressureMax = 127;

        public int KeyNumber { get; set; } = 60;
        public int backupKeyNumber;
        public static int oKeyNumber;
        public int PressureValue { get; set; } = 64;
        public int backupPressure;
        public static int oPressure;

        public MIDIPolyphonicKeyPressureEvent()
        {
            Type = MIDIEventType.KeyAfterTouch;
        }

        public override MIDIEvent Clone()
        {
            return new MIDIPolyphonicKeyPressureEvent()
            {
                Type = MIDIEventType.KeyAfterTouch,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                KeyNumber = this.KeyNumber,
                PressureValue= this.PressureValue,
            };
        }
    }
}
