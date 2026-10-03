using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Events
{
    public class EventDto
    {
        public MIDIEventType Type { get; set; }
        public int Meas { get; set; }
        public int Step { get; set; }
        public int ST { get; set; }

        // Note
        public string NoteName { get; set; }
        public int KeyNumber { get; set; }
        public int GT { get; set; }
        public int Vel { get; set; }

        // SysEx
        public byte[] Data { get; set; }
        public string Name { get; set; }

        // SameMeas
        public int RepeatMeas { get; set; }

        // BarLine
        public int MeasureTotalST { get; set; }

        // Memo
        public string Text { get; set; }

        // ProgramChange
        public int ProgramNumber { get; set; }

        // ControlChange
        public int ControllerNumber { get; set; }
        public int ControlChangeValue { get; set; }

        // PitchBend
        public int PitchBendValue { get; set; }

        // Tempo
        public int TempoValue { get; set; }

        // CPressureValue
        public int CPressureValue { get; set; }

        // PPressureValue
        public int PPressureValue { get; set; }
    }
}
