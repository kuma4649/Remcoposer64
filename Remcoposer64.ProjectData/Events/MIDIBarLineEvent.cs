using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIBarLineEvent : MIDIEvent
    {
        public int MeasureTotalST { get; set; }
        public int backupMeasureTotalST;
        public static int oldMeasureTotalST;

        public override MIDIEvent Clone()
        {
            return new MIDIBarLineEvent()
            {
                Type = MIDIEventType.BarLine,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                MeasureTotalST = this.MeasureTotalST
            };
        }

    }
}
