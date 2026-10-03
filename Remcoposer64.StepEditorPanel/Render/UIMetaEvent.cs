using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIMetaEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);


        public UIMetaEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            drawUtil.DrawCell("Meta(TBD)", ref pos, ref x, 2, MeasStepNormal);

            // ST
            x += 4;
            if (cursor.Position < 3)
                drawUtil.DrawCell(ev.ST.ToString(), ref pos, ref x, 4, MeasStepNormal);
            else
                drawUtil.DrawCell(ev.ST.ToString(), ref pos, ref x, 4, MeasStepNormal);

        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                         : (state == RowState.Normal ? MeasStepNormal
                         : MeasStepSelect);
        }
    }
}
