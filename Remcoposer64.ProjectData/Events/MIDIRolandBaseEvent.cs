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
            var e =
                new MIDIRolandBaseEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIRolandBaseEvent)other;

            RolandBase_gt =
                e.RolandBase_gt;

            RolandBase_vel =
                e.RolandBase_vel;

            backupRolandBase_gt =
                e.backupRolandBase_gt;

            backupRolandBase_vel =
                e.backupRolandBase_vel;
        }
    }
}
