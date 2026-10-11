using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIPolyphonicKeyPressureEventDto : MIDIEventDto
    {
        public int KeyNumber { get; set; }
        public int PressureValue { get; set; }
    }

}
