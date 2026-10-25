using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class UIPortPrefixEvent : UIEvent
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


        private static readonly FBColor MemoColor = new(System.Drawing.Color.White, System.Drawing.Color.DarkBlue);

        public UIPortPrefixEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }
        
        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                         : (state == RowState.Normal ? MeasStepNormal
                         : MeasStepSelect);
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            var pfe = (MIDIPortPrefixEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
             : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
             : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            FBColor fbC = aryFb[3];

            drawUtil.DrawCell("PortPre", ref pos, ref x, COL_NOTENAME, aryFb[0]);

            x += 4;

            // ST
            if (state == RowState.Cursor && cursor.Position < 3)
                drawUtil.DrawCell(pfe.ST.ToString(), ref pos, ref x, COL_ST, fbC);
            else
                drawUtil.DrawCell(pfe.ST != 0 ? pfe.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

            // Port number(GT)
            if (state == RowState.Cursor && cursor.Position == 3)
                drawUtil.DrawCell(pfe.PortNumber.ToString(), ref pos, ref x, COL_GT, fbC);
            else
                drawUtil.DrawCell(pfe.PortNumber.ToString(), ref pos, ref x, COL_GT, aryFb[0]);


        }
    }
}
