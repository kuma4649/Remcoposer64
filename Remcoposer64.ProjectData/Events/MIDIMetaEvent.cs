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
            MIDIMetaEvent e =
                new MIDIMetaEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIMetaEvent)other;

            MetaType = e.MetaType;
            backupMetaType = e.backupMetaType;

            Data =
                e.Data != null
                ? (byte[])e.Data.Clone()
                : Array.Empty<byte>();

            backupData =
                e.backupData != null
                ? (byte[])e.backupData.Clone()
                : null;
        }
    }
}
