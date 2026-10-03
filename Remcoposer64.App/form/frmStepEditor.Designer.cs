namespace Remcoposer64.App.form
{
    partial class frmStepEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            stepEditorPanel1 = new Remcoposer64.StepEditorPanelControl.StepEditorPanel();
            toolStripContainer1 = new ToolStripContainer();
            statusStrip1 = new StatusStrip();
            tsslIsOverwrite = new ToolStripStatusLabel();
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // stepEditorPanel1
            // 
            stepEditorPanel1.Dock = DockStyle.Fill;
            stepEditorPanel1.Location = new Point(0, 0);
            stepEditorPanel1.Name = "stepEditorPanel1";
            stepEditorPanel1.OverwriteMode = false;
            stepEditorPanel1.Size = new Size(800, 403);
            stepEditorPanel1.TabIndex = 0;
            stepEditorPanel1.TabStop = true;
            stepEditorPanel1.UpdateFlg = false;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            toolStripContainer1.BottomToolStripPanel.Controls.Add(statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(stepEditorPanel1);
            toolStripContainer1.ContentPanel.Size = new Size(800, 403);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(800, 450);
            toolStripContainer1.TabIndex = 1;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = DockStyle.None;
            statusStrip1.Items.AddRange(new ToolStripItem[] { tsslIsOverwrite });
            statusStrip1.Location = new Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 22);
            statusStrip1.TabIndex = 0;
            // 
            // tsslIsOverwrite
            // 
            tsslIsOverwrite.Name = "tsslIsOverwrite";
            tsslIsOverwrite.Size = new Size(118, 17);
            tsslIsOverwrite.Text = "toolStripStatusLabel1";
            // 
            // frmStepEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(toolStripContainer1);
            Name = "frmStepEditor";
            Text = "Step Editor";
            Shown += frmStepEditor_Shown;
            toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer1.BottomToolStripPanel.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private StepEditorPanelControl.StepEditorPanel stepEditorPanel1;
        private ToolStripContainer toolStripContainer1;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel tsslIsOverwrite;
    }
}