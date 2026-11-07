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

        public MIDISameMeasEvent()
        {
            Type = MIDIEventType.SameMeas;
        }

        public override MIDIEvent Clone()
        {
            var e =
                new MIDISameMeasEvent();

            e.CopyFrom(this);

            return e;
        }
        
        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDISameMeasEvent)other;

            RepeatMeas = e.RepeatMeas;
            backupRepeatMeas = e.backupRepeatMeas;

            RepeatMeasBeforeNode = null;
            backupRepeatMeasBeforeNode = null;
        }

    }
}
