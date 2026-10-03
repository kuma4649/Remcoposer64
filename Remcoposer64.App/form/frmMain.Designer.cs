namespace Remcoposer64.App
{
    partial class frmMain
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
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            newSequenceToolStripMenuItem = new ToolStripMenuItem();
            openSequenceFileToolStripMenuItem = new ToolStripMenuItem();
            saveSequenceFileToolStripMenuItem = new ToolStripMenuItem();
            saveAsToolStripMenuItem = new ToolStripMenuItem();
            recentFilesToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            exportToolStripMenuItem = new ToolStripMenuItem();
            tsmiImport = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            toolToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            tsmiShowConsole = new ToolStripMenuItem();
            tabControl1 = new TabControl();
            toolStripContainer2 = new ToolStripContainer();
            menuStrip1.SuspendLayout();
            toolStripContainer2.ContentPanel.SuspendLayout();
            toolStripContainer2.TopToolStripPanel.SuspendLayout();
            toolStripContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Dock = DockStyle.None;
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, toolToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { newSequenceToolStripMenuItem, openSequenceFileToolStripMenuItem, saveSequenceFileToolStripMenuItem, saveAsToolStripMenuItem, recentFilesToolStripMenuItem, toolStripSeparator1, exportToolStripMenuItem, tsmiImport, toolStripSeparator2, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // newSequenceToolStripMenuItem
            // 
            newSequenceToolStripMenuItem.Name = "newSequenceToolStripMenuItem";
            newSequenceToolStripMenuItem.Size = new Size(184, 22);
            newSequenceToolStripMenuItem.Text = "Open New Sequence";
            // 
            // openSequenceFileToolStripMenuItem
            // 
            openSequenceFileToolStripMenuItem.Name = "openSequenceFileToolStripMenuItem";
            openSequenceFileToolStripMenuItem.Size = new Size(184, 22);
            openSequenceFileToolStripMenuItem.Text = "Open Sequence file";
            // 
            // saveSequenceFileToolStripMenuItem
            // 
            saveSequenceFileToolStripMenuItem.Name = "saveSequenceFileToolStripMenuItem";
            saveSequenceFileToolStripMenuItem.Size = new Size(184, 22);
            saveSequenceFileToolStripMenuItem.Text = "Save Sequence file";
            // 
            // saveAsToolStripMenuItem
            // 
            saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            saveAsToolStripMenuItem.Size = new Size(184, 22);
            saveAsToolStripMenuItem.Text = "Save As";
            // 
            // recentFilesToolStripMenuItem
            // 
            recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            recentFilesToolStripMenuItem.Size = new Size(184, 22);
            recentFilesToolStripMenuItem.Text = "Recent files";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(181, 6);
            // 
            // exportToolStripMenuItem
            // 
            exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            exportToolStripMenuItem.Size = new Size(184, 22);
            exportToolStripMenuItem.Text = "Export";
            // 
            // tsmiImport
            // 
            tsmiImport.Name = "tsmiImport";
            tsmiImport.Size = new Size(184, 22);
            tsmiImport.Text = "Import";
            tsmiImport.Click += tsmiImport_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(181, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(184, 22);
            exitToolStripMenuItem.Text = "&Exit";
            // 
            // toolToolStripMenuItem
            // 
            toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            toolToolStripMenuItem.Size = new Size(41, 20);
            toolToolStripMenuItem.Text = "&Tool";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiShowConsole });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // tsmiShowConsole
            // 
            tsmiShowConsole.Name = "tsmiShowConsole";
            tsmiShowConsole.Size = new Size(148, 22);
            tsmiShowConsole.Text = "Show Console";
            tsmiShowConsole.Click += tsmiShowConsole_Click;
            // 
            // tabControl1
            // 
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(800, 514);
            tabControl1.TabIndex = 1;
            // 
            // toolStripContainer2
            // 
            // 
            // toolStripContainer2.ContentPanel
            // 
            toolStripContainer2.ContentPanel.Controls.Add(tabControl1);
            toolStripContainer2.ContentPanel.Size = new Size(800, 514);
            toolStripContainer2.Dock = DockStyle.Fill;
            toolStripContainer2.Location = new Point(0, 0);
            toolStripContainer2.Name = "toolStripContainer2";
            toolStripContainer2.Size = new Size(800, 538);
            toolStripContainer2.TabIndex = 3;
            toolStripContainer2.Text = "toolStripContainer2";
            // 
            // toolStripContainer2.TopToolStripPanel
            // 
            toolStripContainer2.TopToolStripPanel.Controls.Add(menuStrip1);
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 538);
            Controls.Add(toolStripContainer2);
            MainMenuStrip = menuStrip1;
            Name = "frmMain";
            Text = "Remcoposer64";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStripContainer2.ContentPanel.ResumeLayout(false);
            toolStripContainer2.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer2.TopToolStripPanel.PerformLayout();
            toolStripContainer2.ResumeLayout(false);
            toolStripContainer2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem toolToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem newSequenceToolStripMenuItem;
        private ToolStripMenuItem openSequenceFileToolStripMenuItem;
        private ToolStripMenuItem saveSequenceFileToolStripMenuItem;
        private ToolStripMenuItem saveAsToolStripMenuItem;
        private ToolStripMenuItem recentFilesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem exportToolStripMenuItem;
        private ToolStripMenuItem tsmiImport;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem tsmiShowConsole;
        private TabControl tabControl1;
        private ToolStripContainer toolStripContainer2;
    }
}
