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
            trackHeaderPanel = new Internal.TrackHeaderPanel();
            trackCanvasPanel = new Internal.TrackCanvasPanel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.IsSplitterFixed = false;
            splitContainer1.BorderStyle = BorderStyle.None;
            splitContainer1.Panel1MinSize = 120;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(trackHeaderPanel);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(trackCanvasPanel);
            splitContainer1.Size = new Size(510, 435);
            splitContainer1.SplitterDistance = 121;
            splitContainer1.TabIndex = 0;
            // 
            // trackHeaderPanel
            // 
            trackHeaderPanel.Dock = DockStyle.Fill;
            trackHeaderPanel.Location = new Point(0, 0);
            trackHeaderPanel.Name = "trackHeaderPanel";
            trackHeaderPanel.Size = new Size(121, 435);
            trackHeaderPanel.TabIndex = 0;
            trackHeaderPanel.BackColor = Color.FromArgb(48, 48, 48);
            // 
            // trackCanvasPanel
            // 
            trackCanvasPanel.Dock = DockStyle.Fill;
            trackCanvasPanel.Location = new Point(0, 0);
            trackCanvasPanel.Name = "trackCanvasPanel";
            trackCanvasPanel.Size = new Size(385, 435);
            trackCanvasPanel.TabIndex = 0;
            trackCanvasPanel.BackColor = Color.FromArgb(32, 32, 32);
            // 
            // TrackEditorControl
            // 
            AutoScaleMode = AutoScaleMode.None;
            Controls.Add(splitContainer1);
            Name = "TrackEditorControl";
            Size = new Size(510, 435);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private SplitContainer splitContainer1;
        private Internal.TrackHeaderPanel trackHeaderPanel;
        private Internal.TrackCanvasPanel trackCanvasPanel;
    }
}