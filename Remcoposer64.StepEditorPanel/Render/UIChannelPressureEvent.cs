using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIChannelPressureEvent : UIEvent
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


        public UIChannelPressureEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDIChannelPressureEvent cp = (MIDIChannelPressureEvent)ev;

            FBColor[] aryFb = state == RowState.Cursor
                ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : (state == RowState.Normal
                ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            if (cp.editing)
            {
                FBColor fbC = aryFb[1];
                string val = cp.editVal;

                if (string.IsNullOrWhiteSpace(cp.editVal))
                {
                    val = cp.virVal;
                    fbC = aryFb[2];
                }

                // After C. 
                drawUtil.DrawCell("After C.", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // K# (空欄)
                x += 4;

                // ST
                if (cursor.Position < 3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(cp.ST.ToString(), ref pos, ref x, COL_ST, aryFb[0]);

                // PressureValue (GT)
                if (cursor.Position > 2)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_GT, fbC);
                else
                    drawUtil.DrawCell(cp.PressureValue.ToString(), ref pos, ref x, COL_GT, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // After C. (空欄)
                drawUtil.DrawCell("After C.", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // K# (空欄)
                x += 4;

                // ST
                if (state == RowState.Cursor && cursor.Position < 3)
                    drawUtil.DrawCell(cp.ST.ToString(), ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(cp.ST != 0 ? cp.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

                // PressureValue (GT)
                if (state == RowState.Cursor && cursor.Position > 2)
                    drawUtil.DrawCell(cp.PressureValue.ToString(), ref pos, ref x, COL_GT, fbC);
                else
                    drawUtil.DrawCell(cp.PressureValue.ToString(), ref pos, ref x, COL_GT, aryFb[0]);
            }
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                         : (state == RowState.Normal ? MeasStepNormal
                         : MeasStepSelect);
        }
    }
}
