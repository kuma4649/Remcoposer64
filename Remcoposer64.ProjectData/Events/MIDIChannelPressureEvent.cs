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

        public MIDIChannelPressureEvent()
        {
            Type = MIDIEventType.ChannelAfterTouch;
        }

        public override MIDIEvent Clone()
        {
            MIDIChannelPressureEvent e =
                new MIDIChannelPressureEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIChannelPressureEvent)other;

            PressureChannel = e.PressureChannel;
            PressureValue = e.PressureValue;

            backupPressureChannel =
                e.backupPressureChannel;

            backupPressureValue =
                e.backupPressureValue;
        }
    }
}
