using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIUsrExEvent : MIDIEvent
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public byte[] backupData;
        public static byte[] oData;

        public string DataString => BitConverter.ToString(Data).Replace("-", " ");
        public string Name { get; set; } = "";
        public byte GT { get; set; } = 0;
        public byte backupGT;
        public static byte oGT;
        public byte VEL { get; set; } = 0;
        public byte backupVEL;
        public static byte oVEL;

        public string backupName;
        public static string oName;

        public MIDIUsrExEvent()
        {
            Type = MIDIEventType.UserExclusive1;
        }

        public override MIDIEvent Clone()
        {
            return new MIDISysExEvent()
            {
                Type = MIDIEventType.UserExclusive1,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                GT = this.GT,
                VEL = this.VEL,

                Data = this.Data,
                Name = this.Name
            };
        }

    }
}
