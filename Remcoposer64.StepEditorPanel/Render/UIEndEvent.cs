using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIEndEvent : UIEvent
    {
        private static FBColor[] ptn = [
            new(Color.White, Color.White),// カーソル文字を背景と同じ色にして隠す
            new(Color.Black, Color.Black),//　ノーマル文字を背景と同じ色にして隠す
            new(Color.White, Color.Gray),// 選択
            new(Color.Black, Color.White),// カーソル
            new(Color.LightGreen, Color.Black),//　ノーマル
            new(Color.White, Color.Gray),// 選択
            ];

        public UIEndEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? ptn[0]
                         : (state == RowState.Normal ? ptn[1]
                         : ptn[2]);
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            FBColor[] aryFb = state == RowState.Cursor ? [ptn[3]]
            : (state == RowState.Normal ? [ptn[4]]
            : [ptn[5]]);
            string text = "[End of track]";

            drawUtil.DrawCell(text, ref pos, ref x, 2, aryFb[0], false);
        }

    }
}
