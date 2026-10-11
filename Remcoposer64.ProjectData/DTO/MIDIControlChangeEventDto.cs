using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIControlChangeEventDto : MIDIEventDto
    {
        public int ControllerNumber { get; set; }
        public int ControllerValue { get; set; }
    }

}
