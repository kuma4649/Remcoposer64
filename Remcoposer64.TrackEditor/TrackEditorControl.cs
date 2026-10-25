using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using System.Drawing;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor
{
    public partial class TrackEditorControl : UserControl
    {
        public MIDIProject Project { get; private set; } = null;
        public Setting Setting { get; private set; } = null;
        public int HeaderLine { get; private set; } = 2;

        internal int pxScrollY = 0;

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
        }

        public void SetContext(MIDIProject project, Setting setting)
        {
            this.Project = project;
            this.Setting = setting;

            trackHeaderPanel.SetContext(this, project, setting);
            trackCanvasPanel.SetContext(this, project, setting);

            splitContainer1.SplitterDistance = this.ClientSize.Width / 2;

            UpdateVScrollRange();

            //Invalidate();
        }

        public void SetHeaderLine(int v)
        {
            HeaderLine = Math.Min(Math.Max(1, v), 3);

            trackHeaderPanel.Invalidate();
            trackCanvasPanel.Invalidate();

            UpdateVScrollRange();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
        }

        private void UpdateVScrollRange()
        {
            vScrollBar1.Minimum = 0;
            vScrollBar1.Maximum =
                Math.Max(
                    0,
                    Project.Track.Count * trackHeaderPanel.pxTrackHeight
                    - trackHeaderPanel.ClientSize.Height);
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            UpdateScrollPosition();
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

        public void SetInitialSplitDistance()
        {
            splitContainer1.SplitterDistance = ClientSize.Width / 3;
        }
    }
}
