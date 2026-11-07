using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIRolandParaEvent : MIDIEvent
    {
        public byte RolandPara_gt { get; set; }
        public byte RolandPara_vel { get; set; }
        public byte backupRolandPara_gt;
        public byte backupRolandPara_vel;
        public static byte oRolandPara_gt;
        public static byte oRolandPara_vel;

        public MIDIRolandParaEvent()
        {
            Type = MIDIEventType.RolandPara;
        }

        public override MIDIEvent Clone()
        {
            var e =
                new MIDIRolandParaEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIRolandParaEvent)other;

            RolandPara_gt =
                e.RolandPara_gt;

            RolandPara_vel =
                e.RolandPara_vel;

            backupRolandPara_gt =
                e.backupRolandPara_gt;

            backupRolandPara_vel =
                e.backupRolandPara_vel;
        }
    }
}
