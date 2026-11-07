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
            MIDIBankProgramEvent e =
                new MIDIBankProgramEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e = (MIDIBankProgramEvent)other;

            ProgramNumber = e.ProgramNumber;
            ControlChange = e.ControlChange;

            backupProgramNumber = e.backupProgramNumber;
            backupControlChange = e.backupControlChange;
        }
    }
}
