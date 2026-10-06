namespace Remcoposer64.Test
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            lblStopwatch = new Label();
            lblDateTime = new Label();
            lblPerformanceCounter = new Label();
            lblRmCounter = new Label();
            txtMIDIFile = new TextBox();
            btnRef = new Button();
            btnPlay = new Button();
            cmbDevice = new ComboBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(69, 15);
            label1.TabIndex = 0;
            label1.Text = "Stopwatch :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 33);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 1;
            label2.Text = "DateTime :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 60);
            label3.Name = "label3";
            label3.Size = new Size(122, 15);
            label3.TabIndex = 2;
            label3.Text = "PerformanceCounter :";
            // 
            // lblStopwatch
            // 
            lblStopwatch.AutoSize = true;
            lblStopwatch.Location = new Point(140, 9);
            lblStopwatch.Name = "lblStopwatch";
            lblStopwatch.Size = new Size(38, 15);
            lblStopwatch.TabIndex = 0;
            lblStopwatch.Text = "label1";
            // 
            // lblDateTime
            // 
            lblDateTime.AutoSize = true;
            lblDateTime.Location = new Point(140, 33);
            lblDateTime.Name = "lblDateTime";
            lblDateTime.Size = new Size(38, 15);
            lblDateTime.TabIndex = 1;
            lblDateTime.Text = "label2";
            // 
            // lblPerformanceCounter
            // 
            lblPerformanceCounter.AutoSize = true;
            lblPerformanceCounter.Location = new Point(140, 60);
            lblPerformanceCounter.Name = "lblPerformanceCounter";
            lblPerformanceCounter.Size = new Size(38, 15);
            lblPerformanceCounter.TabIndex = 2;
            lblPerformanceCounter.Text = "label3";
            // 
            // lblRmCounter
            // 
            lblRmCounter.AutoSize = true;
            lblRmCounter.Location = new Point(140, 150);
            lblRmCounter.Name = "lblRmCounter";
            lblRmCounter.Size = new Size(38, 15);
            lblRmCounter.TabIndex = 2;
            lblRmCounter.Text = "label3";
            // 
            // txtMIDIFile
            // 
            txtMIDIFile.Location = new Point(12, 113);
            txtMIDIFile.Name = "txtMIDIFile";
            txtMIDIFile.Size = new Size(332, 23);
            txtMIDIFile.TabIndex = 3;
            txtMIDIFile.Text = "E:\\FM音源\\data\\MIDI\\ff9_fild.mid";
            // 
            // btnRef
            // 
            btnRef.Location = new Point(350, 113);
            btnRef.Name = "btnRef";
            btnRef.Size = new Size(32, 23);
            btnRef.TabIndex = 4;
            btnRef.Text = "...";
            btnRef.UseVisualStyleBackColor = true;
            btnRef.Click += btnRef_Click;
            // 
            // btnPlay
            // 
            btnPlay.Location = new Point(307, 146);
            btnPlay.Name = "btnPlay";
            btnPlay.Size = new Size(75, 23);
            btnPlay.TabIndex = 5;
            btnPlay.Text = "Play";
            btnPlay.UseVisualStyleBackColor = true;
            btnPlay.Click += btnPlay_Click;
            // 
            // cmbDevice
            // 
            cmbDevice.FormattingEnabled = true;
            cmbDevice.Location = new Point(13, 84);
            cmbDevice.Name = "cmbDevice";
            cmbDevice.Size = new Size(331, 23);
            cmbDevice.TabIndex = 6;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(394, 186);
            Controls.Add(cmbDevice);
            Controls.Add(btnPlay);
            Controls.Add(btnRef);
            Controls.Add(txtMIDIFile);
            Controls.Add(lblRmCounter);
            Controls.Add(lblPerformanceCounter);
            Controls.Add(label3);
            Controls.Add(lblDateTime);
            Controls.Add(label2);
            Controls.Add(lblStopwatch);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            FormClosed += Form1_FormClosed;
            Load += Form1_Load;
            Shown += Form1_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label lblStopwatch;
        private Label lblDateTime;
        private Label lblPerformanceCounter;
        private Label lblRmCounter;
        private TextBox txtMIDIFile;
        private Button btnRef;
        private Button btnPlay;
        private ComboBox cmbDevice;
    }
}
