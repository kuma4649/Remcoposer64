using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIMarkerEvent : MIDIEvent
    {
        public string Text { get; set; } = "";
        public string backupText;
        public static string oldText;

        public MIDIMarkerEvent()
        {
            Type = MIDIEventType.MetaMarker;
            virVal = " ";
        }

        public override MIDIEvent Clone()
        {
            var e = new MIDIMarkerEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIMarkerEvent)other;

            Text = e.Text;
            backupText = e.backupText;
        }
    }
}
