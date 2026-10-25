using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Remcoposer64.TrackEditor.Internal
{
    [ToolboxItem(false)]
    internal class TrackCanvasPanel : TrackPanelBase
    {
        //public int HeaderLines { get; } = 2;

        //public int pxTrackHeight => TrackEditorControl.HeaderLine * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);

        public Color backColor = Color.FromArgb(0, 20, 20, 60);

        public TrackCanvasPanel()
        {
        }

        public override void SetContext(
            TrackEditorControl trackEditorControl,
            MIDIProject project,
            Setting setting)
        {
            base.SetContext(trackEditorControl, project, setting);

        }


        protected override void Render()
        {
            graph.BackColor = backColor;
            graph.FillRectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);

            //int firstTrack =    Math.Max(0,        TrackEditorControl.pxScrollY / pxTrackHeight);
            //int visibleTracks =                ClientSize.Height / pxTrackHeight + 2;
            //int lastTrack =                Math.Min(                    Project.Track.Count,                    firstTrack + visibleTracks);
            LinkedListNode<MIDITrack> trk = Project.Track.First;
            for (int i = 0; i < TrackEditorControl.FirstVisibleTrack; i++)
            {
                if (trk == null) return;
                trk = trk.Next;
            }

            for (int i = TrackEditorControl.FirstVisibleTrack; i < TrackEditorControl.LastVisibleTrack; i++)
            {
                if (trk == null)
                    break;

                int y = i * pxTrackHeight - TrackEditorControl.pxScrollY;

                graph.BackColor = backColor;
                graph.FillRectangle(
                    0,
                    y,
                    ClientSize.Width,
                    pxTrackHeight);


                // Part chip
                graph.BackColor = Color.DarkOrange;
                graph.FillRectangle(
                    i * 20,
                    y + pxTrackBorderLineThin,
                    100,
                    pxTrackHeight - pxTrackBorderLineThin*2);
                graph.BackColor = Color.FromArgb(0,40,20,10);
                graph.FillRectangle(
                    i * 20+ pxTrackBorderLineThin,
                    y + pxTrackBorderLineThin*2,
                    100- pxTrackBorderLineThin*2,
                    pxTrackHeight - pxTrackBorderLineThin*4);


                graph.BackColor = Color.FromArgb(0, 90, 80, 190);
                graph.FillRectangle(
                    0,
                    y + pxTrackHeight - 1,
                    ClientSize.Width,
                    1);


                trk = trk.Next;
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            int trackNo =
                (e.Y + TrackEditorControl.pxScrollY)
                / pxTrackHeight;

            if (trackNo < 0
                || trackNo >= Project.Track.Count)
                return;

            LinkedListNode<MIDITrack> trk =
                Project.Track.First;

            for (int i = 0; i < trackNo; i++)
            {
                if (trk == null)
                    return;

                trk = trk.Next;
            }

            TrackEditorControl.RaiseTrackDoubleClicked(trk);
        }



    }
}
