using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIBankProgramEvent : MIDIEvent
    {
        public byte ProgramNumber { get; set; }
        public byte ControlChange { get; set; }
        public byte backupProgramNumber;
        public byte backupControlChange;
        public static byte oProgramNumber;
        public static byte oControlChange;


        public MIDIBankProgramEvent()
        {
            Type = MIDIEventType.BankProgram;

        }

        public override MIDIEvent Clone()
        {
            return new MIDIBankProgramEvent()
            {
                Type = MIDIEventType.BankProgram,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                ProgramNumber = this.ProgramNumber,
                ControlChange = this.ControlChange,
            };
        }
    }
}
