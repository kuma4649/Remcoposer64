using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIBarLineEvent : UIEvent
    {
        private static FBColor[] ptn = [
            new(Color.Black, Color.White),// カーソル時は反転
            new(Color.White, Color.FromArgb(32, 64, 32)),// ノーマル時は背景が暗い緑
            new(Color.Black, Color.Gray),// 選択時
            new(Color.Black, Color.White),// カーソル
            new(Color.LightGreen, Color.FromArgb(32, 64, 32)), // ノーマル時は背景が暗い緑
            new(Color.Black, Color.Gray),// 選択時
            ];

        public UIBarLineEvent(DrawUtil drawUtil) : base(drawUtil)
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
            MIDIBarLineEvent barLine=(MIDIBarLineEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor ? [ptn[3]]
            : (state == RowState.Normal ? [ptn[4]]
            : [ptn[5]]);

            string text = string.Format("-------- {0,4} --------", barLine.MeasureTotalST);

            drawUtil.DrawCell(text, ref pos, ref x, 2, aryFb[0], false);
        }

    }
}
