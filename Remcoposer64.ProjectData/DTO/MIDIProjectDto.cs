using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIProjectDto
    {
        public MIDIInformationDto Information { get; set; }
        public List<MIDITrackDto> Tracks { get; set; }
        public List<MIDIUserExclusiveDto> UserExclusive { get; set; }
        public List<MIDIRythmDto> Rythm { get; set; }

    }
}
