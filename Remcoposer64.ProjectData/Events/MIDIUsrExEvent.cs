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
            var e =
                new MIDIUsrExEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIUsrExEvent)other;

            GT = e.GT;
            backupGT = e.backupGT;

            VEL = e.VEL;
            backupVEL = e.backupVEL;

            Name = e.Name;
            backupName = e.backupName;

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
