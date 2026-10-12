using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class UILoopEndEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.LightGreen, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);

        public UILoopEndEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDILoopEndEvent be = (MIDILoopEndEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            FBColor fbC = aryFb[3];

            drawUtil.DrawCell(" ] ", ref pos, ref x, COL_NOTENAME, aryFb[0]);

            x += 4;

            if (state == RowState.Cursor && cursor.Position == 3)
                drawUtil.DrawCell(be.LoopCounter.ToString(), ref pos, ref x, COL_GT, fbC);
            else
                drawUtil.DrawCell(be.LoopCounter.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                         : (state == RowState.Normal ? MeasStepNormal
                         : MeasStepSelect);
        }
    }
}
