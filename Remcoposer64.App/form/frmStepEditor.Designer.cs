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
            SuspendLayout();
            // 
            // stepEditorPanel1
            // 
            stepEditorPanel1.Dock = DockStyle.Fill;
            stepEditorPanel1.Location = new Point(0, 0);
            stepEditorPanel1.Name = "stepEditorPanel1";
            stepEditorPanel1.OverwriteMode = false;
            stepEditorPanel1.Size = new Size(800, 450);
            stepEditorPanel1.TabIndex = 0;
            stepEditorPanel1.TabStop = true;
            // 
            // frmStepEditor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(stepEditorPanel1);
            Name = "frmStepEditor";
            Text = "Step Editor";
            Shown += frmStepEditor_Shown;
            ResumeLayout(false);
        }

        #endregion

        private StepEditorPanelControl.StepEditorPanel stepEditorPanel1;
    }
}