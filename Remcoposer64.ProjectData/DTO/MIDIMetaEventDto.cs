using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIMetaEventDto : MIDIEventDto
    {
        public byte MetaType { get; set; }
        public byte[] Data { get; set; }
    }

}
