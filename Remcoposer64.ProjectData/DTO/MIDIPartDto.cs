namespace Remcoposer64.ProjectData.DTO
{
    public class MIDIPartDto
    {
        public int Number { get; set; }
        public string Name { get; set; }
        public long StartTick { get; set; }
        public long LengthTick { get; set; }

        public List<MIDIEventDto> Events { get; set; } = new List<MIDIEventDto>();
    }
}
