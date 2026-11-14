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
using Remcoposer64.Core.Player;

namespace Remcoposer64.TrackEditor.Internal
{
    [ToolboxItem(false)]
    public class TrackCanvasPanel : TrackPanelBase
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
            Setting setting,
            RmPlayer player)
        {
            base.SetContext(trackEditorControl, project, setting, player);

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

            long playX = 0;
            if (Project == Player.GetPlayingProject())
            {
                playX = TickToX(Player.GetFrameCounter());
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

                if (!string.IsNullOrEmpty(trk.Value.Memo))
                {
                    Point pos = new Point((int)(playX + 4), y + 4);
                    graph.DrawStringOSFont(trk.Value.Memo, ref pos, Color.LimeGreen, Color.Transparent);
                    trk.Value.MemoWait--;
                    if (trk.Value.MemoWait <= 0) trk.Value.Memo = "";
                }

                graph.BackColor = Color.FromArgb(0, 90, 80, 190);
                graph.FillRectangle(
                    0,
                    y + pxTrackHeight - 1,
                    ClientSize.Width,
                    1);


                trk = trk.Next;
            }

            if (Project == Player.GetPlayingProject())
            {
                graph.BackColor = Color.LimeGreen;
                graph.FillRectangle(
                    (int)playX,
                    0,
                    2,
                    ClientSize.Height);
            }

        }

        private void DrawParts(LinkedListNode<MIDITrack> trk, int y)
        {
            LinkedListNode<MIDIPart> part =
                trk.Value.getStartPartNode();

            while (part != null)
            {
                long x = TickToX(part.Value.StartTick);
                long w = TickToWidth(part.Value.LengthTick);
                if (w <= 0) w = 1;

                if (x >= ClientSize.Width)
                    break;

                if (x + w < 0)
                {
                    part = part.Next;
                    continue;
                }

                DrawPart(part.Value, (int)x, y, (int)w);

                part = part.Next;
            }
        }

        private void DrawPart(MIDIPart part, int x, int y, int w)
        {

            graph.BackColor = Color.DarkOrange;

            // Part chip外側
            graph.BackColor = Color.DarkOrange;
            graph.FillRectangle(
                x,
                y + pxTrackBorderLineThin,
                w,
                pxTrackHeight - pxTrackBorderLineThin * 2);

            if (w > pxTrackBorderLineThin * 2)
            {
                // Part chip内側下地
                graph.BackColor = Color.FromArgb(0, 40, 20, 10);
                graph.FillRectangle(
                    x + pxTrackBorderLineThin,
                    y + pxTrackBorderLineThin * 2,
                    w - pxTrackBorderLineThin * 2,
                    pxTrackHeight - pxTrackBorderLineThin * 4);

                drawEasyPianoRoll(part,x,y,w);
            }
        }

        private void drawEasyPianoRoll(MIDIPart part, int x, int y, int w)
        {
            graph.BackColor = Color.DarkOrange;
            int noteHeight = pxTrackHeight - pxTrackBorderLineThin * 4;

            Stack<(LinkedListNode<MIDIEvent>, int?)> LoopStack = new Stack<(LinkedListNode<MIDIEvent>, int?)>();
            Stack<LinkedListNode<MIDIEvent>> SameMeasStack = new Stack<LinkedListNode<MIDIEvent>>();
            Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode = new();
            ProjectData.Common.BuildMeasureStartNode(measureStartNode, part, 0, 0);

            LinkedListNode<MIDIEvent> nEvt = part.Event.First;
            int tick = 0;
            while (nEvt != null)
            {
                switch (nEvt.Value.Type)
                {
                    case MIDIEventType.NoteON:

                        MIDINoteEvent ne = (MIDINoteEvent)nEvt.Value;

                        if (ne.GT == 0 || ne.Vel == 0)
                        {
                            tick += nEvt.Value.ST;
                            nEvt = nEvt.Next;
                            continue;
                        }

                        int neHeight = noteHeight - 1 - (noteHeight - 1) * ne.KeyNumber / 127;
                        int neWidth = Math.Max(1, (int)(ne.GT * tickZoom));
                        int drawX = x + pxTrackBorderLineThin + (int)(tick * tickZoom);
                        if (drawX >= x + w)
                        {
                            tick += nEvt.Value.ST;
                            nEvt = nEvt.Next;
                            continue;
                        }
                        neWidth =
                            Math.Min(
                                neWidth,
                                x + w - drawX);

                        if (SameMeasStack.Count > 0 || LoopStack.Count > 0)
                            graph.BackColor = Color.FromArgb(Math.Min(255, ne.Vel * 2), 80, 255, 80);
                        else
                            graph.BackColor = Color.FromArgb(Math.Min(255, ne.Vel * 2), 255, 180, 80);
                        graph.FillRectangle(
                            x + pxTrackBorderLineThin + (int)(tick * tickZoom),
                            y + pxTrackBorderLineThin * 2 + neHeight,
                            neWidth, 1);

                        tick += nEvt.Value.ST;
                        nEvt = nEvt.Next;
                        continue;

                    case MIDIEventType.LoopStart:
                        LoopStack.Push((nEvt.Next, null));
                        nEvt = nEvt.Next;
                        continue;

                    case MIDIEventType.LoopEnd:
                        if (LoopStack.Count > 0)
                        {
                            var (levt, lcnt) = LoopStack.Pop();
                            MIDILoopEndEvent leEvt = (MIDILoopEndEvent)nEvt.Value;
                            if (lcnt == null)
                            {
                                lcnt = leEvt.LoopCounter;
                            }

                            lcnt--;

                            if (lcnt > 0)
                            {
                                LoopStack.Push((levt, lcnt));
                                nEvt = levt;
                                continue;
                            }
                        }
                        nEvt = nEvt.Next;
                        continue;

                    case MIDIEventType.SameMeas:
                        if (SameMeasStack.Count > 0)
                        {
                            nEvt = SameMeasStack.Pop();
                            continue;
                        }

                        bool first = true;
                        while (nEvt.Value.Type == MIDIEventType.SameMeas)
                        {
                            var sevt = (MIDISameMeasEvent)nEvt.Value;
                            int target = sevt.RepeatMeas;

                            var key = (0, 0, target);

                            if (!measureStartNode.TryGetValue(key, out var jumpNode))
                            {
                                Log.Write(LogLevel.Error, $"TrackCanvasPanel.cs: same measureEvent: The link cannot be found : Part Name:{part.Name}");
                                goto jumpLabel;
                                //break;
                            }

                            if (jumpNode.Value.Type == MIDIEventType.BarLine)
                            {
                                goto jumpLabel;
                                //if (jumpNode.Next.Value.Type == MIDIEventType.SameMeas)
                                //{
                                //    if (first) SameMeasStack.Push(nEvt.Next);
                                //    first = false;

                                //    if (nEvt == jumpNode.Next)
                                //    {
                                //        Log.Write(LogLevel.Error, $"TrackCanvasPanel.cs: same measureEvent: Self-reference Case 1 : Part Name:{part.Name} Event:{((MIDISameMeasEvent)nEvt.Value).RepeatMeas}");
                                //        nEvt = nEvt.Next;
                                //        break;
                                //    }

                                //    nEvt = jumpNode.Next;
                                //    continue;
                                //}

                                //// ★ 戻り位置をスタックに積む（元の位置の次イベント）
                                //if (first) SameMeasStack.Push(nEvt.Next);
                                //nEvt = jumpNode.Next;
                                //break;
                            }

                            if (jumpNode.Value.Type == MIDIEventType.SameMeas)
                            {
                                if (first) SameMeasStack.Push(nEvt.Next);
                                first = false;

                                if (nEvt == jumpNode)
                                {
                                    Log.Write(LogLevel.Error, $"TrackCanvasPanel.cs: same measureEvent: Self-reference Case 2 : Part Name:{part.Name} Event:{((MIDISameMeasEvent)nEvt.Value).RepeatMeas}");
                                    nEvt = nEvt.Next;
                                    break;
                                }

                                nEvt = jumpNode;
                                continue;
                            }

                            // ★ 戻り位置をスタックに積む（元の位置の次イベント）
                            if (first) SameMeasStack.Push(nEvt.Next);

                            nEvt = jumpNode;
                            break;

                        }
                        continue;

                    case MIDIEventType.BarLine:
                    case MIDIEventType.MetaEndOfTrack:
                        if (SameMeasStack.Count > 0)
                        {
                            nEvt= SameMeasStack.Pop();
                            continue;
                        }
                        break;
                }
            jumpLabel:
                tick += nEvt.Value.ST;
                nEvt = nEvt.Next;
                continue;
            }


        }

        public double tickZoom = 0.1;

        public long TickToX(long tick)
        {
            return (long)(tick * tickZoom) - TrackEditorControl.pxScrollX;
        }
        
        public long TickToWidth(long tick)
        {
            return (long)(tick * tickZoom);
        }
        public long TickToXAbsolute(long tick)
        {
            return (long)(tick * tickZoom);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            int trackNo = (e.Y + TrackEditorControl.pxScrollY) / pxTrackHeight;

            LinkedListNode<MIDITrack> trk = TrackEditorControl.GetTrackByIndex(trackNo);
            if (trk == null) return;

            TrackEditorControl.MoveCursorTrack(trk);
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
