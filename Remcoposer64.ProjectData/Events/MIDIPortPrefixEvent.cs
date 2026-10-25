using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIPortPrefixEvent : MIDIEvent
    {
        public int PortNumber { get; set; } = 0;
        public int backupPortNumber;
        public static int oldPortNumber;

        public MIDIPortPrefixEvent()
        {
            Type = MIDIEventType.MetaPortPrefix;
            virVal = " ";//編集モードに入っていることを示すために、virValを空白に設定
        }

        public override MIDIEvent Clone()
        {
            return new MIDIPortPrefixEvent()
            {
                Type = MIDIEventType.MetaMarker,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                PortNumber = this.PortNumber
            };
        }

    }
}
