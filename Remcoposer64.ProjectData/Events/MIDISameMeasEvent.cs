using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDISameMeasEvent : MIDIEvent
    {
        public int RepeatMeas { get; set; } = 0;
        public int backupRepeatMeas;
        public static int oRepeatMeas;
        public LinkedListNode<MIDIEvent> RepeatMeasBeforeNode { get; set; }
        public LinkedListNode<MIDIEvent> backupRepeatMeasBeforeNode;
        public static LinkedListNode<MIDIEvent> oRepeatMeasBeforeNode;

        public override MIDIEvent Clone()
        {
            return new MIDISameMeasEvent()
            {
                Type = MIDIEventType.SameMeas,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                RepeatMeas = this.RepeatMeas
            };
        }
    }
}
