using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIPolyphonicKeyPressureEvent : UIEvent
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


        public UIPolyphonicKeyPressureEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDIPolyphonicKeyPressureEvent pk = (MIDIPolyphonicKeyPressureEvent)ev;

            FBColor[] aryFb = state == RowState.Cursor
                ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : (state == RowState.Normal
                ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            if (pk.editing)
            {
                FBColor fbC = aryFb[1];
                string val = pk.editVal;

                if (string.IsNullOrWhiteSpace(pk.editVal))
                {
                    val = pk.virVal;
                    fbC = aryFb[2];
                }

                // After K.
                drawUtil.DrawCell("After K.", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // K#
                x += 4;

                // ST
                if (cursor.Position <3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(pk.ST.ToString(), ref pos, ref x, COL_ST, aryFb[0]);

                // KeyNumber (GT)
                if (cursor.Position == 3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_GT, fbC);
                else
                    drawUtil.DrawCell(pk.KeyNumber.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

                // PressureValue (VEL)
                if (cursor.Position == 4)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(pk.PressureValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // After K.
                drawUtil.DrawCell("After K.", ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // K#
                x += 4;

                // ST
                if (state == RowState.Cursor && cursor.Position <3)
                    drawUtil.DrawCell(pk.ST.ToString(), ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(pk.ST != 0 ? pk.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

                // KeyNumber (GT)
                if (state == RowState.Cursor && cursor.Position == 3)
                    drawUtil.DrawCell(pk.KeyNumber.ToString(), ref pos, ref x, COL_GT, fbC);
                else
                    drawUtil.DrawCell(pk.KeyNumber.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

                // PressureValue (GT)
                if (state == RowState.Cursor && cursor.Position == 4)
                    drawUtil.DrawCell(pk.PressureValue.ToString(), ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(pk.PressureValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
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
