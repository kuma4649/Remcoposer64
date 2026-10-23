using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor.Internal
{
    [ToolboxItem(false)]
    internal class TrackHeaderPanel : TrackPanelBase
    {
        public int pxOSFontHeight { get; private set; }
        public int pxTrackMarginTop { get; private set; }
        public int pxTrackMarginBottom { get; private set; }
        public int pxTrackMarginLeft { get; private set; }
        public int pxTrackBorderLineThin { get; private set; }

        public int HeaderLines { get; } = 1;

        public Color backColor = Color.FromArgb(0, 20, 20, 60);

        public TrackHeaderPanel()
        {
            pxOSFontHeight = Math.Max(1, (int)(pxFontHeight * 0.75));
            pxTrackMarginTop = Math.Max(1, (int)(pxFontHeight * 0.125));
            pxTrackMarginBottom = Math.Max(1, (int)(pxFontHeight * 0.125));
            pxTrackMarginLeft = Math.Max(1, (int)(pxFontHeight * 0.125));
            pxTrackBorderLineThin = Math.Max(1, (int)(pxFontHeight * 0.0625));
        }

        protected override void Render()
        {
            LinkedListNode<MIDITrack> trk = Project.Track.First;
            Point pos = Point.Empty;
            for (int i = 0; i < Project.Track.Count; i++)
            {
                // Back 
                pos.X = 0;
                pos.Y = 0 + i * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);
                graph.BackColor = backColor;
                graph.FillRectangle(
                    pos.X,
                    pos.Y,
                    this.ClientSize.Width,
                    pxFontHeight*HeaderLines
                    );


                // Track Number
                pos.X = pxTrackMarginLeft;
                pos.Y = pxTrackMarginTop + i * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);
                graph.DrawString(
                    $"{i + 1:D2} ",
                    ref pos,
                    Color.White,
                    backColor);

                // Track Name
                pos.Y = pxTrackMarginTop + i * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);
                graph.DrawStringOSFont(
                    $"{trk.Value.Name}",
                    ref pos,
                    Color.White,
                    Color.Black
                    );

                // ベゼル
                graph.BackColor = Color.White;
                pos.X = 0;
                pos.Y = 0 + i * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);
                graph.FillRectangle(pos.X, pos.Y, this.ClientSize.Width, pxTrackBorderLineThin);
                pos.X = 0;
                pos.Y = 0 + i * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);
                graph.FillRectangle(pos.X, pos.Y, pxTrackBorderLineThin, pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom - 1);
                graph.BackColor = Color.Gray;
                pos.X = 0;
                pos.Y = 0 + (i+1) * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom) - pxTrackBorderLineThin;
                graph.FillRectangle(pos.X, pos.Y, this.ClientSize.Width, pxTrackBorderLineThin);
                pos.X = this.ClientSize.Width - 1 - pxTrackBorderLineThin;
                pos.Y = 0 + i * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);
                graph.FillRectangle(pos.X, pos.Y, pxTrackBorderLineThin, pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom - 1);


                trk = trk.Next;

            }
        }
    }
}
