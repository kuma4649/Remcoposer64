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
            return new MIDILoopEndEvent()
            {
                Type = MIDIEventType.LoopEnd,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                LoopCounter = this.LoopCounter,
            };
        }
    }
}
