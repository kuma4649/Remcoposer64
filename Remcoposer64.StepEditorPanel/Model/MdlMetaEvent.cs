using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public class MdlMetaEvent : MdlEvent
    {
        public override MIDIEventType Type => MIDIEventType.Meta;

        public byte MetaType { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public MdlMetaEvent(EventManager parent) : base(parent)
        {
        }

        public override void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPatten)
        {
            me.editing = false;
        }

        public override string GetVal(MIDIEvent me,CursorInfo cursorInfo)
        {
            return MetaType.ToString();
        }

        public override string ToClipboardString(MIDIEvent me)
        {
            MIDIMetaEvent mme = (MIDIMetaEvent)me;
            string hex = BitConverter.ToString(Data).Replace("-", " ");
            return $"{mme.Meas + 1},{mme.Step + 1}: Meta: {mme.MetaType},{hex}";
        }

        public static MIDIEvent FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',', 2);
            if (vals.Length < 2) return null;

            if (!byte.TryParse(vals[0], out byte metaType)) { WinApi.Beep(); return null; }
            var bytes = ParseHexBytes(vals[1]);

            return new MIDIMetaEvent()
            {
                Meas = meas,
                Step = step,
                MetaType = metaType,
                Data = bytes
            };
        }

    }
}
