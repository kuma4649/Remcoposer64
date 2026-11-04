namespace Remcoposer64.App.form
{
    partial class frmOpePanel
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
            components = new System.ComponentModel.Container();
            btFirst = new Button();
            btPlay = new Button();
            btStop = new Button();
            btFF = new Button();
            groupBox1 = new GroupBox();
            lblLocation = new Label();
            groupBox2 = new GroupBox();
            lblTempo = new Label();
            groupBox3 = new GroupBox();
            lblTime = new Label();
            groupBox4 = new GroupBox();
            lblBeat = new Label();
            groupBox5 = new GroupBox();
            bt10 = new Button();
            bt9 = new Button();
            bt8 = new Button();
            bt7 = new Button();
            bt6 = new Button();
            bt5 = new Button();
            bt4 = new Button();
            bt3 = new Button();
            bt2 = new Button();
            bt1 = new Button();
            timerUpdate = new System.Windows.Forms.Timer(components);
            cbFF = new CheckBox();
            cbSlow = new CheckBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            groupBox4.SuspendLayout();
            groupBox5.SuspendLayout();
            SuspendLayout();
            // 
            // btFirst
            // 
            btFirst.Location = new Point(6, 4);
            btFirst.Name = "btFirst";
            btFirst.Size = new Size(36, 32);
            btFirst.TabIndex = 0;
            btFirst.Text = "|<";
            btFirst.UseVisualStyleBackColor = true;
            btFirst.Click += btRewind_Click;
            // 
            // btPlay
            // 
            btPlay.Location = new Point(82, 4);
            btPlay.Name = "btPlay";
            btPlay.Size = new Size(52, 32);
            btPlay.TabIndex = 1;
            btPlay.Text = ">";
            btPlay.UseVisualStyleBackColor = true;
            btPlay.Click += btPlay_Click;
            // 
            // btStop
            // 
            btStop.Location = new Point(173, 4);
            btStop.Name = "btStop";
            btStop.Size = new Size(37, 32);
            btStop.TabIndex = 2;
            btStop.Text = "■";
            btStop.UseVisualStyleBackColor = true;
            btStop.Click += btStop_Click;
            // 
            // btFF
            // 
            btFF.Location = new Point(413, 4);
            btFF.Name = "btFF";
            btFF.Size = new Size(36, 32);
            btFF.TabIndex = 3;
            btFF.Text = ">>";
            btFF.UseVisualStyleBackColor = true;
            btFF.Visible = false;
            btFF.Click += btFF_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblLocation);
            groupBox1.Location = new Point(216, 4);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(101, 34);
            groupBox1.TabIndex = 4;
            groupBox1.TabStop = false;
            groupBox1.Text = "Location";
            // 
            // lblLocation
            // 
            lblLocation.AutoSize = true;
            lblLocation.Location = new Point(11, 14);
            lblLocation.Name = "lblLocation";
            lblLocation.Size = new Size(73, 15);
            lblLocation.TabIndex = 0;
            lblLocation.Text = "0000:000:000";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblTempo);
            groupBox2.Location = new Point(321, 4);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(64, 34);
            groupBox2.TabIndex = 4;
            groupBox2.TabStop = false;
            groupBox2.Text = "Tempo";
            // 
            // lblTempo
            // 
            lblTempo.AutoSize = true;
            lblTempo.Location = new Point(7, 13);
            lblTempo.Name = "lblTempo";
            lblTempo.Size = new Size(48, 15);
            lblTempo.TabIndex = 2;
            lblTempo.Text = "000/000";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(lblTime);
            groupBox3.Location = new Point(216, 40);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(101, 34);
            groupBox3.TabIndex = 4;
            groupBox3.TabStop = false;
            groupBox3.Text = "Time";
            // 
            // lblTime
            // 
            lblTime.AutoSize = true;
            lblTime.Location = new Point(11, 14);
            lblTime.Name = "lblTime";
            lblTime.Size = new Size(82, 15);
            lblTime.TabIndex = 1;
            lblTime.Text = "0000:00:00.000";
            // 
            // groupBox4
            // 
            groupBox4.Controls.Add(lblBeat);
            groupBox4.Location = new Point(321, 40);
            groupBox4.Name = "groupBox4";
            groupBox4.Size = new Size(64, 34);
            groupBox4.TabIndex = 4;
            groupBox4.TabStop = false;
            groupBox4.Text = "Beat";
            // 
            // lblBeat
            // 
            lblBeat.AutoSize = true;
            lblBeat.Location = new Point(8, 14);
            lblBeat.Name = "lblBeat";
            lblBeat.Size = new Size(48, 15);
            lblBeat.TabIndex = 3;
            lblBeat.Text = "000/000";
            // 
            // groupBox5
            // 
            groupBox5.Controls.Add(bt10);
            groupBox5.Controls.Add(bt9);
            groupBox5.Controls.Add(bt8);
            groupBox5.Controls.Add(bt7);
            groupBox5.Controls.Add(bt6);
            groupBox5.Controls.Add(bt5);
            groupBox5.Controls.Add(bt4);
            groupBox5.Controls.Add(bt3);
            groupBox5.Controls.Add(bt2);
            groupBox5.Controls.Add(bt1);
            groupBox5.Location = new Point(5, 41);
            groupBox5.Name = "groupBox5";
            groupBox5.Size = new Size(205, 34);
            groupBox5.TabIndex = 4;
            groupBox5.TabStop = false;
            groupBox5.Text = "Maker";
            // 
            // bt10
            // 
            bt10.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt10.Location = new Point(182, 12);
            bt10.Name = "bt10";
            bt10.Size = new Size(20, 20);
            bt10.TabIndex = 8;
            bt10.Text = "A";
            bt10.UseVisualStyleBackColor = true;
            // 
            // bt9
            // 
            bt9.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt9.Location = new Point(163, 12);
            bt9.Name = "bt9";
            bt9.Size = new Size(20, 20);
            bt9.TabIndex = 7;
            bt9.Text = "9";
            bt9.UseVisualStyleBackColor = true;
            // 
            // bt8
            // 
            bt8.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt8.Location = new Point(143, 12);
            bt8.Name = "bt8";
            bt8.Size = new Size(20, 20);
            bt8.TabIndex = 5;
            bt8.Text = "8";
            bt8.UseVisualStyleBackColor = true;
            bt8.Click += bt8_Click;
            // 
            // bt7
            // 
            bt7.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt7.Location = new Point(123, 12);
            bt7.Name = "bt7";
            bt7.Size = new Size(20, 20);
            bt7.TabIndex = 6;
            bt7.Text = "7";
            bt7.UseVisualStyleBackColor = true;
            bt7.Click += bt7_Click;
            // 
            // bt6
            // 
            bt6.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt6.Location = new Point(103, 12);
            bt6.Name = "bt6";
            bt6.Size = new Size(20, 20);
            bt6.TabIndex = 3;
            bt6.Text = "6";
            bt6.UseVisualStyleBackColor = true;
            bt6.Click += bt6_Click;
            // 
            // bt5
            // 
            bt5.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt5.Location = new Point(83, 12);
            bt5.Name = "bt5";
            bt5.Size = new Size(20, 20);
            bt5.TabIndex = 4;
            bt5.Text = "5";
            bt5.UseVisualStyleBackColor = true;
            bt5.Click += bt5_Click;
            // 
            // bt4
            // 
            bt4.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt4.Location = new Point(63, 12);
            bt4.Name = "bt4";
            bt4.Size = new Size(20, 20);
            bt4.TabIndex = 1;
            bt4.Text = "4";
            bt4.UseVisualStyleBackColor = true;
            bt4.Click += bt4_Click;
            // 
            // bt3
            // 
            bt3.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt3.Location = new Point(43, 12);
            bt3.Name = "bt3";
            bt3.Size = new Size(20, 20);
            bt3.TabIndex = 2;
            bt3.Text = "3";
            bt3.UseVisualStyleBackColor = true;
            bt3.Click += bt3_Click;
            // 
            // bt2
            // 
            bt2.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt2.Location = new Point(23, 12);
            bt2.Name = "bt2";
            bt2.Size = new Size(20, 20);
            bt2.TabIndex = 0;
            bt2.Text = "2";
            bt2.UseVisualStyleBackColor = true;
            bt2.Click += bt2_Click;
            // 
            // bt1
            // 
            bt1.Font = new Font("Yu Gothic UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 128);
            bt1.Location = new Point(3, 12);
            bt1.Name = "bt1";
            bt1.Size = new Size(20, 20);
            bt1.TabIndex = 0;
            bt1.Text = "1";
            bt1.UseVisualStyleBackColor = true;
            bt1.Click += bt1_Click;
            // 
            // timerUpdate
            // 
            timerUpdate.Enabled = true;
            timerUpdate.Interval = 10;
            timerUpdate.Tick += timerUpdate_Tick;
            // 
            // cbFF
            // 
            cbFF.Appearance = Appearance.Button;
            cbFF.Location = new Point(135, 4);
            cbFF.Name = "cbFF";
            cbFF.Size = new Size(37, 32);
            cbFF.TabIndex = 5;
            cbFF.Text = ">>";
            cbFF.TextAlign = ContentAlignment.MiddleCenter;
            cbFF.UseVisualStyleBackColor = true;
            cbFF.CheckedChanged += cbFF_CheckedChanged;
            // 
            // cbSlow
            // 
            cbSlow.Appearance = Appearance.Button;
            cbSlow.Location = new Point(44, 4);
            cbSlow.Name = "cbSlow";
            cbSlow.Size = new Size(37, 32);
            cbSlow.TabIndex = 6;
            cbSlow.Text = "|>";
            cbSlow.TextAlign = ContentAlignment.MiddleCenter;
            cbSlow.UseVisualStyleBackColor = true;
            cbSlow.CheckedChanged += cbSlow_CheckedChanged;
            // 
            // frmOpePanel
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(394, 82);
            Controls.Add(cbSlow);
            Controls.Add(cbFF);
            Controls.Add(groupBox4);
            Controls.Add(groupBox2);
            Controls.Add(groupBox5);
            Controls.Add(groupBox3);
            Controls.Add(groupBox1);
            Controls.Add(btFF);
            Controls.Add(btStop);
            Controls.Add(btPlay);
            Controls.Add(btFirst);
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Name = "frmOpePanel";
            StartPosition = FormStartPosition.Manual;
            Text = "Ope Panel";
            FormClosing += frmOpePanel_FormClosing;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            groupBox4.ResumeLayout(false);
            groupBox4.PerformLayout();
            groupBox5.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btFirst;
        private Button btPlay;
        private Button btStop;
        private Button btFF;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private Button bt1;
        private Label lblLocation;
        private Label lblTime;
        private Button bt2;
        private Label lblTempo;
        private Label lblBeat;
        private Button bt8;
        private Button bt7;
        private Button bt6;
        private Button bt5;
        private Button bt4;
        private Button bt3;
        private System.Windows.Forms.Timer timerUpdate;
        private CheckBox cbFF;
        private Button bt10;
        private Button bt9;
        private CheckBox cbSlow;
    }
}