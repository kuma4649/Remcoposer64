using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDINoteEventDto : MIDIEventDto
    {
        public int KeyNumber { get; set; }
        public string NoteName { get; set; }
        public int GT { get; set; }
        public int Vel { get; set; }
        public bool Tie { get; set; }
    }
}
