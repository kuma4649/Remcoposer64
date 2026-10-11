using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIRolandDeviceEventDto : MIDIEventDto
    {
        public byte RolandDev_gt { get; set; }
        public byte RolandDev_vel { get; set; }
    }

}
