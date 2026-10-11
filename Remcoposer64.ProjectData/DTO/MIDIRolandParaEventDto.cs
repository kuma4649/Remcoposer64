using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIRolandParaEventDto : MIDIEventDto
    {
        public byte RolandPara_gt { get; set; }
        public byte RolandPara_vel { get; set; }
    }

}
