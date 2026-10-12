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
            return new MIDILoopStartEvent()
            {
                Type = MIDIEventType.LoopStart,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
            };
        }
    }
}
