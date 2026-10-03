namespace Remcoposer64.App.form
{
    partial class frmTrackList
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
            lstTracks = new ListBox();
            SuspendLayout();
            // 
            // lstTracks
            // 
            lstTracks.Dock = DockStyle.Fill;
            lstTracks.FormattingEnabled = true;
            lstTracks.Location = new Point(0, 0);
            lstTracks.Name = "lstTracks";
            lstTracks.Size = new Size(800, 450);
            lstTracks.TabIndex = 0;
            // 
            // frmTrackList
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(lstTracks);
            Name = "frmTrackList";
            Text = "Track List";
            ResumeLayout(false);
        }

        #endregion

        private ListBox lstTracks;
    }
}