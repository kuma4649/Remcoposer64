using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    public enum MIDISpEventType : byte
    {
        UserExclusive1 = 0x90,
        UserExclusive2 = 0x91,
        UserExclusive3 = 0x92,
        UserExclusive4 = 0x93,
        UserExclusive5 = 0x94,
        UserExclusive6 = 0x95,
        UserExclusive7 = 0x96,
        UserExclusive8 = 0x97,
        ChExclusive = 0x98,
        OutSideProc = 0x99,
        BankProgram = 0xE2,
        KeyScan = 0xE5,
        MIDICh = 0xE6,
        TempoChange = 0xE7,
        RolandBase = 0xDD,
        RolandPara = 0xDE,
        RolandDevice = 0xDF,
        KeyChange = 0xF5,
        Comment = 0xF6,
        LoopEnd = 0xF8,
        LoopStart = 0xF9,
        SameMeasure = 0xFC,
        MeasureEnd = 0xFD,
        EndOfTrack = 0xFE
    }
}
