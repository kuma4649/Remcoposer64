using Remcoposer64.Common;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using System.Drawing;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor
{
    public partial class TrackEditorControl : UserControl
    {
        public MIDIProject Project { get; private set; } = null;
        public Setting Setting { get; private set; } = null;
        public RmPlayer Player { get; private set; } = null;
        public int HeaderLine { get; private set; } = 2;
        public TrackEditorCursor TrackCursor = new();
        public TrackEditorSelection TrackSelection = new();

        internal int pxScrollY = 0;
        internal int pxScrollX = 0;
        internal int pxHeaderScrollX = 0;

        public event EventHandler<TrackDoubleClickEventArgs> TrackDoubleClicked;

        internal void RaiseTrackDoubleClicked(
            LinkedListNode<MIDITrack> trk)
        {
            TrackDoubleClicked?.Invoke(
                this,
                new TrackDoubleClickEventArgs(trk));
        }

        public int FirstVisibleTrack =>
    Math.Max(0, pxScrollY / trackHeaderPanel.pxTrackHeight);

        public int VisibleTrackCount =>
            trackCanvasPanel.ClientSize.Height
            / trackHeaderPanel.pxTrackHeight
            + 2;

        public int LastVisibleTrack =>
            Math.Min(
                Project.Track.Count,
                FirstVisibleTrack + VisibleTrackCount);


        public TrackEditorControl()
        {
            InitializeComponent();

            BackColor = Color.FromArgb(40, 40, 40);
            TrackCursor.Clear();
            TrackSelection.Clear();
        }

        public void SetContext(MIDIProject project, Setting setting, RmPlayer player)
        {
            this.Project = project;
            this.Setting = setting;
            this.Player = player;

            trackHeaderPanel.SetContext(this, project, setting, player);
            trackCanvasPanel.SetContext(this, project, setting, player);

            splitContainer1.SplitterDistance = this.ClientSize.Width / 2;

            TrackCursor.Clear();
            TrackSelection.Clear();
            if (project != null && project.Track != null)
                TrackCursor.Track = project.Track.First;
            if (TrackCursor.Track != null && TrackCursor.Track.Value.Part != null)
                TrackCursor.Part = TrackCursor.Track.Value.Part.First;

            UpdateScrollRange();
            trackCanvasPanel.Invalidate();
        }

        public void SetHeaderLine(int v)
        {
            pxScrollY = 0;
            vScrollBar1.Value = 0;

            HeaderLine = Math.Min(Math.Max(1, v), 3);

            trackHeaderPanel.Invalidate();
            trackCanvasPanel.Invalidate();

            UpdateScrollRange();
            trackCanvasPanel.Invalidate();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void UpdateScrollRange()
        {
            UpdateVScrollRange();
            UpdateHeaderHScrollRange();
            UpdateHScrollRange();
        }

        private void UpdateVScrollRange()
        {
            vScrollBar1.Minimum = 0;
            vScrollBar1.LargeChange = trackCanvasPanel.ClientSize.Height;

            if (Project == null) return;

            try
            {
                int maxScroll =
                    Math.Max(
                        0,
                        Project.Track.Count * (trackHeaderPanel.pxTrackHeight + 1)
                        - trackHeaderPanel.ClientSize.Height
                        );

                vScrollBar1.Maximum =
                    maxScroll + vScrollBar1.LargeChange - 1;

            }
            catch { }
        }

        private void UpdateHeaderHScrollRange()
        {
            hHeaderScrollBar.LargeChange =
                trackHeaderPanel.ClientSize.Width;

            int pxHeaderWidth =
                trackHeaderPanel.GetMaxWidth();

            int maxScroll =
                Math.Max(
                    0,
                    pxHeaderWidth
                    - trackHeaderPanel.ClientSize.Width);

            hHeaderScrollBar.Maximum =
                maxScroll
                + hHeaderScrollBar.LargeChange
                - 1;
        }

        private void UpdateHScrollRange()
        {
            hScrollBar1.LargeChange = trackCanvasPanel.ClientSize.Width;

            if (Project == null) return;

            int maxTick = 0;

            LinkedListNode<MIDITrack> trk =
                Project.Track.First;

            while (trk != null)
            {
                LinkedListNode<MIDIPart> part =
                    trk.Value.getStartPartNode();

                while (part != null)
                {
                    maxTick = Math.Max(
                        maxTick,
                        part.Value.StartTick
                        + part.Value.LengthTick);

                    part = part.Next;
                }

                trk = trk.Next;
            }

            int maxScroll =
                Math.Max(
                    0,
                    trackCanvasPanel.TickToWidth(maxTick)
                    - trackCanvasPanel.ClientSize.Width);

            hScrollBar1.Maximum =
                maxScroll + hScrollBar1.LargeChange - 1;
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            UpdateScrollPosition();

            int totalHeight =
                Project.Track.Count * trackHeaderPanel.pxTrackHeight;

            int visibleHeight =
                trackHeaderPanel.ClientSize.Height;

            int maxScroll =
                Math.Max(
                    0,
                    totalHeight - visibleHeight);

            //Debug.WriteLine($"TrackCount={Project.Track.Count}");
            //Debug.WriteLine($"TrackHeight={trackHeaderPanel.pxTrackHeight}");
            //Debug.WriteLine($"TotalHeight={totalHeight}");
            //Debug.WriteLine($"VisibleHeight={visibleHeight}");
            //Debug.WriteLine($"MaxScroll={maxScroll}");
            //Debug.WriteLine($"LargeChange={vScrollBar1.LargeChange}");
            //Debug.WriteLine($"ScrollY={pxScrollY}");
            //Debug.WriteLine($"HeaderHeight={trackHeaderPanel.ClientSize.Height}");
            //Debug.WriteLine($"CanvasHeight={trackCanvasPanel.ClientSize.Height}");
            //Debug.WriteLine($"First={FirstVisibleTrack}");
            //Debug.WriteLine($"Last={LastVisibleTrack}");
        }

        private void hScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            pxScrollX = hScrollBar1.Value;

            trackCanvasPanel.Invalidate();
        }

        private void hHeaderScrollBar_ValueChanged(    object sender,    EventArgs e)
        {
            pxHeaderScrollX = hHeaderScrollBar.Value;

            trackHeaderPanel.Invalidate();
        }

        private void UpdateScrollPosition()
        {
            pxScrollY = vScrollBar1.Value;

            trackHeaderPanel.Invalidate();
            trackCanvasPanel.Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            int step = trackHeaderPanel.pxTrackHeight;

            int value = vScrollBar1.Value;

            if (e.Delta > 0)
                value -= step;
            else
                value += step;

            value = Math.Max(
                vScrollBar1.Minimum,
                Math.Min(vScrollBar1.Maximum, value));

            vScrollBar1.Value = value;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (Project == null) return;

            UpdateScrollRange();
            trackCanvasPanel.Invalidate();
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            UpdateScrollRange();
            trackCanvasPanel.Invalidate();
        }

        public void SetInitialSplitDistance()
        {
            splitContainer1.SplitterDistance = ClientSize.Width / 3;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            trackCanvasPanel.tickZoom = ((TrackBar)sender).Value * 0.002;
            UpdateScrollRange();
            trackCanvasPanel.Invalidate();

        }

        public void ScrollPlayingMarker()
        {
            if (Project == Player.GetPlayingProject())
            {
                int playX = trackCanvasPanel.TickToXAbsolute((int)Player.GetFrameCounter());
                playX = Math.Max(playX - 50, 0);
                if (hScrollBar1.Value == playX)
                {
                    trackCanvasPanel.Refresh();
                }
                else
                    hScrollBar1.Value = playX;
            }

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Home:
                    MoveCursorTrack(Project.Track.First);
                    RefreshCursor();
                    return true;

                case Keys.End:
                    MoveCursorTrack(Project.Track.Last);
                    RefreshCursor();
                    return true;

                case Keys.Up:
                    if (TrackCursor.Track?.Previous != null)
                        MoveCursorTrack(TrackCursor.Track.Previous);
                    RefreshCursor();
                    return true;

                case Keys.Down:
                    if (TrackCursor.Track?.Next != null)
                        MoveCursorTrack(TrackCursor.Track.Next);
                    RefreshCursor();
                    return true;

                case Keys.Enter:
                    RaiseTrackDoubleClicked(TrackCursor.Track);
                    return true;
            }

            return base.ProcessCmdKey(
                ref msg,
                keyData);
        }

        private void MoveCursorTrack(    LinkedListNode<MIDITrack> trk)
        {
            TrackCursor.Track = trk;
            TrackCursor.Part = trk.Value.Part?.First;
            EnsureCursorVisible();
        }
        private void EnsureCursorVisible()
        {
            if (TrackCursor.Track == null)
                return;

            int index = TrackCursor.Track.Value.Number;

            int y = index * trackHeaderPanel.pxTrackHeight;

            if (y < pxScrollY)
            {
                vScrollBar1.Value =
                    Math.Max(
                        0,
                        y);
            }
            else if (
                y + trackHeaderPanel.pxTrackHeight >
                pxScrollY + trackCanvasPanel.ClientSize.Height)
            {
                vScrollBar1.Value =
                    Math.Min(
                        vScrollBar1.Maximum,
                        y + trackHeaderPanel.pxTrackHeight
                        - trackCanvasPanel.ClientSize.Height);
            }
        }

        private void RefreshCursor()
        {
            trackHeaderPanel.Refresh();
            trackCanvasPanel.Refresh();
        }
    }
}