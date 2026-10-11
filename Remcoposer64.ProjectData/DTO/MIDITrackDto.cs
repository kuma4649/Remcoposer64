namespace Remcoposer64.ProjectData.DTO
{
    public class MIDITrackDto
    {
        public int Number { get; set; }
        public int TrackNumber { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public string Comment { get; set; }

        public int OutDevice { get; set; }
        public int? OutChannel { get; set; }
        public int InDevice { get; set; }
        public int? InChannel { get; set; }

        public bool Solo { get; set; }
        public bool Mute { get; set; }

        public int? Key { get; set; }
        public int St { get; set; }

        public List<MIDIPartDto> Parts { get; set; } = new List<MIDIPartDto>();
        public List<MIDIRythmDto> Rythm { get; set; } = new List<MIDIRythmDto>();
    }
}
