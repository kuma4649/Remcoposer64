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
            return new MIDIProgramChangeEvent()
            {
                Type = MIDIEventType.ProgramChange,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                ProgramNumber = this.ProgramNumber,

            };
        }

    }
}
