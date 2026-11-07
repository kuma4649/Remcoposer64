using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDILoopEndEvent : MIDIEvent
    {
        public int LoopCounter { get; set; } = 0;
        public int backupLoopCounter;
        public static int oLoopCounter;


        public MIDILoopEndEvent()
        {
            Type = MIDIEventType.LoopEnd;
        }

        public override MIDIEvent Clone()
        {
            MIDILoopEndEvent e =
                new MIDILoopEndEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDILoopEndEvent)other;

            LoopCounter =
                e.LoopCounter;

            backupLoopCounter =
                e.backupLoopCounter;
        }
    }
}
