using System.Xml.Serialization;

namespace Remcoposer64.ProjectData.DTO
{
    [XmlInclude(typeof(MIDINoteEventDto))]
    [XmlInclude(typeof(MIDIControlChangeEventDto))]
    [XmlInclude(typeof(MIDIProgramChangeEventDto))]
    [XmlInclude(typeof(MIDIPitchBendEventDto))]
    [XmlInclude(typeof(MIDIPolyphonicKeyPressureEventDto))]
    [XmlInclude(typeof(MIDIChannelPressureEventDto))]
    [XmlInclude(typeof(MIDITempoEventDto))]
    [XmlInclude(typeof(MIDIMemoEventDto))]
    [XmlInclude(typeof(MIDIMetaEventDto))]
    [XmlInclude(typeof(MIDIBarLineEventDto))]
    [XmlInclude(typeof(MIDIEndEventDto))]
    [XmlInclude(typeof(MIDIRolandBaseEventDto))]
    [XmlInclude(typeof(MIDIRolandDeviceEventDto))]
    [XmlInclude(typeof(MIDIRolandParaEventDto))]
    [XmlInclude(typeof(MIDISysExEventDto))]
    [XmlInclude(typeof(MIDISameMeasEventDto))]
    public abstract class MIDIEventDto
    {
        public MIDIEventType Type { get; set; }
        public int Meas { get; set; }
        public int Step { get; set; }
        public int ST { get; set; }

        public byte[] MIDIMessage { get; set; }
        public byte[][] MIDIMessageLst { get; set; }
    }

}
