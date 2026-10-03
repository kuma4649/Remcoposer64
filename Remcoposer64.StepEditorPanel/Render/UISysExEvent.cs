using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UISysExEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);

        public UISysExEvent(DrawUtil drawUtil) : base(drawUtil)
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
            MIDISysExEvent sysEx = (MIDISysExEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            if (sysEx.editing)
            {
                FBColor fbC = aryFb[1];
                string val = sysEx.editVal;
                if (string.IsNullOrWhiteSpace(sysEx.editVal))
                {
                    val = sysEx.virVal;
                    fbC = aryFb[2];
                }

                // SysEX
                drawUtil.DrawCell("SysEX", ref pos, ref x, 2, aryFb[0]);

                // ST
                x += 4;
                if (cursor.Position < 3)
                    drawUtil.DrawCell(val, ref pos, ref x, 4, fbC);
                else
                    drawUtil.DrawCell(sysEx.ST.ToString(), ref pos, ref x, 4, aryFb[0]);

                // Exclusive
                string data = sysEx.Name;
                if (string.IsNullOrEmpty(sysEx.Name)) data = " ( " + sysEx.DataString + " )";
                if (data.Length > 18)
                    data = data.Substring(0, 6) + "...";
                x++;
                if (cursor.Position > 2)
                    drawUtil.DrawCellOSFont(data, ref pos, ref x, 5, fbC, false);
                else
                    drawUtil.DrawCellOSFont(data, ref pos, ref x, 5, aryFb[0], false);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // SysEX
                drawUtil.DrawCell("SysEX", ref pos, ref x, 2, aryFb[0]);

                // ST
                x += 4;
                if (state == RowState.Cursor && cursor.Position < 3)
                    drawUtil.DrawCell(sysEx.ST.ToString(), ref pos, ref x, 4, fbC);
                else
                    drawUtil.DrawCell(sysEx.ST.ToString(), ref pos, ref x, 4, aryFb[0]);

                // Exclusive
                string data = sysEx.Name;
                if (string.IsNullOrEmpty(sysEx.Name)) data = " ( " + sysEx.DataString + " )";
                if (data.Length > 18)
                    data = data.Substring(0, 6) + "...";
                x++;
                if (state == RowState.Cursor && cursor.Position > 2)
                    drawUtil.DrawCellOSFont(data, ref pos, ref x, 5, fbC, false);
                else
                    drawUtil.DrawCellOSFont(data, ref pos, ref x, 5, aryFb[0], false);
            }
        }

    }
}
