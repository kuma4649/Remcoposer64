using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIChannelPressureEventDto : MIDIEventDto
    {
        public int PressureChannel { get; set; }
        public int PressureValue { get; set; }
    }

}
