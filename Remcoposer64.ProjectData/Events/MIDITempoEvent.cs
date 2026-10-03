using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDITempoEvent : MIDIEvent
    {
        public static readonly int TEMPOMin = 0;
        public static readonly int TEMPOMax = 99999;

        public int TempoValue { get; set; } = 120;
        public int backupTempoValue;
        public static int oTempoValue;

        public override MIDIEvent Clone()
        {
            return new MIDITempoEvent()
            {
                Type = MIDIEventType.MetaTempo,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                TempoValue = this.TempoValue,

            };
        }

        public bool IsValidDisplayValue(int v)
        {
            return v >= TEMPOMin && v <= TEMPOMax;
        }

    }
}
