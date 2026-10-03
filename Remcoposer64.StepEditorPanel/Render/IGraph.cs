using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public interface IGraph : IDisposable
    {
        Color ForeColor {  set; }
        Color BackColor {  set; }

        void BeginPaint(Rectangle rect);
        void EndPaint();

        void DrawGlyph(int x, int y, AsciiMonoCache.Glyph g, Color fg, Color bg);
        void DrawString(string text, ref Point pos, Color fg, Color bg);
        void DrawStringOSFont(string text, ref Point pos, Color fg, Color bg);
        void DrawLine(int x1, int y1, int x2, int y2, Color color);

        void SetFontSize(int size);
        void FillRectangle(int x, int y, int width, int height);

        void Resize(int width, int height);

   }
}
