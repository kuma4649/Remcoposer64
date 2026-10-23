using System.Diagnostics.Metrics;

namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIInformationDto
    {
        public string FileFullName { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Writer { get; set; }
        public string Composer { get; set; }
        public string Arranger { get; set; }
        public string Copyright { get; set; }
        public string Memo { get; set; }
        public int Key { get; set; }
        public int PlayBIAS { get; set; }
        public string ControlFileCM6 { get; set; }
        public string ControlFileGSD { get; set; }
        public string ControlFileGSD2 { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int TimeBase { get; set; }
        public int Tempo { get; set; }
        public int BeatDen { get; set; }
        public int BeatMol { get; set; }

        public List<int> Marker { get; set; } = new List<int>();

        public int? tStartIndex { get; set; }
        public int? tEndIndex { get; set; }
        public int tNumber { get; set; }
        public int tCounter { get; set; }

        public int MaxLengthMIDIOut { get; set; }
    }
}
