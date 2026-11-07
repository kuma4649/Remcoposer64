using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIRolandDeviceEvent : MIDIEvent
    {
        public byte RolandDev_gt { get; 
            set; 
        }
        public byte RolandDev_vel { get; set; }
        public byte backupRolandDev_gt;
        public byte backupRolandDev_vel;
        public static byte oRolandDev_gt;
        public static byte oRolandDev_vel;

        public MIDIRolandDeviceEvent()
        {
            Type = MIDIEventType.RolandDevice;
        }

        public override MIDIEvent Clone()
        {
            var e =
                new MIDIRolandDeviceEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIRolandDeviceEvent)other;

            RolandDev_gt =
                e.RolandDev_gt;

            RolandDev_vel =
                e.RolandDev_vel;

            backupRolandDev_gt =
                e.backupRolandDev_gt;

            backupRolandDev_vel =
                e.backupRolandDev_vel;
        }
    }
}
