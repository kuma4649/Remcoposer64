using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIBankProgramEventDto : MIDIEventDto
    {
        public byte ProgramNumber { get; set; }
        public byte ControlChange { get; set; }
    }
}
