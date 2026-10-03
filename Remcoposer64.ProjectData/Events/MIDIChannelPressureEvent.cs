using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIChannelPressureEvent : MIDIEvent
    {
        public static readonly int PressureValue_MINVALUE = 0;
        public static readonly int PressureValue_MAXVALUE = 127;

        public int PressureChannel { get; set; } = 0;
        public int backupPressureChannel;
        public static int oPressureChannel;

        public int PressureValue { get; set; } = 64;
        public int backupPressureValue;
        public static int oPressureValue;

        public override MIDIEvent Clone()
        {
            return new MIDIChannelPressureEvent()
            {
                Type = MIDIEventType.ChannelAfterTouch,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                PressureChannel = this.PressureChannel,
                PressureValue = this.PressureValue
            };
        }
    }
}
