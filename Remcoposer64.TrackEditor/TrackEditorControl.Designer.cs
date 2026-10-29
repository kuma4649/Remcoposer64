using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor
{
    partial class TrackEditorControl
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            hHeaderScrollBar = new HScrollBar();
            trackHeaderPanel = new Remcoposer64.TrackEditor.Internal.TrackHeaderPanel();
            trackBar1 = new TrackBar();
            hScrollBar1 = new HScrollBar();
            trackCanvasPanel = new Remcoposer64.TrackEditor.Internal.TrackCanvasPanel();
            vScrollBar1 = new VScrollBar();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trackBar1).BeginInit();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(hHeaderScrollBar);
            splitContainer1.Panel1.Controls.Add(trackHeaderPanel);
            splitContainer1.Panel1MinSize = 120;
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(trackBar1);
            splitContainer1.Panel2.Controls.Add(hScrollBar1);
            splitContainer1.Panel2.Controls.Add(trackCanvasPanel);
            splitContainer1.Size = new Size(490, 435);
            splitContainer1.SplitterDistance = 121;
            splitContainer1.TabIndex = 0;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            // 
            // hHeaderScrollBar
            // 
            hHeaderScrollBar.Dock = DockStyle.Bottom;
            hHeaderScrollBar.Location = new Point(0, 418);
            hHeaderScrollBar.Name = "hHeaderScrollBar";
            hHeaderScrollBar.Size = new Size(121, 17);
            hHeaderScrollBar.TabIndex = 1;
            hHeaderScrollBar.ValueChanged += hHeaderScrollBar_ValueChanged;
            // 
            // trackHeaderPanel
            // 
            trackHeaderPanel.BackColor = Color.FromArgb(48, 48, 48);
            trackHeaderPanel.Dock = DockStyle.Fill;
            trackHeaderPanel.Location = new Point(0, 0);
            trackHeaderPanel.Name = "trackHeaderPanel";
            trackHeaderPanel.Size = new Size(121, 435);
            trackHeaderPanel.TabIndex = 0;
            // 
            // trackBar1
            // 
            trackBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            trackBar1.Location = new Point(291, 415);
            trackBar1.Maximum = 100;
            trackBar1.Name = "trackBar1";
            trackBar1.Size = new Size(71, 45);
            trackBar1.TabIndex = 0;
            trackBar1.TickFrequency = 10;
            trackBar1.Value = 50;
            trackBar1.ValueChanged += trackBar1_ValueChanged;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hScrollBar1.Location = new Point(0, 418);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(288, 17);
            hScrollBar1.TabIndex = 0;
            hScrollBar1.ValueChanged += hScrollBar1_ValueChanged;
            // 
            // trackCanvasPanel
            // 
            trackCanvasPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackCanvasPanel.BackColor = Color.FromArgb(32, 32, 32);
            trackCanvasPanel.Location = new Point(0, 0);
            trackCanvasPanel.Name = "trackCanvasPanel";
            trackCanvasPanel.Size = new Size(365, 415);
            trackCanvasPanel.TabIndex = 0;
            // 
            // vScrollBar1
            // 
            vScrollBar1.Dock = DockStyle.Right;
            vScrollBar1.Location = new Point(493, 0);
            vScrollBar1.Name = "vScrollBar1";
            vScrollBar1.Size = new Size(17, 435);
            vScrollBar1.TabIndex = 1;
            vScrollBar1.Scroll += vScrollBar1_Scroll;
            vScrollBar1.ValueChanged += vScrollBar1_ValueChanged;
            // 
            // TrackEditorControl
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(vScrollBar1);
            Controls.Add(splitContainer1);
            Name = "TrackEditorControl";
            Size = new Size(510, 435);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)trackBar1).EndInit();
            ResumeLayout(false);
        }

        private SplitContainer splitContainer1;
        private Internal.TrackHeaderPanel trackHeaderPanel;
        private Internal.TrackCanvasPanel trackCanvasPanel;
        private VScrollBar vScrollBar1;
        private HScrollBar hScrollBar1;
        private HScrollBar hHeaderScrollBar;
        private TrackBar trackBar1;
    }
}