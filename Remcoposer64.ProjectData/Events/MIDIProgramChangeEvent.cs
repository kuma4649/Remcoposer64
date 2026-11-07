using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIProgramChangeEvent : MIDIEvent
    {
        public static readonly int ProgramNumber_MINVALUE = 0;
        public static readonly int ProgramNumber_MAXVALUE = 127;

        public int ProgramNumber { get; set; }
        public int backupProgramNumber;
        public static int oProgramNumber;

        public MIDIProgramChangeEvent()
        {
            Type = MIDIEventType.ProgramChange;
        }

        public override MIDIEvent Clone()
        {
            var e =
                new MIDIProgramChangeEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIProgramChangeEvent)other;

            ProgramNumber =
                e.ProgramNumber;

            backupProgramNumber =
                e.backupProgramNumber;
        }

    }
}
