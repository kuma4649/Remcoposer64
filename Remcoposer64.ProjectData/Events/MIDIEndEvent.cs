using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIEndEvent : MIDIEvent
    {
        public MIDIEndEvent()
        {
            Type = MIDIEventType.MetaEndOfTrack;
        }

        public override MIDIEvent Clone()
        {
            return new MIDIEndEvent()
            {
                Type = MIDIEventType.MetaEndOfTrack,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
            };
        }
    }
}
