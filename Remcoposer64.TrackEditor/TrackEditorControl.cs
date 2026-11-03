using Remcoposer64.Common;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using Remcoposer64.TrackEditor.Command;
using System.Diagnostics;
using static Remcoposer64.Common.SimpleCandidateForm;

namespace Remcoposer64.TrackEditor
{
    public partial class TrackEditorControl : UserControl
    {
        public MIDIProject Project { get; private set; } = null;
        public Setting Setting { get; private set; } = null;
        public RmPlayer Player { get; private set; } = null;

        public UndoRedoManager.UndoRedoManager UndoRedoManager { get; private set; } = null;

        public int HeaderLine { get; private set; } = 2;
        public TrackEditorCursor TrackCursor = new();
        public TrackEditorSelection TrackSelection = new();

        public int pxScrollY = 0;
        public int pxScrollX = 0;
        public int pxHeaderScrollX = 0;


        // 編集向け

        private SimpleCandidateForm? candidateForm;
        public bool editingOutChannel;
        public string outChannelInput = "";
        public bool editingInChannel;
        public string inChannelInput = "";



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

        public void SetContext(MIDIProject project, Setting setting, RmPlayer player, UndoRedoManager.UndoRedoManager undoRedoManager)
        {
            this.Project = project;
            this.Setting = setting;
            this.Player = player;
            this.UndoRedoManager = undoRedoManager;

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

            int maxTick = GetMaxTick();
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

        private void hHeaderScrollBar_ValueChanged(object sender, EventArgs e)
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
            trackCanvasPanel.tickZoom = ((TrackBar)sender).Value * 0.0002;
            UpdateScrollRange();
            trackCanvasPanel.Invalidate();

        }

        public void ScrollPlayingMarker()
        {
            if (Project != Player.GetPlayingProject()) return;

            int maxTick = GetMaxTick();
            if (trackCanvasPanel.TickToWidth(maxTick) <= trackCanvasPanel.ClientSize.Width)
            {
                pxScrollX = 0;
                trackCanvasPanel.Refresh();
                return;
            }

            int playX = trackCanvasPanel.TickToXAbsolute((int)Player.GetFrameCounter());
            playX = Math.Max(playX - 50, 0);
            if (hScrollBar1.Value == playX) trackCanvasPanel.Refresh();
            else if (playX <= hScrollBar1.Maximum) hScrollBar1.Value = playX;
            else trackCanvasPanel.Refresh();
        }

        private int GetMaxTick()
        {
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
            return maxTick;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {

            Debug.WriteLine(keyData);
            // ★モディファイア除去
            Keys key = (keyData & Keys.KeyCode);
            // モディファイアキーの状態を取得
            bool ctrl = (keyData & Keys.Control) == Keys.Control;
            bool shift = (keyData & Keys.Shift) == Keys.Shift;
            bool alt = (keyData & Keys.Alt) == Keys.Alt;

#if DEBUG
            Debug.WriteLine(
                $"KeyDown: {key}, Ctrl: {ctrl}, Shift: {shift}, Alt: {alt}");
#endif

            if (ctrl && !shift && key == Keys.Z)
            {
                Debug.WriteLine("UNDO");
                UndoRedoManager?.Undo();
                RefreshCursor();
                return true;
            }

            if ((ctrl && !shift && key == Keys.Y)  || (ctrl && shift && key == Keys.Z))
            {
                Debug.WriteLine("REDO");
                UndoRedoManager?.Redo();
                RefreshCursor();
                return true;
            }


            if (editingOutChannel)
            {
                switch (key)
                {
                    case Keys.Enter:
                        CommitOutChannel(outChannelInput);
                        editingOutChannel = false;
                        outChannelInput = "";
                        RefreshCursor();
                        return true;

                    case Keys.Escape:
                        editingOutChannel = false;
                        outChannelInput = "";
                        RefreshCursor();
                        return true;
                }

                if ((key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9))
                {
                    if (key >= Keys.D0 && key <= Keys.D9) outChannelInput += (char)('0' + (key - Keys.D0));
                    else outChannelInput += (char)('0' + (key - Keys.NumPad0));

                    if (outChannelInput.Length >= 2)
                    {
                        CommitOutChannel(outChannelInput);

                        editingOutChannel = false;
                        outChannelInput = "";

                        RefreshCursor();
                    }
                    else
                    {
                        RefreshCursor();
                    }

                    return true;
                }
            }

            if (TrackCursor.HeaderColumn == TrackHeaderColumn.OutChannel)
            {
                if ((key >= Keys.D0 && key <= Keys.D9)||(key >= Keys.NumPad0 && key <= Keys.NumPad9))
                {
                    BeginOutChannelEdit(key);
                    return true;
                }
            }

            if (editingInChannel)
            {
                switch (key)
                {
                    case Keys.Enter:
                        CommitInChannel(inChannelInput);
                        editingInChannel = false;
                        inChannelInput = "";
                        RefreshCursor();
                        return true;

                    case Keys.Escape:
                        editingInChannel = false;
                        inChannelInput = "";
                        RefreshCursor();
                        return true;
                }

                if ((key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9))
                {
                    if (key >= Keys.D0 && key <= Keys.D9) inChannelInput += (char)('0' + (key - Keys.D0));
                    else inChannelInput += (char)('0' + (key - Keys.NumPad0));

                    if (inChannelInput.Length >= 2)
                    {
                        CommitInChannel(inChannelInput);

                        editingInChannel = false;
                        inChannelInput = "";

                        RefreshCursor();
                    }
                    else
                    {
                        RefreshCursor();
                    }

                    return true;
                }
            }

            if (TrackCursor.HeaderColumn == TrackHeaderColumn.InChannel)
            {
                if ((key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9))
                {
                    BeginInChannelEdit(key);
                    return true;
                }
            }


            if (candidateForm != null && candidateForm.Visible)
            {
                switch (keyData)
                {
                    case Keys.Up:
                    case Keys.Down:
                    case Keys.Left:
                    case Keys.Right:
                    case Keys.Enter:
                    case Keys.Space:
                        candidateForm.MoveSelection(keyData);
                        return true;

                    case Keys.Escape:
                        candidateForm.Hide();
                        return true;
                }
            }

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

                case Keys.Left:
                    MoveCursorColumn(
                        trackHeaderPanel.PrevColumn(
                            TrackCursor.HeaderColumn));
                    return true;

                case Keys.Right:
                    MoveCursorColumn(
                        trackHeaderPanel.NextColumn(
                            TrackCursor.HeaderColumn));
                    return true;

                case Keys.Space:
                    ExecuteCurrentColumnAction();
                    return true;

                case Keys.Enter:
                    RaiseTrackDoubleClicked(TrackCursor.Track);
                    return true;
            }

            return base.ProcessCmdKey(
                ref msg,
                keyData);
        }

        public void ClearInputMode()
        {
            candidateForm?.Hide();
            editingOutChannel = false;
            outChannelInput = "";
            editingInChannel = false;
            inChannelInput = "";
        }

        public void MoveCursorTrack(LinkedListNode<MIDITrack> trk)
        {
            MoveCursorTrack(trk, TrackCursor.HeaderColumn);
        }

        public void MoveCursorTrack(LinkedListNode<MIDITrack> trk, TrackHeaderColumn column)
        {
            editingOutChannel = false;
            outChannelInput = "";
            editingInChannel = false;
            inChannelInput = "";

            TrackCursor.Track = trk;
            TrackCursor.Part = trk.Value.Part?.First;
            TrackCursor.HeaderColumn = column;

            EnsureCursorVisible();
            RefreshCursor();
        }

        public void MoveCursorColumn(TrackHeaderColumn column)
        {
            editingOutChannel = false;
            outChannelInput = "";
            editingInChannel = false;
            inChannelInput = "";

            TrackCursor.HeaderColumn = column;

            EnsureCursorVisible();
            RefreshCursor();
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

        public LinkedListNode<MIDITrack>? GetTrackByIndex(int trackNo)
        {
            if (trackNo < 0 || trackNo >= Project.Track.Count)
                return null;

            LinkedListNode<MIDITrack> trk =
                Project.Track.First;

            for (int i = 0; i < trackNo; i++)
            {
                if (trk == null)
                    return null;

                trk = trk.Next;
            }

            return trk;

        }

        private void btnStretch_Click(object sender, EventArgs e)
        {
            StretchZoom();
        }

        public void StretchZoom()
        {
            if (Project == null || Project.Track.Count < 1) return;

            int maxAbsTick = 0;
            LinkedListNode<MIDITrack> nTrk = Project.Track.First;
            while (nTrk != null)
            {
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                while (nPrt != null)
                {

                    int absTick = nPrt.Value.StartTick + nPrt.Value.LengthTick;
                    maxAbsTick = Math.Max(maxAbsTick, absTick);

                    nPrt = nPrt.Next;
                }

                nTrk = nTrk.Next;
            }

            double zm = (double)trackCanvasPanel.Width / (double)maxAbsTick / 0.0002;
            //trackBar1.Value = Math.Max((int)Math.Round(zm, MidpointRounding.AwayFromZero), 1);
            trackBar1.Value = Math.Min(Math.Max((int)zm, 1), trackBar1.Maximum);
        }

        public void SetCanvasScrollX(int v)
        {
            pxScrollX = v;
            hScrollBar1.Value = v;
        }

        public void ExecuteCurrentColumnAction()
        {
            if (TrackCursor.Track == null)
                return;

            switch (TrackCursor.HeaderColumn)
            {
                case TrackHeaderColumn.Mute:
                    TrackCursor.Track.Value.Mute =
                        !TrackCursor.Track.Value.Mute;
                    break;

                case TrackHeaderColumn.Solo:
                    TrackCursor.Track.Value.Solo =
                        !TrackCursor.Track.Value.Solo;
                    break;

                case TrackHeaderColumn.OutDevice:
                    ShowOutDeviceCandidates();
                    break;

                case TrackHeaderColumn.InDevice:
                    ShowInDeviceCandidates();
                    break;
            }

            RefreshCursor();
        }

        private void ShowOutDeviceCandidates()
        {
            if (TrackCursor.Track == null)
                return;

            List<SimpleCandidateForm.CandidateItem> items = new();

            var devs =
                Setting.midiOut.lstMidiOutInfo[
                    Setting.midiOut.CurrentDev];
            if (devs == null) return;

            for (int i = 0; i < devs.Length; i++)
            {
                items.Add(
                    new SimpleCandidateForm.CandidateItem()
                    {
                        Value = i,
                        Text = devs[i].alias
                    });
            }

            candidateForm?.Dispose();
            candidateForm = null;

            candidateForm =
                new SimpleCandidateForm(
                    items,
                    TrackCursor.Track.Value.OutDevice,
                    SimpleCandidateForm.CandidateFilterMode.Text,
                    1,
                    CandidateLayoutMode.VerticalList);

            candidateForm.CandidateSelected += item =>
            {
                MIDITrack track =
                    TrackCursor.Track.Value;

                if (track.OutDevice == item.Value)
                    return;

                UndoRedoManager.Execute(
                    new ChangeTrackOutDeviceCommand(
                        track,
                        track.OutDevice,
                        item.Value));
                Debug.WriteLine(UndoRedoManager.GetDebugInfo());

                RefreshCursor();
            };


            int trackY = TrackCursor.Track.Value.Number * trackHeaderPanel.pxTrackHeight - pxScrollY;
            Rectangle rc = trackHeaderPanel.GetColumnRect(trackY, TrackCursor.HeaderColumn);
            Point pt = trackHeaderPanel.PointToScreen(new Point(
                        rc.Left,
                        rc.Bottom));
            candidateForm.StartPosition = FormStartPosition.Manual;

            candidateForm.Location = pt;
            candidateForm.Hide();
            candidateForm.Show(this);
            //candidateForm.Refresh();
        }

        private void ShowInDeviceCandidates()
        {
            if (TrackCursor.Track == null)
                return;

            List<SimpleCandidateForm.CandidateItem> items = new();

            var devs =
                Setting.midiIn.lstMidiInInfo[
                    Setting.midiIn.CurrentDev];
            if (devs == null) return;

            for (int i = 0; i < devs.Length; i++)
            {
                items.Add(
                    new SimpleCandidateForm.CandidateItem()
                    {
                        Value = i,
                        Text = devs[i].alias
                    });
            }

            candidateForm?.Dispose();
            candidateForm = null;

            candidateForm =
                new SimpleCandidateForm(
                    items,
                    TrackCursor.Track.Value.InDevice,
                    SimpleCandidateForm.CandidateFilterMode.Text,
                    1,
                    CandidateLayoutMode.VerticalList);

            candidateForm.CandidateSelected += item =>
            {
                MIDITrack track =
                    TrackCursor.Track.Value;

                if (track.InDevice == item.Value)
                    return;

                UndoRedoManager.Execute(
                    new ChangeTrackInDeviceCommand(
                        track,
                        track.InDevice,
                        item.Value));
                Debug.WriteLine(UndoRedoManager.GetDebugInfo());

                RefreshCursor();
            };


            int trackY = TrackCursor.Track.Value.Number * trackHeaderPanel.pxTrackHeight - pxScrollY;
            Rectangle rc = trackHeaderPanel.GetColumnRect(trackY, TrackCursor.HeaderColumn);
            Point pt = trackHeaderPanel.PointToScreen(new Point(
                        rc.Left,
                        rc.Bottom));
            candidateForm.StartPosition = FormStartPosition.Manual;

            candidateForm.Location = pt;
            candidateForm.Hide();
            candidateForm.Show(this);
            //candidateForm.Refresh();
        }

        public void HideCandidateForm()
        {
            if (candidateForm != null)
                candidateForm.Hide();
        }



        private void BeginOutChannelEdit(Keys key)
        {
            editingOutChannel = true;


            if (key >= Keys.D0 && key <= Keys.D9) outChannelInput = ((int)(key - Keys.D0)).ToString();
            else outChannelInput = ((int)(key - Keys.NumPad0)).ToString();

            RefreshCursor();
        }

        private void CommitOutChannel(string text)
        {
            if (TrackCursor.Track == null)
                return;

            int? value = null;

            if (int.TryParse(text, out int ch))
            {
                if (ch >= 1 && ch <= 16)
                {
                    value = ch - 1;
                }
            }

            if (TrackCursor.Track.Value.OutChannel == value)
                return;

            UndoRedoManager.Execute(
                new ChangeTrackOutChannelCommand(
                    TrackCursor.Track.Value,
                    TrackCursor.Track.Value.OutChannel,
                    value));

            RefreshCursor();
        }

        private void BeginInChannelEdit(Keys key)
        {
            editingInChannel = true;


            if (key >= Keys.D0 && key <= Keys.D9) inChannelInput = ((int)(key - Keys.D0)).ToString();
            else inChannelInput = ((int)(key - Keys.NumPad0)).ToString();

            RefreshCursor();
        }

        private void CommitInChannel(string text)
        {
            if (TrackCursor.Track == null)
                return;

            int? value = null;

            if (int.TryParse(text, out int ch))
            {
                if (ch >= 1 && ch <= 16)
                {
                    value = ch - 1;
                }
            }

            if (TrackCursor.Track.Value.InChannel == value)
                return;

            UndoRedoManager.Execute(
                new ChangeTrackInChannelCommand(
                    TrackCursor.Track.Value,
                    TrackCursor.Track.Value.InChannel,
                    value));

            RefreshCursor();
        }
    }
}