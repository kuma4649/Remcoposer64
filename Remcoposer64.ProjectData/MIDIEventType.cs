using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    public enum MIDIEventType : int
    {
        None = -1,
        MetaSeqNumber = 0x00,
        MetaTextEvent = 0x01,
        MetaCopyrightNotice = 0x02,
        MetaTrackName = 0x03,
        MetaInstrumentName = 0x04,
        MetaLyric = 0x05,
        MetaMarker = 0x06,
        MetaCuePoint = 0x07,
        MetaProgramName = 0x08,
        MetaDeviceName = 0x09,
        MetaChannelPrefix = 0x20,
        MetaPortPrefix = 0x21,
        MetaEndOfTrack = 0x2F,
        MetaTempo = 0x51,
        MetaSMPTEOffset = 0x54,
        MetaTimeSignature = 0x58,
        MetaKeySignature = 0x59,
        MetaSequencerSpecific = 0x7F,
        NoteOff = 0x80,
        NoteON = 0x90,
        KeyAfterTouch = 0xA0,
        ControlChange = 0xB0,
        ProgramChange = 0xC0,
        ChannelAfterTouch = 0xD0,
        PitchBend = 0xE0,
        SysExF0 = 0xF0,
        SysExF7 = 0xF7,

        BarLine = 0x100,
        //End = 0x101,
        Memo = 0x102,
        Meta = 0x103,
        SameMeas = 0x104
    }
}
