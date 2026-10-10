using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Windows.Forms.AxHost;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class UIRolandBaseEvent : UIEvent
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

        public UIRolandBaseEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDIRolandBaseEvent be = (MIDIRolandBaseEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            FBColor fbC = aryFb[3];

            drawUtil.DrawCell("Rol Base", ref pos, ref x, COL_NOTENAME, aryFb[0]);

            x += 4;

            if (state == RowState.Cursor && cursor.Position < 3)
                drawUtil.DrawCell(be.ST.ToString(), ref pos, ref x, COL_ST, fbC);
            else
                drawUtil.DrawCell(be.ST != 0 ? be.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

            if (state == RowState.Cursor && cursor.Position == 3)
                drawUtil.DrawCell(be.RolandBase_gt.ToString(), ref pos, ref x, COL_GT, fbC);
            else
                drawUtil.DrawCell(be.RolandBase_gt.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

            if (state == RowState.Cursor && cursor.Position == 4)
                drawUtil.DrawCell(be.RolandBase_vel.ToString(), ref pos, ref x, COL_VEL, fbC);
            else
                drawUtil.DrawCell(be.RolandBase_vel.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                         : (state == RowState.Normal ? MeasStepNormal
                         : MeasStepSelect);
        }
    }
}
