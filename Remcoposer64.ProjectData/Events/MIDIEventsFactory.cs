using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public static class MIDIEventsFactory
    {
        public static MIDIEvent CreateMIDIEvent(MIDIEventType type)
        {
            switch (type)
            {
                case MIDIEventType.BarLine:
                    return new MIDIBarLineEvent();
                case MIDIEventType.ChannelAfterTouch:
                    return new MIDIChannelPressureEvent();
                case MIDIEventType.ControlChange:
                    return new MIDIControlChangeEvent();
                case MIDIEventType.KeyAfterTouch:
                    return new MIDIPolyphonicKeyPressureEvent();
                case MIDIEventType.Memo:
                    return new MIDIMemoEvent();
                case MIDIEventType.MetaTextEvent:
                    return new MIDIMemoEvent();
                case MIDIEventType.Meta:
                    return new MIDIMetaEvent();
                case MIDIEventType.MetaEndOfTrack:
                    return new MIDIEndEvent();
                case MIDIEventType.MetaTempo:
                    return new MIDITempoEvent();
                case MIDIEventType.NoteOff:
                    return new MIDINoteEvent();
                case MIDIEventType.NoteON:
                    return new MIDINoteEvent();
                case MIDIEventType.PitchBend:
                    return new MIDIPitchBendEvent();
                case MIDIEventType.ProgramChange:
                    return new MIDIProgramChangeEvent();
                case MIDIEventType.SameMeas:
                    return new MIDISameMeasEvent();
                case MIDIEventType.SysExF0:
                    return new MIDISysExEvent();
                case MIDIEventType.SysExF7:
                    return new MIDISysExEvent();
                default:
                    return new MIDIMetaEvent();
                    //Debug.WriteLine(type.ToString());
                    //throw new ArgumentOutOfRangeException();
            }
        }
    }
}
