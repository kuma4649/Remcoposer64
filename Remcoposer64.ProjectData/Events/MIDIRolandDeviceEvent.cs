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
            return new MIDIRolandDeviceEvent()
            {
                Type = MIDIEventType.RolandDevice,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                RolandDev_gt = this.RolandDev_gt,
                RolandDev_vel = this.RolandDev_vel,
            };
        }
    }
}
