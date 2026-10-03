using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UISameMeasEvent : UIEvent
    {
        private static readonly FBColor Cursor = new(Color.Black, Color.White);
        private static readonly FBColor Normal = new(Color.White, Color.Black);
        private static readonly FBColor Select = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.Magenta, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);

        public UISameMeasEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? Cursor
                : (state == RowState.Normal ? Normal
                : Select);
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventNormal]
                         : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);
            MIDISameMeasEvent sameMeas =(MIDISameMeasEvent)ev;

            string text;
            if (sameMeas.RepeatMeas < 0) text = "Ref!";
            else text = string.Format("{0,4}", sameMeas.RepeatMeas + 1);
            int ox = x;

            drawUtil.DrawCell("======== ", ref pos, ref x, COL_NOTENAME, aryFb[0], false);
            drawUtil.DrawCell("======== "[(x - ox)..], ref pos, ref x, COL_KEYNUMBER, aryFb[0], false);

            if (sameMeas.editing)
            {
                string val = sameMeas.editVal;
                FBColor fbC = aryFb[1];
                if (string.IsNullOrWhiteSpace(sameMeas.editVal))
                {
                    val = sameMeas.virVal;
                    fbC = aryFb[2];
                }

                if (cursor.Position >= 0)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC, true);
                else
                    drawUtil.DrawCell(text, ref pos, ref x, COL_ST, aryFb[0], false);
            }
            else
            {
                FBColor fbC = aryFb[3];
                if (cursor.Position >= 0)
                    drawUtil.DrawCell(text, ref pos, ref x, COL_ST, fbC, true);
                else
                    drawUtil.DrawCell(text, ref pos, ref x, COL_ST, aryFb[0], false);
            }

            drawUtil.DrawCell(" ========", ref pos, ref x, COL_GT, aryFb[0], false);

        }

    }
}
