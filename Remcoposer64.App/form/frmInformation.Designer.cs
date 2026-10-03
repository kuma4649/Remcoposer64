namespace Remcoposer64.App.form
{
    partial class frmInformation
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
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            txtFileName = new TextBox();
            txtTitle = new TextBox();
            txtTempo = new TextBox();
            txtTimeBase = new TextBox();
            txtBeat = new TextBox();
            txtCopyright = new TextBox();
            label7 = new Label();
            txtBeat2 = new TextBox();
            label8 = new Label();
            txtKey = new TextBox();
            label9 = new Label();
            txtMemo = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 19);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 0;
            label1.Text = "FileName :";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 48);
            label2.Name = "label2";
            label2.Size = new Size(35, 15);
            label2.TabIndex = 1;
            label2.Text = "Title :";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 77);
            label3.Name = "label3";
            label3.Size = new Size(48, 15);
            label3.TabIndex = 2;
            label3.Text = "Tempo :";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(161, 77);
            label4.Name = "label4";
            label4.Size = new Size(62, 15);
            label4.TabIndex = 3;
            label4.Text = "TimeBase :";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(311, 77);
            label5.Name = "label5";
            label5.Size = new Size(36, 15);
            label5.TabIndex = 4;
            label5.Text = "Beat :";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(12, 106);
            label6.Name = "label6";
            label6.Size = new Size(65, 15);
            label6.TabIndex = 5;
            label6.Text = "Copyright :";
            // 
            // txtFileName
            // 
            txtFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFileName.Location = new Point(80, 16);
            txtFileName.Name = "txtFileName";
            txtFileName.Size = new Size(532, 23);
            txtFileName.TabIndex = 6;
            // 
            // txtTitle
            // 
            txtTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtTitle.Location = new Point(80, 45);
            txtTitle.Name = "txtTitle";
            txtTitle.Size = new Size(532, 23);
            txtTitle.TabIndex = 6;
            // 
            // txtTempo
            // 
            txtTempo.Location = new Point(80, 74);
            txtTempo.Name = "txtTempo";
            txtTempo.Size = new Size(57, 23);
            txtTempo.TabIndex = 6;
            // 
            // txtTimeBase
            // 
            txtTimeBase.Location = new Point(229, 74);
            txtTimeBase.Name = "txtTimeBase";
            txtTimeBase.Size = new Size(57, 23);
            txtTimeBase.TabIndex = 6;
            // 
            // txtBeat
            // 
            txtBeat.Location = new Point(353, 74);
            txtBeat.Name = "txtBeat";
            txtBeat.Size = new Size(57, 23);
            txtBeat.TabIndex = 6;
            // 
            // txtCopyright
            // 
            txtCopyright.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtCopyright.Location = new Point(80, 103);
            txtCopyright.Name = "txtCopyright";
            txtCopyright.Size = new Size(532, 23);
            txtCopyright.TabIndex = 6;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(416, 77);
            label7.Name = "label7";
            label7.Size = new Size(12, 15);
            label7.TabIndex = 4;
            label7.Text = "/";
            // 
            // txtBeat2
            // 
            txtBeat2.Location = new Point(434, 74);
            txtBeat2.Name = "txtBeat2";
            txtBeat2.Size = new Size(57, 23);
            txtBeat2.TabIndex = 6;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(517, 77);
            label8.Name = "label8";
            label8.Size = new Size(32, 15);
            label8.TabIndex = 2;
            label8.Text = "Key :";
            // 
            // txtKey
            // 
            txtKey.Location = new Point(555, 74);
            txtKey.Name = "txtKey";
            txtKey.Size = new Size(57, 23);
            txtKey.TabIndex = 6;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(12, 138);
            label9.Name = "label9";
            label9.Size = new Size(47, 15);
            label9.TabIndex = 5;
            label9.Text = "Memo :";
            // 
            // txtMemo
            // 
            txtMemo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtMemo.Location = new Point(80, 153);
            txtMemo.Multiline = true;
            txtMemo.Name = "txtMemo";
            txtMemo.Size = new Size(532, 276);
            txtMemo.TabIndex = 6;
            // 
            // frmInformation
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(624, 441);
            Controls.Add(txtMemo);
            Controls.Add(txtCopyright);
            Controls.Add(txtBeat2);
            Controls.Add(txtBeat);
            Controls.Add(txtTimeBase);
            Controls.Add(txtKey);
            Controls.Add(txtTempo);
            Controls.Add(txtTitle);
            Controls.Add(txtFileName);
            Controls.Add(label9);
            Controls.Add(label6);
            Controls.Add(label7);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label8);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            MinimumSize = new Size(640, 480);
            Name = "frmInformation";
            Text = "Information";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox txtFileName;
        private TextBox txtTitle;
        private TextBox txtTempo;
        private TextBox txtTimeBase;
        private TextBox txtBeat;
        private TextBox txtCopyright;
        private Label label7;
        private TextBox txtBeat2;
        private Label label8;
        private TextBox txtKey;
        private Label label9;
        private TextBox txtMemo;
    }
}