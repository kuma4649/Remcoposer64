using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDISysExEventDto : MIDIEventDto
    {
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public byte GT { get; set; }
        public byte VEL { get; set; }

    }

}
