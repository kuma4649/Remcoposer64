using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIPitchBendEvent : UIEvent
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


        public UIPitchBendEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDIPitchBendEvent pb = (MIDIPitchBendEvent)ev;

            FBColor[] aryFb = state == RowState.Cursor
                ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : (state == RowState.Normal
                ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            // 表示値（Raw or Centered）
            int dispValue = pb.GetDisplayValue();

            if (pb.editing)
            {
                FBColor fbC = aryFb[1];
                string val = pb.editVal;
                if (string.IsNullOrWhiteSpace(pb.editVal))
                {
                    val = pb.virVal;
                    fbC = aryFb[2];
                }

                // PitchBend
                drawUtil.DrawCell("Pitch", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // keynumber列をスキップ
                x += 4;

                // ST
                if (cursor.Position < 3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(pb.ST.ToString(), ref pos, ref x, COL_ST, aryFb[0]);

                // GT列をスキップ
                x += 5;

                // Value (VEL列を使う)
                if (cursor.Position > 2)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(dispValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // PitchBend
                drawUtil.DrawCell("Pitch", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // keynumber列をスキップ
                x += 4;

                // ST
                if (state == RowState.Cursor && cursor.Position < 3)
                    drawUtil.DrawCell(pb.ST.ToString(), ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(pb.ST != 0 ? pb.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

                // GT列をスキップ
                x += 5;

                // Value (VEL列を使う)
                if (state == RowState.Cursor && cursor.Position > 2)
                    drawUtil.DrawCell(dispValue.ToString(), ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(dispValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
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
