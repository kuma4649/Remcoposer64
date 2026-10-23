using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIMarkerEvent : MIDIEvent
    {
        public string Text { get; set; } = "";
        public string backupText;
        public static string oldText;

        public MIDIMarkerEvent()
        {
            Type = MIDIEventType.MetaMarker;
            virVal = " ";//編集モードに入っていることを示すために、virValを空白に設定
        }

        public override MIDIEvent Clone()
        {
            return new MIDIMemoEvent()
            {
                Type = MIDIEventType.MetaMarker,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                Text = this.Text
            };
        }

    }
}
