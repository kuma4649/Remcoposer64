using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDITempoEventDto : MIDIEventDto
    {
        public int TempoValue { get; set; }
        public bool RelativeTempoChange { get; set; }
        public int RelativeTempoChangeValue { get; set; }
    }

}
