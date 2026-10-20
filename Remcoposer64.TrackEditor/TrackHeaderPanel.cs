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

        public TrackHeaderPanel()
        {
        }

        protected override void Render()
        {
            LinkedListNode<MIDITrack> trk = Project.Track.First;
            for (int i = 0; i < Project.Track.Count; i++)
            {
                Point pos = new Point(2, i * 16);

                graph.DrawString(
                    $"{i + 1:D2} {trk.Value.Name}",
                    ref pos,
                    Color.White,
                    Color.Black);

                trk = trk.Next;
            }
        }
    }
}
