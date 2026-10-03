using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;

namespace Remcoposer64.StepEditorPanelControl.Model
{
    public abstract class MdlEvent
    {
        public EventManager parent;

        public static readonly int ST_MAXLEN = 4;
        public static readonly string ST_SPACE = "    ";
        public static readonly int ST_MINVALUE = 0;
        public static readonly int ST_MAXVALUE = 9999;

        // 派生クラスが種類を識別できるようにする
        public abstract MIDIEventType Type { get; }

        //public string MeasString(int Meas)
        //{
        //    return editing ? "- :" : $"{Meas + 1}:";
        //}

        //public string StepString(int Step)
        //{
        //    return editing ? "- :" : $"{Step + 1}:";
        //}


        public MIDITieCache[] TieCache { get; set; } = null;

        public MdlEvent(EventManager parent)
        {
            this.parent = parent;
        }


        public abstract void Commit(MIDIEvent me, CursorInfo cursorInfo, CommitKeyPattern keyPattern);
        public abstract string GetVal(MIDIEvent me, CursorInfo cursorInfo);

        // ★ クリップボード用の文字列
        public abstract string ToClipboardString(MIDIEvent me);
        public static MIDIEvent FromClipboard(string type, EventManager parent, int meas, int step, string body)
        {
            return type switch
            {
                "ChannelPressure" => MdlChannelPressureEvent.FromClipboardBody(parent, meas, step, body),
                "ControlChange" => MdlControlChangeEvent.FromClipboardBody(parent, meas, step, body),
                "Memo" => MdlMemoEvent.FromClipboardBody(parent, meas, step, body),
                "Meta" => MdlMetaEvent.FromClipboardBody(parent, meas, step, body),
                "NoteEvent" => MdlNoteEvent.FromClipboardBody(parent, meas, step, body),
                "PolyphonicKeyPressure" => MdlPolyphonicKeyPressureEvent.FromClipboardBody(parent, meas, step, body),
                "ProgramChange" => MdlProgramChangeEvent.FromClipboardBody(parent, meas, step, body),
                "PitchBend" => MdlPitchBendEvent.FromClipboardBody(parent, meas, step, body),
                "TempoEvent" => MdlTempoEvent.FromClipboardBody(parent, meas, step, body),
                "SysEx" => MdlSysExEvent.FromClipboardBody(parent, meas, step, body),
                "BarLine" => MdlBarLineEvent.FromClipboardBody(parent, meas, step, body),
                "SameMeas" => MdlSameMeasEvent.FromClipboardBody(parent, meas, step, body),
                _ => null
            };
        }


        public virtual bool BeginEdit(MIDIEvent me, CursorInfo cursor, Keyboard keyb)
        {
            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = 0;
            me.editing = true;
            return true;
        }

        public virtual void EndEdit(MIDIEvent me, Keyboard keyb)
        {
            me.editing = false;
            keyb.inputState = Keyboard.InputState.None;
        }

        protected void ExecuteUndoRedoCommand(UndoRedoManager.ICommand cmd)
        {
            parent.parent.undoRedoManager.Execute(cmd);
        }

        public virtual bool UpdateActiveEvent(MIDIEvent me,char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
        {
            // デフォルトは編集終了しない
            return false;
        }

        public virtual int AdjustCursorPosition(int rawPos)
        {
            return rawPos;
        }

        public static byte[] ParseHexBytes(string hex)
        {
            var parts = hex.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var bytes = new byte[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                bytes[i] = Convert.ToByte(parts[i], 16);

            return bytes;
        }

        public virtual void CopyFrom(MIDIEvent srcMe, MIDIEvent otherMe, MdlEvent otherMdle)
        {
            // 編集状態系
            srcMe.editing = otherMe.editing;
            srcMe.editVal = otherMe.editVal;
            srcMe.virVal = otherMe.virVal;
        }

    }
}
