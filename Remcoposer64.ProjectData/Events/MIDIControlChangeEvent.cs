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

        public MIDIControlChangeEvent()
        {
            Type = MIDIEventType.ControlChange;
        }

        public override MIDIEvent Clone()
        {
            MIDIControlChangeEvent e =
                new MIDIControlChangeEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIControlChangeEvent)other;

            ControllerNumber =
                e.ControllerNumber;

            ControllerValue =
                e.ControllerValue;

            backupControllerNumber =
                e.backupControllerNumber;

            backupControllerValue =
                e.backupControllerValue;
        }
    }
}
