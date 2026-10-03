using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIMetaEvent : MIDIEvent
    {
        public byte MetaType { get; set; }
        public byte backupMetaType;
        public static byte oldMetaType;
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public byte[] backupData;
        public static byte[] oldData;

        public MIDIMetaEvent()
        {
            Type = MIDIEventType.Meta;
        }   

        public override MIDIEvent Clone()
        {
            return new MIDIMetaEvent()
            {
                Type = MIDIEventType.Meta,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                MetaType = this.MetaType,
                Data = this.Data
            };
        }

    }
}
