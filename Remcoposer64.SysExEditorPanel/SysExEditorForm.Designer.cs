namespace Remcoposer64.SysExEditorPanel
{
    partial class SysExEditorForm
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
            TreeNode treeNode1 = new TreeNode("GM Reset");
            TreeNode treeNode2 = new TreeNode("GS Reset");
            TreeNode treeNode3 = new TreeNode("XG Reset");
            TreeNode treeNode4 = new TreeNode("Reset", new TreeNode[] { treeNode1, treeNode2, treeNode3 });
            TreeNode treeNode5 = new TreeNode("Sample SysEx 1");
            TreeNode treeNode6 = new TreeNode("Sample SysEx 2");
            TreeNode treeNode7 = new TreeNode("Sample SysEx 3");
            TreeNode treeNode8 = new TreeNode("Samples", new TreeNode[] { treeNode5, treeNode6, treeNode7 });
            TreeNode treeNode9 = new TreeNode("User SysEx");
            TreeNode treeNode10 = new TreeNode("SysEx", new TreeNode[] { treeNode4, treeNode8, treeNode9 });
            hexTextBox = new TextBox();
            label1 = new Label();
            button1 = new Button();
            splitContainer1 = new SplitContainer();
            treeView1 = new TreeView();
            contextMenuStrip1 = new ContextMenuStrip(components);
            addSysExToolStripMenuItem = new ToolStripMenuItem();
            deleteSysExToolStripMenuItem = new ToolStripMenuItem();
            setEventToolStripMenuItem = new ToolStripMenuItem();
            label2 = new Label();
            button5 = new Button();
            button4 = new Button();
            button3 = new Button();
            btnUpdate = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // hexTextBox
            // 
            hexTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            hexTextBox.Location = new Point(3, 32);
            hexTextBox.Multiline = true;
            hexTextBox.Name = "hexTextBox";
            hexTextBox.Size = new Size(360, 253);
            hexTextBox.TabIndex = 0;
            hexTextBox.Text = "F0 00 00 00 7F";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 6);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 2;
            label1.Text = "SysEx Name :";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button1.Location = new Point(5, 335);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "Set event";
            button1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeView1);
            splitContainer1.Panel1.Controls.Add(button1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label2);
            splitContainer1.Panel2.Controls.Add(hexTextBox);
            splitContainer1.Panel2.Controls.Add(button5);
            splitContainer1.Panel2.Controls.Add(button4);
            splitContainer1.Panel2.Controls.Add(button3);
            splitContainer1.Panel2.Controls.Add(btnUpdate);
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Size = new Size(544, 361);
            splitContainer1.SplitterDistance = 174;
            splitContainer1.TabIndex = 5;
            // 
            // treeView1
            // 
            treeView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            treeView1.ContextMenuStrip = contextMenuStrip1;
            treeView1.LabelEdit = true;
            treeView1.Location = new Point(3, 3);
            treeView1.Name = "treeView1";
            treeNode1.Name = "ノード4";
            treeNode1.Text = "GM Reset";
            treeNode2.Name = "ノード5";
            treeNode2.Text = "GS Reset";
            treeNode3.Name = "ノード6";
            treeNode3.Text = "XG Reset";
            treeNode4.Name = "ノード1";
            treeNode4.Text = "Reset";
            treeNode5.Name = "ノード7";
            treeNode5.Text = "Sample SysEx 1";
            treeNode6.Name = "ノード8";
            treeNode6.Text = "Sample SysEx 2";
            treeNode7.Name = "ノード9";
            treeNode7.Text = "Sample SysEx 3";
            treeNode8.Name = "ノード2";
            treeNode8.Text = "Samples";
            treeNode9.Name = "ノード3";
            treeNode9.Text = "User SysEx";
            treeNode10.Name = "ノード0";
            treeNode10.Text = "SysEx";

            treeNode1.Tag = new SysExItem { Name = "GM Reset", HexString = "F0 7E 7F 09 01 F7" };
            treeNode2.Tag = new SysExItem { Name = "GS Reset", HexString = "F0 41 00 42 12 40 00 7F 00 41 F7" };
            treeNode3.Tag = new SysExItem { Name = "XG Reset", HexString = "F0 43 10 4C 00 00 7E 00 F7" };

            treeNode5.Tag = new SysExItem { Name = "Sample SysEx 1", HexString = "F0 00 00 00 7F" };
            treeNode6.Tag = new SysExItem { Name = "Sample SysEx 2", HexString = "F0 00 00 00 01" };
            treeNode7.Tag = new SysExItem { Name = "Sample SysEx 3", HexString = "F0 00 00 00 02" };

            treeNode9.Tag = new SysExItem { Name = "User SysEx", HexString = "" };

            treeView1.Nodes.AddRange(new TreeNode[] { treeNode10 });
            treeView1.Size = new Size(167, 324);
            treeView1.TabIndex = 5;
            treeView1.AfterLabelEdit += treeView1_AfterLabelEdit;
            treeView1.AfterSelect += treeView1_AfterSelect;
            treeView1.NodeMouseClick += treeView1_NodeMouseClick;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { addSysExToolStripMenuItem, deleteSysExToolStripMenuItem, setEventToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(181, 92);
            contextMenuStrip1.Opening += contextMenuStrip1_Opening;
            // 
            // addSysExToolStripMenuItem
            // 
            addSysExToolStripMenuItem.Name = "addSysExToolStripMenuItem";
            addSysExToolStripMenuItem.Size = new Size(180, 22);
            addSysExToolStripMenuItem.Text = "Add SysEx";
            addSysExToolStripMenuItem.Click += OnAddSysEx;
            // 
            // deleteSysExToolStripMenuItem
            // 
            deleteSysExToolStripMenuItem.Name = "deleteSysExToolStripMenuItem";
            deleteSysExToolStripMenuItem.Size = new Size(180, 22);
            deleteSysExToolStripMenuItem.Text = "Delete SysEx";
            deleteSysExToolStripMenuItem.Click += OnDeleteSysEx;
            // 
            // setEventToolStripMenuItem
            // 
            setEventToolStripMenuItem.Name = "setEventToolStripMenuItem";
            setEventToolStripMenuItem.Size = new Size(180, 22);
            setEventToolStripMenuItem.Text = "Set event";
            setEventToolStripMenuItem.Click += OnSetEvent;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(85, 6);
            label2.Name = "label2";
            label2.Size = new Size(86, 15);
            label2.TabIndex = 5;
            label2.Text = "Sample SysEx 1";
            // 
            // button5
            // 
            button5.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button5.Location = new Point(233, 288);
            button5.Name = "button5";
            button5.Size = new Size(109, 23);
            button5.TabIndex = 4;
            button5.Text = "Check sum Value";
            button5.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            button4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button4.Location = new Point(118, 288);
            button4.Name = "button4";
            button4.Size = new Size(109, 23);
            button4.TabIndex = 4;
            button4.Text = "Check sum End";
            button4.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            button3.Location = new Point(3, 288);
            button3.Name = "button3";
            button3.Size = new Size(109, 23);
            button3.TabIndex = 4;
            button3.Text = "Check sum Start";
            button3.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnUpdate.Location = new Point(3, 335);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new Size(109, 23);
            btnUpdate.TabIndex = 4;
            btnUpdate.Text = "Update SysEx";
            btnUpdate.UseVisualStyleBackColor = true;
            btnUpdate.Click += btnUpdate_Click;
            // 
            // SysExEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(544, 361);
            Controls.Add(splitContainer1);
            MinimumSize = new Size(560, 400);
            Name = "SysExEditorForm";
            Text = "SysEx Editor";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TextBox hexTextBox;
        private Label label1;
        private Button button1;
        private SplitContainer splitContainer1;
        private Button btnUpdate;
        private TreeView treeView1;
        private Label label2;
        private Button button5;
        private Button button4;
        private Button button3;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem addSysExToolStripMenuItem;
        private ToolStripMenuItem deleteSysExToolStripMenuItem;
        private ToolStripMenuItem setEventToolStripMenuItem;
    }
}