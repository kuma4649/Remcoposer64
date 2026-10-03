using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public abstract class Event
    {
        public EventManager parent;

        public int Meas { get; set; }
        public int Step { get; set; }
        public int ST { get; set; }
        public static int oST { get; set; } = 0;
        public int STsum { get; set; } = 0;

        public static readonly int ST_MAXLEN = 4;
        public static readonly string ST_SPACE = "    ";
        public static readonly int ST_MINVALUE = 0;
        public static readonly int ST_MAXVALUE = 9999;

        public static readonly int GT_MAXLEN = 4;
        public static readonly string GT_SPACE = "    ";
        public static readonly int GT_MINVALUE = 0;
        public static readonly int GT_MAXVALUE = 9999;

        public static readonly int VEL_MAXLEN = 3;
        public static readonly string VEL_SPACE = "    ";
        public static readonly int VEL_MINVALUE = 0;
        public static readonly int VEL_MAXVALUE = 127;

        public static readonly string GT_VEL_SPACE = "        ";

        // 派生クラスが種類を識別できるようにする
        public abstract EventType Type { get; }

        public string MeasString => editing ? "- :" : $"{Meas + 1}:";
        public string StepString => editing ? "- :" : $"{Step + 1}:";

        public bool editing = false;
        public string editVal = "";
        public string virVal = "*";

        public MIDITieCache[] TieCache { get; set; } = null;

        public Event(EventManager parent)
        {
            this.parent = parent;
        }


        public abstract void Commit(CursorInfo cursorInfo,CommitKeyPattern keyPattern);
        public abstract string GetVal(CursorInfo cursorInfo);
        public abstract Event Clone();

        // ★ クリップボード用の文字列
        public abstract string ToClipboardString();
        public static Event FromClipboard(string type, EventManager parent, int meas, int step, string body)
        {
            return type switch
            {
                "NoteEvent" => NoteEvent.FromClipboardBody(parent, meas, step, body),
                "PolyphonicKeyPressure" => PolyphonicKeyPressureEvent.FromClipboardBody(parent, meas, step, body),
                "ControlChange" => ControlChangeEvent.FromClipboardBody(parent, meas, step, body),
                "ProgramChange" => ProgramChangeEvent.FromClipboardBody(parent, meas, step, body),
                "PitchBend" => PitchBendEvent.FromClipboardBody(parent, meas, step, body),
                "ChannelPressure" => ChannelPressureEvent.FromClipboardBody(parent, meas, step, body),
                "TempoEvent" => TempoEvent.FromClipboardBody(parent, meas, step, body),
                "SysEx" => SysExEvent.FromClipboardBody(parent, meas, step, body),
                "Meta" => MetaEvent.FromClipboardBody(parent, meas, step, body),
                "BarLine" => BarLineEvent.FromClipboardBody(parent, meas, step, body),
                "SameMeas" => SameMeasEvent.FromClipboardBody(parent, meas, step, body),
                _ => null
            };
        }

        public virtual void CopyFrom(Event other)
        {
            this.Meas = other.Meas;
            this.Step = other.Step;
            this.ST = other.ST;
            this.STsum = other.STsum;
        }

        public virtual bool BeginEdit(CursorInfo cursor, Keyboard keyb)
        {
            keyb.inputState = Keyboard.InputState.EditEvent;
            cursor.Position = 0;
            editing = true;
            return true;
        }

        public virtual void EndEdit(Keyboard keyb)
        {
            editing = false;
            keyb.inputState = Keyboard.InputState.None;
        }

        protected void ExecuteUndoRedoCommand(UndoRedoManager.ICommand cmd)
        {
            parent.parent.undoRedoManager.Execute(cmd);
        }

        public virtual bool UpdateActiveEvent(char keyChar, CommitKeyPattern keyPattern, CursorInfo cursor)
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

    }

    public enum MoveNextCursorAction
    {
        NextEvent,      // flg = 0
        PreviousEvent,  // flg = 1
        NextField,      // flg = 2
        StayOnField     // flg = 3
    }

    public enum CommitKeyPattern : int
    {
        OtherKey = -1,
        EnterKey = 0,
        RightKey = 1,
        LeftKey = 2,
        DownKey = 3,
        UpKey = 4,
    }
}
