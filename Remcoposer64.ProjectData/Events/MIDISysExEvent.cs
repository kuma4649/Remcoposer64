using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDISysExEvent : MIDIEvent
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public byte[] backupData;
        public static byte[] oData;

        public string DataString => BitConverter.ToString(Data).Replace("-", " ");
        public string Name { get; set; } = "";
        public string backupName;
        public static string oName;

        public override MIDIEvent Clone()
        {
            return new MIDISysExEvent()
            {
                Type = MIDIEventType.SysExF0,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                Data = this.Data,
                Name = this.Name
            };
        }

    }
}
