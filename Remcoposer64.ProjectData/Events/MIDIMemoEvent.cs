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
            Type = MIDIEventType.Memo;
            virVal = " ";//編集モードに入っていることを示すために、virValを空白に設定
        }
        
        public override MIDIEvent Clone()
        {
            var e = new MIDIMemoEvent();

            e.CopyFrom(this);

            return e;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var e =
                (MIDIMemoEvent)other;

            Text = e.Text;
            backupText = e.backupText;
        }
    }
}
