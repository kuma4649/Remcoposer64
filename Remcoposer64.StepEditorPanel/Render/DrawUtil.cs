using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Render;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using System;
using System.Collections.Generic;
using System.Text;
using Remcoposer64.StepEditorPanelControl.Model;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class DrawUtil
    {
        public IGraph grp;
        public int charWidth;
        public int lineHeight;
        public int[] columnWidths;
        public string[] headers;
        public int iconMergin;
        private string oldMeas = "";
        private string oldStep = "";
        private int oldST = -1;


        public DrawUtil(IGraph grp, int charWidth, int lineHeight, int[] columnWidths, string[] headers)
        {
            this.grp = grp;
            this.charWidth = charWidth;
            this.lineHeight = lineHeight;
            this.columnWidths = columnWidths;
            this.headers = headers;
            iconMergin = (int)(charWidth * 1.2f);
        }

        public void DrawHeader(Rectangle rect)
        {
            grp.BackColor = System.Drawing.Color.Green;
            grp.FillRectangle(rect.Left, rect.Top, rect.Width, lineHeight);

            Point pos = new(rect.Left, rect.Top);
            int x = 0;

            for (int i = 0; i < headers.Length; i++)
            {
                pos.X = rect.Left + x * charWidth + iconMergin;
                grp.DrawString(headers[i], ref pos, System.Drawing.Color.Black, System.Drawing.Color.Green);
                x += columnWidths[i * 2 + 1];
            }
        }

        public void InitialOldMeasOldStep()
        {
            oldMeas = "";
            oldStep = "";
        }

        public void DrawMeasAndStep(MIDIEvent ev, ref Point pos, ref int x, FBColor fb, bool isCursor)
        {
            if (ev.editing)
            {
                DrawCell(ev.MeasString, ref pos, ref x, 0, fb);
                oldMeas = ev.MeasString;

                DrawCell(ev.StepString, ref pos, ref x, 1, fb);
                oldStep = ev.StepString;
                return;
            }

            DrawCell((oldMeas != ev.MeasString || isCursor) ? ev.MeasString : "     :", ref pos, ref x, 0, fb);
            oldMeas = ev.MeasString;

            DrawCell((oldStep != ev.StepString || isCursor) ? ev.StepString : "     :", ref pos, ref x, 1, fb);
            oldStep = ev.StepString;

            if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
            {
                oldStep = "";   // ← これで次のイベントは必ず Step を描く
            }

        }

        internal void BeginPaint(Rectangle rect)
        {
            if (grp == null) return;

            grp.BeginPaint(rect);

            grp.BackColor = System.Drawing.Color.Black;
            grp.FillRectangle(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        internal void EndPaint()
        {
            grp?.EndPaint();
        }

        internal void DrawCell(string text, ref Point pos, ref int x, int col, FBColor fb, bool alignRight = true)
        {
            int xx = pos.X;
            int xxx = pos.X + x * charWidth;
            int length = columnWidths[col * 2 + 1];

            if (alignRight)
            {
                int align = columnWidths[col * 2 + 0];
                if (align != 0)
                {
                    xxx += (length - text.Length) * charWidth;
                }
            }

            pos.X = xxx;
            grp.DrawString(text, ref pos, fb.Fore, fb.Back);
            pos.X = xx;

            x += length;
        }

        internal void DrawCellOSFont(
            string text,
            ref Point pos,
            ref int x,
            int col,
            FBColor fb,
            bool alignRight = true)
        {

            int xx = pos.X;
            int xxx = pos.X + x * charWidth;
            int length = columnWidths[col * 2 + 1];

            if (alignRight)
            {
                int align = columnWidths[col * 2 + 0];
                if (align != 0)
                {
                    // OSフォントは幅が可変なので、文字幅を計測する必要がある
                    int textWidth = (int)(text.Length * charWidth * 0.9); // 仮の幅計算
                    xxx += (length * charWidth - textWidth);
                }
            }

            pos.X = xxx;
            grp.DrawStringOSFont(text, ref pos, fb.Fore, fb.Back);
            pos.X = xx;

            x += length;
        }

        internal void DrawString(string v, ref Point tiePos, Color fore, Color back)
        {
            grp.DrawString(v, ref tiePos, fore, back);
        }

        internal void DrawStringOSFont(string left, ref Point posLeft, Color fore, Color back)
        {
            grp.DrawStringOSFont(left, ref posLeft, fore, back);
        }

        internal int MeasureStringOSFont(string left)
        {
            return ((GraphVortice)grp).MeasureStringOSFont(left);
        }

        internal void FillRectangle(int memoBoxDrawX, int memoBoxDrawY, int memoBoxDrawWidth, int memoBoxDrawHeight)
        {
            grp.FillRectangle(memoBoxDrawX, memoBoxDrawY, memoBoxDrawWidth, memoBoxDrawHeight);
        }

        internal void FillRectangle(int memoBoxDrawX, int memoBoxDrawY, int memoBoxDrawWidth, int memoBoxDrawHeight, Color color)
        {
            grp.BackColor = color;
            grp.FillRectangle(memoBoxDrawX, memoBoxDrawY, memoBoxDrawWidth, memoBoxDrawHeight);
        }

        internal void DrawLine(int left, int y1, int right, int y2, Color yellow)
        {
            grp.DrawLine(left, y1, right, y2, yellow);
        }
    }
}
