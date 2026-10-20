using global::Remcoposer64.StepEditorPanelControl.Render;
using Remcoposer64.Common.Render;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using System.Xml.Linq;

namespace Remcoposer64.StepEditorPanelControl
{
    public class EventListRenderer
    {
        private readonly StepEditorPanel parent;

        private readonly UIEventProvider uiep;

        private DrawUtil drawUtil;

        public enum CursorStyle
        {
            None,
            Underline
        }

        public EventListRenderer(StepEditorPanel parent, IGraph graph, int charWidth, int lineHeight, int[] columnWidths, string[] headers)
        {
            this.parent = parent;
            drawUtil = new DrawUtil(graph, charWidth, lineHeight, columnWidths, headers);
            uiep = new UIEventProvider(drawUtil);
        }

        public void Render(EventManager eventManager, Rectangle rect, int scrollOffset, CursorInfo cursor)
        {
            drawUtil.BeginPaint(rect);

            drawUtil.DrawHeader(rect);
            drawUtil.InitialOldMeasOldStep();

            bool dispStep = true;
            int oldStep = -1;
            bool firstDisp = true;

            if (eventManager.EventCount > 0)
            {
                CursorPosition playingCursorIndex = eventManager.GetPlayingCursorIndex();
                
                if (playingCursorIndex != null)
                {
                    scrollOffset = Math.Max(0, playingCursorIndex.Offset - 5);
                }

                int row = 0;
                for (int i = scrollOffset; i < scrollOffset + parent.VisibleRows + 1; i++)
                {
                    if (i >= eventManager.EventCount)
                        break;

                    var node = eventManager.GetEventNodeAtIndex(i);
                    var mdl = eventManager.GetModel(node.Value.Type);

                    bool isCursor = i == cursor.Index;
                    bool isPlayingCursor = (playingCursorIndex != null && node.Value.Meas == playingCursorIndex.Meas && node.Value.Step == playingCursorIndex.Step);
                    UIEvent uie = uiep.Get(node.Value.Type);

                    ProcessStepDisplayControl(ref oldStep, ref dispStep, ref firstDisp, node.Value);

                    DrawEventRow(node.Value, uie, mdl, row, i, rect, isCursor, isPlayingCursor, cursor, dispStep);
                    row++;
                }
            }

            drawUtil.EndPaint();
        }

        /// <summary>
        /// step表示制御処理
        /// </summary>
        private void ProcessStepDisplayControl(ref int oldStep,ref bool dispStep,ref bool firstDisp,MIDIEvent evt)
        {
            if (oldStep == evt.Step)
            {
                if (firstDisp)
                {
                    dispStep = true;

                    firstDisp = false;
                }
                else
                {
                    dispStep = false;
                }
            }
            else
            {
                dispStep = true;

                oldStep = evt.Step;
                firstDisp = false;
            }

            if (dispStep)
            {
                if (evt.Type == MIDIEventType.MetaTextEvent
                    || evt.Type == MIDIEventType.Memo
                    || evt.Type == MIDIEventType.BarLine
                    || evt.Type == MIDIEventType.LoopStart
                    || evt.Type == MIDIEventType.LoopEnd
                    || evt.Type == MIDIEventType.SameMeas
                    || evt.Type == MIDIEventType.EndOfTrack
                    || evt.Type == MIDIEventType.MetaEndOfTrack
                    )
                {
                    firstDisp = true;
                    dispStep = false;
                }
            }

        }

        public void DrawEventRow(MIDIEvent ev, UIEvent ue, MdlEvent me, int row, int absoluteIndex, Rectangle rect, bool isCursor, bool isPlayingCursor, CursorInfo cursor, bool dispStep)
        {
            Point pos = new(rect.Left + drawUtil.iconMergin, rect.Top + (1 + row) * drawUtil.lineHeight);

            RowState state;
            if (absoluteIndex == cursor.Index) state = RowState.Cursor;
            else if (cursor.IsSelectedRow(absoluteIndex)) state = RowState.Selected;
            else state = RowState.Normal;

            // 背景
            FBColor fb = ue.GetMeasStepColors(state);

            drawUtil.FillRectangle(rect.Left, pos.Y, rect.Width, drawUtil.lineHeight, fb.Back);

            // Meas/Step を描く
            int x = 0;
            drawUtil.DrawMeasAndStep(ev, ref pos, ref x, fb, isCursor, dispStep);

            // イベント を描く
            ue.DrawEvent(ev, me, pos, ref x, cursor, state);

            // 演奏中は演奏位置を描画
            if (isPlayingCursor)
            {
                int y = pos.Y + drawUtil.lineHeight - 2;
                drawUtil.DrawLine(rect.Left, y, rect.Right, y, System.Drawing.Color.Yellow);
            }
        }

    }
}