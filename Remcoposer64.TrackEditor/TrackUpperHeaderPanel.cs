using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor
{
    public class TrackUpperHeaderPanel : HeaderPanelBase
    {
        protected override void Render()
        {
            bool hasSolo = Project.Track.Any(t => t.Solo);
            bool hasMute = Project.Track.Any(t => t.Mute);

            Color BGColor = Color.DarkSlateBlue;
            Color FGColor = Color.FromArgb(0, 150, 140, 250);
            Color EdgeColor = Color.White;
            TrackHeaderColumn hCol = TrackEditorControl.TrackCursor.HeaderColumn;

            graph.BackColor = BGColor;
            graph.FillRectangle(0, 0, Width, Height);
            Point pos = new Point(0, 0);

            graph.DrawString("#", ref pos, FGColor, BGColor);

            Color col = (!hasSolo && hasMute) ? Color.LightPink : Color.DarkMagenta;
            pos.X = pxFontHeight * 3;
            graph.DrawString("M", ref pos, col, BGColor);
            col = hasSolo ? Color.LightBlue : Color.Blue;
            pos.X = pxFontHeight * 4;
            graph.DrawString("S", ref pos, col, BGColor);

            if (hCol < TrackHeaderColumn.OutChannel)
            {
                pos.X = pxFontHeight * 6;
                graph.DrawString("ST+", ref pos, FGColor, BGColor);
                pos.X = pxFontHeight * 11;
                graph.DrawString("KEY+", ref pos, FGColor, BGColor);
                pos.X = pxFontHeight * 16;
                graph.DrawString("NAME", ref pos, FGColor, BGColor);
            }
            else if (hCol < TrackHeaderColumn.InChannel)
            {
                pos.X = pxFontHeight * 6;
                graph.DrawString("Ch.", ref pos, FGColor, BGColor);
                pos.X = pxFontHeight * 10;
                graph.DrawString("Device  Output", ref pos, FGColor, BGColor);
            }
            else
            {
                pos.X = pxFontHeight * 6;
                graph.DrawString("Ch.", ref pos, FGColor, BGColor);
                pos.X = pxFontHeight * 10;
                graph.DrawString("Device  Input", ref pos, FGColor, BGColor);
            }

            graph.DrawLine(0, Height - 1, Width, Height - 1, EdgeColor);
        }
    }
}
