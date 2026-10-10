namespace Remcoposer64.ProjectData.Events
{
    [Serializable]
    public abstract class MIDIEvent
    {
        public MIDIEventType Type { get; set; }

        public int Meas { get; set; }
        public int Step { get; set; }

        public int ST { get; set; }
        public int backupST;
        public static int oST;

        //public int GT { get; set; }
        public byte[] MIDIMessage { get; set; }
        public byte[][] MIDIMessageLst { get; set; }

        // 制御向け
        public MIDITieCache[] TieCache { get; set; } = null;
        public int STsum { get; set; } = 0;

        // 編集制御向け
        public bool editing = false;
        public string editVal = "";
        public string virVal = "*";
        public string MeasString => editing ? "- :" : $"{Meas + 1}:";
        public string StepString => editing ? "- :" : $"{Step + 1}:";

        public bool Played { get; set; } = false;

        public MIDIEvent()
        {
            Type = MIDIEventType.None;
            Meas = 0;
            Step = 0;
            ST = 0;
            //GT = 0;
            MIDIMessage = null;
        }

        public abstract MIDIEvent Clone();

        public virtual void CopyFrom(MIDIEvent other)
        {
            Meas = other.Meas;
            Step = other.Step;
            ST = other.ST;
        }

    }
}
