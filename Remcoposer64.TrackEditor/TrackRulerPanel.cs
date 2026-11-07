using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor
{
    public class TrackRulerPanel : HeaderPanelBase
    {
        protected override void Render()
        {
            graph.BackColor = Color.DarkSlateBlue;
            graph.FillRectangle(0, 0, Width, Height);

            graph.DrawLine(0, Height - 1, Width, Height - 1, Color.White);
        }
    }
}
