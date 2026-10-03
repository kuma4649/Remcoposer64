using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Render;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class UIMemoEvent : UIEvent
    {
        private static readonly FBColor Cursor = new(Color.Black, Color.White);
        private static readonly FBColor Normal = new(Color.White, Color.DarkBlue);
        private static readonly FBColor Select = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.White, Color.Black);
        private static readonly FBColor EventNormal = new(Color.White, Color.DarkBlue);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorComposition = new(Color.Yellow, Color.Black);


        private static readonly FBColor MemoColor = new(System.Drawing.Color.White, System.Drawing.Color.DarkBlue);

        public UIMemoEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state switch
            {
                RowState.Cursor => Cursor,
                RowState.Selected => Select,
                _ => Normal
            };
        }

        public override void DrawEvent(MIDIEvent ev,MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {

            var memo = (MIDIMemoEvent)ev;
            var mme = (MdlMemoEvent)me;

            if (!mme.memoBoxSwitch)
            {
                FBColor fb = state switch
                {
                    RowState.Cursor => EventCursor,
                    RowState.Selected => EventSelect,
                    _ => EventNormal
                };

                drawUtil.DrawCellOSFont(memo.Text, ref pos, ref x, 2, fb, false);
                return;
            }

            drawUtil.FillRectangle(mme.memoBoxDrawX, mme.memoBoxDrawY, mme.memoBoxDrawWidth, mme.memoBoxDrawHeight);

            if (!string.IsNullOrEmpty(mme.memoBoxText))
            {
                string left = mme.memoBoxText.Substring(0, mme.memoBoxCaretIndex);
                string right = mme.memoBoxText.Substring(mme.memoBoxCaretIndex);

                int baseX = mme.memoBoxDrawX;
                int y = mme.memoBoxDrawY;

                // 左側（確定文字）
                Point posLeft = new Point(baseX, y);
                drawUtil.DrawStringOSFont(left, ref posLeft, EventCursorEdit.Fore, EventCursorEdit.Back);

                // composition（前編集文字）
                int compX = baseX + drawUtil.MeasureStringOSFont(left);
                Point posComp = new Point(compX, y);
                if (!string.IsNullOrEmpty(mme.memoBoxComposition))
                {
                    drawUtil.DrawStringOSFont(mme.memoBoxComposition, ref posComp, EventCursorComposition.Fore, EventCursorComposition.Back);
                    int w = drawUtil.MeasureStringOSFont(mme.memoBoxComposition);
                    drawUtil.FillRectangle(compX, y + drawUtil.lineHeight - 2, w, 2, EventCursorComposition.Fore);
                }

                // 右側（確定文字）
                int rightX = compX + drawUtil.MeasureStringOSFont(mme.memoBoxComposition);
                Point posRight = new Point(rightX, y);
                drawUtil.DrawStringOSFont(right, ref posRight, EventCursorEdit.Fore, EventCursorEdit.Back);

                // キャレット描画
                int caretX = compX + drawUtil.MeasureStringOSFont(mme.memoBoxComposition.Substring(0, mme.IMECursorPos));
                drawUtil.FillRectangle(caretX, y, 1, drawUtil.lineHeight, Color.White);
            }
            else
            {
                Point pos_ = new Point(mme.memoBoxDrawX, mme.memoBoxDrawY);
                drawUtil.DrawStringOSFont(memo.virVal, ref pos_, EventCursorDefault.Fore, EventCursorDefault.Back);
            }

        }
    }
}
