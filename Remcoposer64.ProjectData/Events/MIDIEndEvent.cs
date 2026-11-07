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
            MIDIEndEvent e =
                new MIDIEndEvent();

            e.CopyFrom(this);

            return e;
        }
    }
}
