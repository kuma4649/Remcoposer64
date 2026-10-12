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
            tsmiNewSequence = new ToolStripMenuItem();
            tsmiOpenSequenceFile = new ToolStripMenuItem();
            tsmiSaveSequenceFile = new ToolStripMenuItem();
            tsmiSaveAs = new ToolStripMenuItem();
            tsmiRecentFiles = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            tsmiExport = new ToolStripMenuItem();
            tsmiImport = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tsmiExit = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            //tsmiPlay_ = new ToolStripMenuItem();
            toolToolStripMenuItem = new ToolStripMenuItem();
            tsmiSetting = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            tsmiOpenOpePanel = new ToolStripMenuItem();
            tsmiOpenInformation = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            tsmiShowConsole = new ToolStripMenuItem();
            tabControl1 = new TabControl();
            toolStripContainer2 = new ToolStripContainer();
            transportToolStripMenuItem = new ToolStripMenuItem();
            tsmiPlay = new ToolStripMenuItem();
            tsmiStop = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            toolStripContainer2.ContentPanel.SuspendLayout();
            toolStripContainer2.TopToolStripPanel.SuspendLayout();
            toolStripContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Dock = DockStyle.None;
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, editToolStripMenuItem, transportToolStripMenuItem, toolToolStripMenuItem, windowToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(800, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiNewSequence, tsmiOpenSequenceFile, tsmiSaveSequenceFile, tsmiSaveAs, tsmiRecentFiles, toolStripSeparator1, tsmiExport, tsmiImport, toolStripSeparator2, tsmiExit });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // tsmiNewSequence
            // 
            tsmiNewSequence.Name = "tsmiNewSequence";
            tsmiNewSequence.Size = new Size(175, 22);
            tsmiNewSequence.Text = "New Sequence file";
            // 
            // tsmiOpenSequenceFile
            // 
            tsmiOpenSequenceFile.Name = "tsmiOpenSequenceFile";
            tsmiOpenSequenceFile.Size = new Size(175, 22);
            tsmiOpenSequenceFile.Text = "Open Sequence file";
            tsmiOpenSequenceFile.Click += tsmiOpenSequenceFile_Click;
            // 
            // tsmiSaveSequenceFile
            // 
            tsmiSaveSequenceFile.Name = "tsmiSaveSequenceFile";
            tsmiSaveSequenceFile.Size = new Size(175, 22);
            tsmiSaveSequenceFile.Text = "Save Sequence file";
            tsmiSaveSequenceFile.Click += tsmiSaveSequenceFile_Click;
            // 
            // tsmiSaveAs
            // 
            tsmiSaveAs.Name = "tsmiSaveAs";
            tsmiSaveAs.Size = new Size(175, 22);
            tsmiSaveAs.Text = "Save As";
            // 
            // tsmiRecentFiles
            // 
            tsmiRecentFiles.Name = "tsmiRecentFiles";
            tsmiRecentFiles.Size = new Size(175, 22);
            tsmiRecentFiles.Text = "Recent files";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(172, 6);
            // 
            // tsmiExport
            // 
            tsmiExport.Name = "tsmiExport";
            tsmiExport.Size = new Size(175, 22);
            tsmiExport.Text = "Export";
            // 
            // tsmiImport
            // 
            tsmiImport.Name = "tsmiImport";
            tsmiImport.Size = new Size(175, 22);
            tsmiImport.Text = "Import";
            tsmiImport.Click += tsmiImport_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(172, 6);
            // 
            // tsmiExit
            // 
            tsmiExit.Name = "tsmiExit";
            tsmiExit.Size = new Size(175, 22);
            tsmiExit.Text = "&Exit";
            tsmiExit.Click += tsmiExit_Click;
            // 
            // editToolStripMenuItem
            // 
            //editToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiPlay_ });
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "&Edit";
             
            // tsmiPlay_
             
            //tsmiPlay_.Name = "tsmiPlay_";
            //tsmiPlay_.Size = new Size(96, 22);
            //tsmiPlay_.Text = "&Play";
            //tsmiPlay_.Click += tsmiPlay_Click;
            // 
            // toolToolStripMenuItem
            // 
            toolToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiSetting });
            toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            toolToolStripMenuItem.Size = new Size(41, 20);
            toolToolStripMenuItem.Text = "&Tool";
            // 
            // tsmiSetting
            // 
            tsmiSetting.Name = "tsmiSetting";
            tsmiSetting.Size = new Size(180, 22);
            tsmiSetting.Text = "Setting";
            tsmiSetting.Click += tsmiSetting_Click;
            // 
            // windowToolStripMenuItem
            // 
            windowToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiOpenOpePanel, tsmiOpenInformation });
            windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            windowToolStripMenuItem.Size = new Size(63, 20);
            windowToolStripMenuItem.Text = "&Window";
            // 
            // tsmiOpenOpePanel
            // 
            tsmiOpenOpePanel.Name = "tsmiOpenOpePanel";
            tsmiOpenOpePanel.Size = new Size(180, 22);
            tsmiOpenOpePanel.Text = "Open OpePanel";
            tsmiOpenOpePanel.Click += tsmiOpenOpePanel_Click;
            // 
            // tsmiOpenInformation
            // 
            tsmiOpenInformation.Name = "tsmiOpenInformation";
            tsmiOpenInformation.Size = new Size(180, 22);
            tsmiOpenInformation.Text = "Open Information";
            tsmiOpenInformation.Click += tsmiOpenInformation_Click;
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
            tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
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
            // transportToolStripMenuItem
            // 
            transportToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiPlay, tsmiStop });
            transportToolStripMenuItem.Name = "transportToolStripMenuItem";
            transportToolStripMenuItem.Size = new Size(68, 20);
            transportToolStripMenuItem.Text = "Transport";
            // 
            // tsmiPlay
            // 
            tsmiPlay.Name = "tsmiPlay";
            tsmiPlay.ShortcutKeys = Keys.F5;
            tsmiPlay.Size = new Size(180, 22);
            tsmiPlay.Text = "&Play";
            tsmiPlay.Click += tsmiPlay_Click;
            // 
            // tsmiStop
            // 
            tsmiStop.Name = "tsmiStop";
            tsmiStop.ShortcutKeys = Keys.F10;
            tsmiStop.Size = new Size(180, 22);
            tsmiStop.Text = "&Stop";
            tsmiStop.Click += tsmiStop_Click;
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
        private ToolStripMenuItem tsmiNewSequence;
        private ToolStripMenuItem tsmiOpenSequenceFile;
        private ToolStripMenuItem tsmiSaveSequenceFile;
        private ToolStripMenuItem tsmiSaveAs;
        private ToolStripMenuItem tsmiRecentFiles;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem tsmiExit;
        private ToolStripMenuItem tsmiExport;
        private ToolStripMenuItem tsmiImport;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem tsmiShowConsole;
        private TabControl tabControl1;
        private ToolStripContainer toolStripContainer2;
        private ToolStripMenuItem tsmiSetting;
        private ToolStripMenuItem editToolStripMenuItem;
        //private ToolStripMenuItem tsmiPlay_;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem tsmiOpenOpePanel;
        private ToolStripMenuItem tsmiOpenInformation;
        private ToolStripMenuItem transportToolStripMenuItem;
        private ToolStripMenuItem tsmiPlay;
        private ToolStripMenuItem tsmiStop;
    }
}
