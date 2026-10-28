using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Remcoposer64.ProjectData.Events;

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


                DrawParts(trk, y);


                graph.BackColor = Color.FromArgb(0, 90, 80, 190);
                graph.FillRectangle(
                    0,
                    y + pxTrackHeight - 1,
                    ClientSize.Width,
                    1);


                trk = trk.Next;
            }
        }

        private void DrawParts(LinkedListNode<MIDITrack> trk, int y)
        {
            LinkedListNode<MIDIPart> part =
                trk.Value.getStartPartNode();

            while (part != null)
            {
                int x = TickToX(part.Value.StartTick);
                int w = TickToWidth(part.Value.LengthTick);
                if (w <= 0) w = 1;

                if (x >= ClientSize.Width)
                    break;

                if (x + w < 0)
                {
                    part = part.Next;
                    continue;
                }

                DrawPart(part.Value, x, y, w);

                part = part.Next;
            }
        }

        private void DrawPart(MIDIPart part, int x, int y, int w)
        {

            graph.BackColor = Color.DarkOrange;

            // Part chip
            graph.BackColor = Color.DarkOrange;
            graph.FillRectangle(
                x,
                y + pxTrackBorderLineThin,
                w,
                pxTrackHeight - pxTrackBorderLineThin * 2);
            if (w > pxTrackBorderLineThin * 2)
            {
                graph.BackColor = Color.FromArgb(0, 40, 20, 10);
                graph.FillRectangle(
                    x + pxTrackBorderLineThin,
                    y + pxTrackBorderLineThin * 2,
                    w - pxTrackBorderLineThin * 2,
                    pxTrackHeight - pxTrackBorderLineThin * 4);

                graph.BackColor = Color.DarkOrange;
                int noteHeight = pxTrackHeight - pxTrackBorderLineThin * 4;

                LinkedListNode<MIDIEvent> nevt = part.Event.First;
                int tick = 0;
                while (nevt != null)
                {
                    if(nevt.Value.Type!= MIDIEventType.NoteON)
                    {
                        tick += nevt.Value.ST;
                        nevt = nevt.Next;
                        continue;
                    }

                    MIDINoteEvent ne = (MIDINoteEvent)nevt.Value;
                    int neHeight = noteHeight - 1 - (noteHeight - 1) * ne.KeyNumber / 127;
                    int neWidth = Math.Max(1, (int)(ne.GT * tickZoom));
                    int drawX = x + pxTrackBorderLineThin + (int)(tick * tickZoom);
                    if (drawX >= x + w)
                    {
                        tick += ne.ST;
                        nevt = nevt.Next;
                        continue;
                    }
                    neWidth =
                        Math.Min(
                            neWidth,
                            x + w - drawX);

                    graph.BackColor = Color.FromArgb(Math.Min(255, ne.Vel * 2), 255, 180, 80);
                    graph.FillRectangle(
                        x + pxTrackBorderLineThin + (int)(tick * tickZoom),
                        y + pxTrackBorderLineThin * 2 + neHeight,
                        neWidth, 1);
                    tick += ne.ST;
                    nevt = nevt.Next;
                }

            }
        }

        private double tickZoom = 0.1;

        private int TickToX(int tick)
        {
            return (int)(tick * tickZoom) - TrackEditorControl.pxScrollX;
        }
        
        public int TickToWidth(int tick)
        {
            return (int)(tick * tickZoom);
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
