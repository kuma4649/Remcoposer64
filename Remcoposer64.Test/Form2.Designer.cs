namespace Remcoposer64.Test
{
    partial class Form2
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
            trackEditorControl1 = new Remcoposer64.TrackEditor.TrackEditorControl();
            button1 = new Button();
            SuspendLayout();
            // 
            // trackEditorControl1
            // 
            trackEditorControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            trackEditorControl1.BackColor = Color.FromArgb(40, 40, 40);
            trackEditorControl1.Location = new Point(0, 29);
            trackEditorControl1.Name = "trackEditorControl1";
            trackEditorControl1.Size = new Size(800, 421);
            trackEditorControl1.TabIndex = 0;
            trackEditorControl1.TrackDoubleClicked += trackEditorControl1_TrackDoubleClicked;
            // 
            // button1
            // 
            button1.Location = new Point(12, 0);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(button1);
            Controls.Add(trackEditorControl1);
            Name = "Form2";
            Text = "Form2";
            Shown += Form2_Shown;
            ResumeLayout(false);
        }

        #endregion

        private TrackEditor.TrackEditorControl trackEditorControl1;
        private Button button1;
    }
}