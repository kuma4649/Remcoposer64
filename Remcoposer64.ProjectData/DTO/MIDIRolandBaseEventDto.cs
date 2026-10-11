using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIRolandBaseEventDto : MIDIEventDto
    {
        public byte RolandBase_gt { get; set; }
        public byte RolandBase_vel { get; set; }
    }

}
