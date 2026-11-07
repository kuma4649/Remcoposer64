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
            var e =
                new MIDIPortPrefixEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIPortPrefixEvent)other;

            PortNumber =
                e.PortNumber;

            backupPortNumber =
                e.backupPortNumber;
        }

    }
}
