using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIPitchBendEvent : MIDIEvent
    {
        public static readonly int RawMin = 0;
        public static readonly int RawMax = 16383;

        public static readonly int CenterMin = -8192;
        public static readonly int CenterMax = 8191;

        public static bool PitchBendCenteredMode = true;

        public int PitchValue { get; set; }
        public int backupPitchValue;
        public static int oPitchValue;

        public int GetDisplayValue()
        {
            return PitchBendCenteredMode
                ? PitchValue - 8192
                : PitchValue;
        }
        public int ToRawValue(int displayValue)
        {
            return PitchBendCenteredMode
                ? displayValue + 8192
                : displayValue;
        }

        public bool IsValidDisplayValue(int v)
        {
            if (PitchBendCenteredMode)
                return v >= CenterMin && v <= CenterMax;
            else
                return v >= RawMin && v <= RawMax;
        }

        public override MIDIEvent Clone()
        {
            return new MIDIPitchBendEvent()
            {
                Type = MIDIEventType.PitchBend,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                PitchValue = this.PitchValue,

            };
        }
    }
}
