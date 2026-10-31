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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            tsmiNewSequence = new ToolStripMenuItem();
            tsmiOpenSequenceFile = new ToolStripMenuItem();
            tsmiSaveSequenceFile = new ToolStripMenuItem();
            tsmiSaveAs = new ToolStripMenuItem();
            tsmiCloseSequenceFile = new ToolStripMenuItem();
            tsmiRecentFiles = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            tsmiExport = new ToolStripMenuItem();
            tsmiImport = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            tsmiExit = new ToolStripMenuItem();
            editToolStripMenuItem = new ToolStripMenuItem();
            transportToolStripMenuItem = new ToolStripMenuItem();
            tsmiPlay = new ToolStripMenuItem();
            tsmiStop = new ToolStripMenuItem();
            toolToolStripMenuItem = new ToolStripMenuItem();
            tsmiSetting = new ToolStripMenuItem();
            windowToolStripMenuItem = new ToolStripMenuItem();
            tsmiOpenOpePanel = new ToolStripMenuItem();
            tsmiOpenInformation = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            tsmiShowConsole = new ToolStripMenuItem();
            statusStrip1 = new StatusStrip();
            toolStrip1 = new ToolStrip();
            toolStripLabel1 = new ToolStripLabel();
            tsbMIDIOut_A = new ToolStripButton();
            tsbMIDIOut_B = new ToolStripButton();
            tsbMIDIOut_C = new ToolStripButton();
            tsbMIDIOut_D = new ToolStripButton();
            tsbMIDIOut_E = new ToolStripButton();
            tsbMIDIOut_F = new ToolStripButton();
            tsbMIDIOut_G = new ToolStripButton();
            dockPanel1 = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            vS2015DarkTheme1 = new WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme();
            visualStudioToolStripExtender1 = new WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(components);
            toolStripContainer1 = new ToolStripContainer();
            toolStrip2 = new ToolStrip();
            toolStripLabel2 = new ToolStripLabel();
            tsbTrackRow1 = new ToolStripButton();
            tsbTrackRow2 = new ToolStripButton();
            tsbTrackRow3 = new ToolStripButton();
            toolStrip3 = new ToolStrip();
            toolStripLabel3 = new ToolStripLabel();
            tsbSEFont8 = new ToolStripButton();
            tsbSEFont12 = new ToolStripButton();
            tsbSEFont16 = new ToolStripButton();
            timerScreen = new System.Windows.Forms.Timer(components);
            menuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            toolStrip2.SuspendLayout();
            toolStrip3.SuspendLayout();
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
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { tsmiNewSequence, tsmiOpenSequenceFile, tsmiSaveSequenceFile, tsmiSaveAs, tsmiCloseSequenceFile, tsmiRecentFiles, toolStripSeparator1, tsmiExport, tsmiImport, toolStripSeparator2, tsmiExit });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            fileToolStripMenuItem.DropDownOpening += fileToolStripMenuItem_DropDownOpening;
            // 
            // tsmiNewSequence
            // 
            tsmiNewSequence.Name = "tsmiNewSequence";
            tsmiNewSequence.Size = new Size(180, 22);
            tsmiNewSequence.Text = "New Sequence file";
            // 
            // tsmiOpenSequenceFile
            // 
            tsmiOpenSequenceFile.Name = "tsmiOpenSequenceFile";
            tsmiOpenSequenceFile.Size = new Size(180, 22);
            tsmiOpenSequenceFile.Text = "Open Sequence file";
            tsmiOpenSequenceFile.Click += tsmiOpenSequenceFile_Click;
            // 
            // tsmiSaveSequenceFile
            // 
            tsmiSaveSequenceFile.Name = "tsmiSaveSequenceFile";
            tsmiSaveSequenceFile.Size = new Size(180, 22);
            tsmiSaveSequenceFile.Text = "Save Sequence file";
            tsmiSaveSequenceFile.Click += tsmiSaveSequenceFile_Click;
            // 
            // tsmiSaveAs
            // 
            tsmiSaveAs.Name = "tsmiSaveAs";
            tsmiSaveAs.Size = new Size(180, 22);
            tsmiSaveAs.Text = "Save As";
            // 
            // tsmiCloseSequenceFile
            // 
            tsmiCloseSequenceFile.Name = "tsmiCloseSequenceFile";
            tsmiCloseSequenceFile.Size = new Size(180, 22);
            tsmiCloseSequenceFile.Text = "Close Sequence file";
            tsmiCloseSequenceFile.Click += tsmiCloseSequenceFile_Click;
            // 
            // tsmiRecentFiles
            // 
            tsmiRecentFiles.Name = "tsmiRecentFiles";
            tsmiRecentFiles.Size = new Size(180, 22);
            tsmiRecentFiles.Text = "Recent files";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // tsmiExport
            // 
            tsmiExport.Name = "tsmiExport";
            tsmiExport.Size = new Size(180, 22);
            tsmiExport.Text = "Export";
            // 
            // tsmiImport
            // 
            tsmiImport.Name = "tsmiImport";
            tsmiImport.Size = new Size(180, 22);
            tsmiImport.Text = "Import";
            tsmiImport.Click += tsmiImport_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(177, 6);
            // 
            // tsmiExit
            // 
            tsmiExit.Name = "tsmiExit";
            tsmiExit.Size = new Size(180, 22);
            tsmiExit.Text = "&Exit";
            tsmiExit.Click += tsmiExit_Click;
            // 
            // editToolStripMenuItem
            // 
            editToolStripMenuItem.Name = "editToolStripMenuItem";
            editToolStripMenuItem.Size = new Size(39, 20);
            editToolStripMenuItem.Text = "&Edit";
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
            tsmiPlay.Size = new Size(123, 22);
            tsmiPlay.Text = "&Play";
            tsmiPlay.Click += tsmiPlay_Click;
            // 
            // tsmiStop
            // 
            tsmiStop.Name = "tsmiStop";
            tsmiStop.ShortcutKeys = Keys.F10;
            tsmiStop.Size = new Size(123, 22);
            tsmiStop.Text = "&Stop";
            tsmiStop.Click += tsmiStop_Click;
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
            tsmiSetting.Size = new Size(111, 22);
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
            tsmiOpenOpePanel.Size = new Size(168, 22);
            tsmiOpenOpePanel.Text = "Open OpePanel";
            tsmiOpenOpePanel.Click += tsmiOpenOpePanel_Click;
            // 
            // tsmiOpenInformation
            // 
            tsmiOpenInformation.Name = "tsmiOpenInformation";
            tsmiOpenInformation.Size = new Size(168, 22);
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
            // statusStrip1
            // 
            statusStrip1.Dock = DockStyle.None;
            statusStrip1.Location = new Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(800, 22);
            statusStrip1.TabIndex = 0;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = DockStyle.None;
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripLabel1, tsbMIDIOut_A, tsbMIDIOut_B, tsbMIDIOut_C, tsbMIDIOut_D, tsbMIDIOut_E, tsbMIDIOut_F, tsbMIDIOut_G });
            toolStrip1.Location = new Point(3, 24);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(223, 25);
            toolStrip1.TabIndex = 1;
            // 
            // toolStripLabel1
            // 
            toolStripLabel1.Name = "toolStripLabel1";
            toolStripLabel1.Size = new Size(50, 22);
            toolStripLabel1.Text = "MIDIout";
            // 
            // tsbMIDIOut_A
            // 
            tsbMIDIOut_A.CheckOnClick = true;
            tsbMIDIOut_A.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_A.Image = (Image)resources.GetObject("tsbMIDIOut_A.Image");
            tsbMIDIOut_A.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_A.Name = "tsbMIDIOut_A";
            tsbMIDIOut_A.Size = new Size(23, 22);
            tsbMIDIOut_A.Text = "A";
            tsbMIDIOut_A.TextImageRelation = TextImageRelation.TextBeforeImage;
            tsbMIDIOut_A.Click += tsbMIDIOut_A_Click;
            // 
            // tsbMIDIOut_B
            // 
            tsbMIDIOut_B.CheckOnClick = true;
            tsbMIDIOut_B.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_B.Image = (Image)resources.GetObject("tsbMIDIOut_B.Image");
            tsbMIDIOut_B.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_B.Name = "tsbMIDIOut_B";
            tsbMIDIOut_B.Size = new Size(23, 22);
            tsbMIDIOut_B.Text = "B";
            tsbMIDIOut_B.Click += tsbMIDIOut_B_Click;
            // 
            // tsbMIDIOut_C
            // 
            tsbMIDIOut_C.CheckOnClick = true;
            tsbMIDIOut_C.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_C.Image = (Image)resources.GetObject("tsbMIDIOut_C.Image");
            tsbMIDIOut_C.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_C.Name = "tsbMIDIOut_C";
            tsbMIDIOut_C.Size = new Size(23, 22);
            tsbMIDIOut_C.Text = "C";
            tsbMIDIOut_C.Click += tsbMIDIOut_C_Click;
            // 
            // tsbMIDIOut_D
            // 
            tsbMIDIOut_D.CheckOnClick = true;
            tsbMIDIOut_D.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_D.Image = (Image)resources.GetObject("tsbMIDIOut_D.Image");
            tsbMIDIOut_D.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_D.Name = "tsbMIDIOut_D";
            tsbMIDIOut_D.Size = new Size(23, 22);
            tsbMIDIOut_D.Text = "D";
            tsbMIDIOut_D.Click += tsbMIDIOut_D_Click;
            // 
            // tsbMIDIOut_E
            // 
            tsbMIDIOut_E.CheckOnClick = true;
            tsbMIDIOut_E.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_E.Image = (Image)resources.GetObject("tsbMIDIOut_E.Image");
            tsbMIDIOut_E.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_E.Name = "tsbMIDIOut_E";
            tsbMIDIOut_E.Size = new Size(23, 22);
            tsbMIDIOut_E.Text = "E";
            tsbMIDIOut_E.Click += tsbMIDIOut_E_Click;
            // 
            // tsbMIDIOut_F
            // 
            tsbMIDIOut_F.CheckOnClick = true;
            tsbMIDIOut_F.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_F.Image = (Image)resources.GetObject("tsbMIDIOut_F.Image");
            tsbMIDIOut_F.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_F.Name = "tsbMIDIOut_F";
            tsbMIDIOut_F.Size = new Size(23, 22);
            tsbMIDIOut_F.Text = "F";
            tsbMIDIOut_F.Click += tsbMIDIOut_F_Click;
            // 
            // tsbMIDIOut_G
            // 
            tsbMIDIOut_G.CheckOnClick = true;
            tsbMIDIOut_G.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbMIDIOut_G.Image = (Image)resources.GetObject("tsbMIDIOut_G.Image");
            tsbMIDIOut_G.ImageTransparentColor = Color.Magenta;
            tsbMIDIOut_G.Name = "tsbMIDIOut_G";
            tsbMIDIOut_G.Size = new Size(23, 22);
            tsbMIDIOut_G.Text = "G";
            tsbMIDIOut_G.Click += tsbMIDIOut_G_Click;
            // 
            // dockPanel1
            // 
            dockPanel1.Dock = DockStyle.Fill;
            dockPanel1.Location = new Point(0, 0);
            dockPanel1.Name = "dockPanel1";
            dockPanel1.Size = new Size(800, 467);
            dockPanel1.TabIndex = 4;
            dockPanel1.ActiveDocumentChanged += dockPanel1_ActiveDocumentChanged;
            dockPanel1.ActiveContentChanged += dockPanel1_ActiveContentChanged;
            dockPanel1.DragDrop += dockPanel1_DragDrop;
            dockPanel1.DragEnter += dockPanel1_DragEnter;
            // 
            // visualStudioToolStripExtender1
            // 
            visualStudioToolStripExtender1.DefaultRenderer = null;
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
            toolStripContainer1.ContentPanel.Controls.Add(dockPanel1);
            toolStripContainer1.ContentPanel.Size = new Size(800, 467);
            toolStripContainer1.Dock = DockStyle.Fill;
            toolStripContainer1.Location = new Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new Size(800, 538);
            toolStripContainer1.TabIndex = 5;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(menuStrip1);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip2);
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip3);
            // 
            // toolStrip2
            // 
            toolStrip2.Dock = DockStyle.None;
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripLabel2, tsbTrackRow1, tsbTrackRow2, tsbTrackRow3 });
            toolStrip2.Location = new Point(239, 24);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(138, 25);
            toolStrip2.TabIndex = 2;
            // 
            // toolStripLabel2
            // 
            toolStripLabel2.Name = "toolStripLabel2";
            toolStripLabel2.Size = new Size(57, 22);
            toolStripLabel2.Text = "TrackRow";
            // 
            // tsbTrackRow1
            // 
            tsbTrackRow1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbTrackRow1.Image = (Image)resources.GetObject("tsbTrackRow1.Image");
            tsbTrackRow1.ImageTransparentColor = Color.Magenta;
            tsbTrackRow1.Name = "tsbTrackRow1";
            tsbTrackRow1.Size = new Size(23, 22);
            tsbTrackRow1.Text = "1";
            tsbTrackRow1.Click += tsbTrackRow1_Click;
            // 
            // tsbTrackRow2
            // 
            tsbTrackRow2.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbTrackRow2.Image = (Image)resources.GetObject("tsbTrackRow2.Image");
            tsbTrackRow2.ImageTransparentColor = Color.Magenta;
            tsbTrackRow2.Name = "tsbTrackRow2";
            tsbTrackRow2.Size = new Size(23, 22);
            tsbTrackRow2.Text = "2";
            tsbTrackRow2.Click += tsbTrackRow2_Click;
            // 
            // tsbTrackRow3
            // 
            tsbTrackRow3.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbTrackRow3.Image = (Image)resources.GetObject("tsbTrackRow3.Image");
            tsbTrackRow3.ImageTransparentColor = Color.Magenta;
            tsbTrackRow3.Name = "tsbTrackRow3";
            tsbTrackRow3.Size = new Size(23, 22);
            tsbTrackRow3.Text = "3";
            tsbTrackRow3.Click += tsbTrackRow3_Click;
            // 
            // toolStrip3
            // 
            toolStrip3.Dock = DockStyle.None;
            toolStrip3.Items.AddRange(new ToolStripItem[] { toolStripLabel3, tsbSEFont8, tsbSEFont12, tsbSEFont16 });
            toolStrip3.Location = new Point(383, 24);
            toolStrip3.Name = "toolStrip3";
            toolStrip3.Size = new Size(220, 25);
            toolStrip3.TabIndex = 3;
            // 
            // toolStripLabel3
            // 
            toolStripLabel3.Name = "toolStripLabel3";
            toolStripLabel3.Size = new Size(108, 22);
            toolStripLabel3.Text = "StepEditor FontSize";
            // 
            // tsbSEFont8
            // 
            tsbSEFont8.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbSEFont8.Image = (Image)resources.GetObject("tsbSEFont8.Image");
            tsbSEFont8.ImageTransparentColor = Color.Magenta;
            tsbSEFont8.Name = "tsbSEFont8";
            tsbSEFont8.Size = new Size(23, 22);
            tsbSEFont8.Text = "8";
            tsbSEFont8.Click += tsbSEFont8_Click;
            // 
            // tsbSEFont12
            // 
            tsbSEFont12.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbSEFont12.Image = (Image)resources.GetObject("tsbSEFont12.Image");
            tsbSEFont12.ImageTransparentColor = Color.Magenta;
            tsbSEFont12.Name = "tsbSEFont12";
            tsbSEFont12.Size = new Size(23, 22);
            tsbSEFont12.Text = "12";
            tsbSEFont12.Click += tsbSEFont12_Click;
            // 
            // tsbSEFont16
            // 
            tsbSEFont16.DisplayStyle = ToolStripItemDisplayStyle.Text;
            tsbSEFont16.Image = (Image)resources.GetObject("tsbSEFont16.Image");
            tsbSEFont16.ImageTransparentColor = Color.Magenta;
            tsbSEFont16.Name = "tsbSEFont16";
            tsbSEFont16.Size = new Size(23, 22);
            tsbSEFont16.Text = "16";
            tsbSEFont16.Click += tsbSEFont16_Click;
            // 
            // timerScreen
            // 
            timerScreen.Enabled = true;
            timerScreen.Interval = 20;
            timerScreen.Tick += timerScreen_Tick;
            // 
            // frmMain
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 538);
            Controls.Add(toolStripContainer1);
            MainMenuStrip = menuStrip1;
            Name = "frmMain";
            Text = "Remcoposer64";
            DragDrop += dockPanel1_DragDrop;
            DragEnter += dockPanel1_DragEnter;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer1.BottomToolStripPanel.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            toolStrip3.ResumeLayout(false);
            toolStrip3.PerformLayout();
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
        private ToolStripMenuItem tsmiSetting;
        private ToolStripMenuItem editToolStripMenuItem;
        //private ToolStripMenuItem tsmiPlay_;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem tsmiOpenOpePanel;
        private ToolStripMenuItem tsmiOpenInformation;
        private ToolStripMenuItem transportToolStripMenuItem;
        private ToolStripMenuItem tsmiPlay;
        private ToolStripMenuItem tsmiStop;
        private ToolStrip toolStrip1;
        private ToolStripLabel toolStripLabel1;
        private ToolStripButton tsbMIDIOut_A;
        private ToolStripButton tsbMIDIOut_B;
        private ToolStripButton tsbMIDIOut_C;
        private ToolStripButton tsbMIDIOut_D;
        private ToolStripButton tsbMIDIOut_E;
        private ToolStripButton tsbMIDIOut_F;
        private ToolStripButton tsbMIDIOut_G;
        private StatusStrip statusStrip1;
        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel1;
        private WeifenLuo.WinFormsUI.Docking.VS2015DarkTheme vS2015DarkTheme1;
        private WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender visualStudioToolStripExtender1;
        private ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.Timer timerScreen;
        private ToolStrip toolStrip2;
        private ToolStripLabel toolStripLabel2;
        private ToolStripButton tsbTrackRow1;
        private ToolStripButton tsbTrackRow2;
        private ToolStripButton tsbTrackRow3;
        private ToolStrip toolStrip3;
        private ToolStripLabel toolStripLabel3;
        private ToolStripButton tsbSEFont8;
        private ToolStripButton tsbSEFont12;
        private ToolStripButton tsbSEFont16;
        private ToolStripMenuItem tsmiCloseSequenceFile;
    }
}
