using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UITempoEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.Khaki, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);



        public UITempoEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDITempoEvent te = (MIDITempoEvent)ev;

            FBColor[] aryFb = state == RowState.Cursor
                ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : (state == RowState.Normal
                ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            if (te.editing)
            {
                FBColor fbC = aryFb[1];
                string val = te.editVal;

                if (string.IsNullOrWhiteSpace(te.editVal))
                {
                    val = te.virVal;
                    fbC = aryFb[2];
                }

                // Tempo
                drawUtil.DrawCell("Tempo", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // K# (空欄)
                x += 4;

                // ST
                if (cursor.Position < 3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(te.ST.ToString(), ref pos, ref x, COL_ST, aryFb[0]);

                // GT列をスキップ
                x += 5;

                // BPM (VEL)
                if (cursor.Position > 2)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(te.TempoValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // Tempo
                drawUtil.DrawCell("Tempo", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // K# (空欄)
                x += 4;

                // ST
                if (state == RowState.Cursor && cursor.Position < 3)
                    drawUtil.DrawCell(te.ST.ToString(), ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(te.ST != 0 ? te.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

                // GT列をスキップ
                x += 5;

                // BPM (VEL)
                if (state == RowState.Cursor && cursor.Position > 2)
                    drawUtil.DrawCell(te.TempoValue.ToString(), ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(te.TempoValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
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
