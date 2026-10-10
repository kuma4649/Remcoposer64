using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIRolandBaseEvent : MIDIEvent
    {
        public byte RolandBase_gt { get; set; }
        public byte RolandBase_vel { get; set; }
        public byte backupRolandBase_gt;
        public byte backupRolandBase_vel;
        public static byte oRolandBase_gt;
        public static byte oRolandBase_vel;

        public MIDIRolandBaseEvent()
        {
            Type = MIDIEventType.RolandBase;
        }

        public override MIDIEvent Clone()
        {
            return new MIDIRolandBaseEvent()
            {
                Type = MIDIEventType.RolandBase,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                RolandBase_gt = this.RolandBase_gt,
                RolandBase_vel = this.RolandBase_vel,
            };

        }
    }
}
