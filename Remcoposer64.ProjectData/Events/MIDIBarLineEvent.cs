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

        public MIDIBarLineEvent()
        {
            Type = MIDIEventType.BarLine;
        }

        public override MIDIEvent Clone()
        {
            MIDIBarLineEvent e =
                new MIDIBarLineEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e = (MIDIBarLineEvent)other;

            MeasureTotalST = e.MeasureTotalST;
            backupMeasureTotalST = e.backupMeasureTotalST;
        }

    }
}
