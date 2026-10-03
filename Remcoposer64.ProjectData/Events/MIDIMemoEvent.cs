using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDIMemoEvent : MIDIEvent
    {
        public string Text { get; set; } = "";
        public string backupText;
        public static string oldText;

        public MIDIMemoEvent()
        {
            virVal = " ";//編集モードに入っていることを示すために、virValを空白に設定
        }

        public override MIDIEvent Clone()
        {
            return new MIDIMemoEvent()
            {
                Type = MIDIEventType.Memo,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                Text = this.Text
            };
        }

    }
}
