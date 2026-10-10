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
            return new MIDIRolandParaEvent()
            {
                Type = MIDIEventType.RolandPara,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                RolandPara_gt = this.RolandPara_gt,
                RolandPara_vel = this.RolandPara_vel,
            };
        }
    }
}
