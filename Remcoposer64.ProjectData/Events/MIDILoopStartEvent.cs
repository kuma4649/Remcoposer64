using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDILoopStartEvent : MIDIEvent
    {
        public MIDILoopStartEvent()
        {
            Type = MIDIEventType.LoopStart;
        }

        public override MIDIEvent Clone()
        {
            MIDILoopStartEvent e =
                new MIDILoopStartEvent();

            e.CopyFrom(this);

            return e;
        }
    }
}
