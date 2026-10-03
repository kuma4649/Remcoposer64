
namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class MetaEvent : Event
    {
        public override EventType Type => EventType.Meta;

        public byte MetaType { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public MetaEvent(EventManager parent) : base(parent)
        {
        }

        public override Event Clone()
        {
            return new MetaEvent(parent)
            {
                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,
                STsum = this.STsum,

                MetaType = this.MetaType,
                Data = (byte[])this.Data.Clone(),

                editing = this.editing,
                editVal = this.editVal,
                virVal = this.virVal
            };
        }

        public override void Commit(CursorInfo cursorInfo, CommitKeyPattern keyPatten)
        {
            editing = false;
        }

        public override string GetVal(CursorInfo cursorInfo)
        {
            return MetaType.ToString();
        }

        public override string ToClipboardString()
        {
            string hex = BitConverter.ToString(Data).Replace("-", " ");
            return $"{Meas + 1},{Step + 1}: Meta: {MetaType},{hex}";
        }

        public static Event FromClipboardBody(EventManager parent, int meas, int step, string body)
        {
            var vals = body.Split(',', 2);
            if (vals.Length < 2) return null;

            if (!byte.TryParse(vals[0], out byte metaType)) { WinApi.Beep(); return null; }
            var bytes = ParseHexBytes(vals[1]);

            return new MetaEvent(parent)
            {
                Meas = meas,
                Step = step,
                MetaType = metaType,
                Data = bytes
            };
        }

    }
}
