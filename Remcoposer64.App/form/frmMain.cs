using Remcoposer64.App.form;
using Remcoposer64.Common;
using Remcoposer64.Core;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using System.Security.Cryptography;
using WeifenLuo.WinFormsUI.Docking;

namespace Remcoposer64.App
{
    public partial class frmMain : Form
    {
        private Setting setting;
        private frmConsole console;
        private frmOpePanel opePanel;
        private UndoRedoManager.UndoRedoManager undoMng;

        private List<MIDIProject> projects = new List<MIDIProject>();
        private MIDIProject currentProject = null;
        private RmPlayer player = null;

        private List<frmProject> fProjects = new List<frmProject>();

        public frmMain()
        {
            Initial();
        }

        // E:\FM音源\data\MIDI\MakingMIDI\XGfeelingheart.mid

        public frmMain(string[] args)
        {
            Initial();

            if (args.Length > 1)
            {
                var file = args[1];
                if (File.Exists(file))
                {
                    ImportMidiFile(file);
                }
            }
        }

        internal void Play()
        {
            if (currentProject == null)
            {
                MessageBox.Show("再生するプロジェクトがありません。");
                return;
            }

            player.InitialPlay();
            player.PlayMusic(currentProject);
        }

        internal void Stop()
        {
            player.StopPlayback();
        }

        internal string GetLocationText()
        {
            if (player == null) return "----:---:---";

            int meas = player.GetMeas() + 1;
            int step = player.GetStep() + 1;
            int tick = player.GetTick();

            if (meas <= 1 && step <= 1 && tick <= 0)
                return "----:---:---";

            return $"{meas:D4}:{step:D3}:{tick:D3}";
        }

        internal string GetTempoText()
        {
            if (player == null) return "---/---";
            int tempo = player.GetTempo();
            int bTempo = player.GetBaseTempo();
            if (tempo <= 0 || bTempo <= 0) return "---/---";
            return $"{tempo:D3}/{bTempo:D3}";
        }

        internal string GetTimeText()
        {
            if (player == null) return "-";

            long totalMs = player.GetTimeMSec();
            if (totalMs < 0) return "-";

            int h = (int)(totalMs / (1000 * 60 * 60));
            int m = (int)((totalMs / (1000 * 60)) % 60);
            int s = (int)((totalMs / 1000) % 60);
            int ms = (int)(totalMs % 1000);

            return $"{h:D3}:{m:D2}:{s:D2}.{ms:D3}";
        }

        internal string GetBeatText()
        {
            if (player == null) return "---/---";
            int den = player.GetBeatDen();
            int mol = player.GetBeatMol();
            if (den <= 0 || mol <= 0) return "---/---";
            return $"{mol:D3}/{den:D3}";
        }


        //
        // Event
        //

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmiSetting_Click(object sender, EventArgs e)
        {
            frmSetting frm = new frmSetting(setting);
            DialogResult res = frm.ShowDialog();
            if (res != DialogResult.OK) return;

            setting = frm.GetSetting();
            setting.Save();

            SetDevice();
        }

        private void SetDevice()
        {
            // 演奏停止し、発音を消音し、更にMIDIデバイスをクローズする
            player.StopPlayback();
            player.Close();

            // MIDIデバイスの表示を更新する
            foreach (frmProject tab in fProjects)
            {
                MIDIProject project = (MIDIProject)tab.Tag;
                DataGridView dgv = (DataGridView)tab.Controls[0];

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    LinkedListNode<MIDITrack> trkNode = (LinkedListNode<MIDITrack>)row.Tag;
                    MIDITrack trk = trkNode.Value;
                    Setting.MidiOut mo = setting.midiOut;
                    if (mo.lstMidiOutInfo.Count > mo.CurrentDev
                        && mo.lstMidiOutInfo[mo.CurrentDev] != null)
                    {
                        row.Cells["Device"].Value = mo.lstMidiOutInfo[mo.CurrentDev][Math.Min(trk.OutDevice, mo.lstMidiOutInfo[mo.CurrentDev].Length - 1)].name;
                    }
                }
            }

            // MIDIデバイスをオープンする
            player = new RmPlayer(setting);

        }

        private void tsmiImport_Click(object sender, EventArgs e)
        {
            ImportMidiFile();
        }

        private void tsmiShowConsole_Click(object sender, EventArgs e)
        {
            console.Show();
            console.BringToFront();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 演奏停止し、発音を消音し、更にMIDIデバイスをクローズする
            player.StopPlayback();
            player.Close();

            try
            {
                string fullPath = Common.Common.settingFilePath;
                fullPath = Path.Combine(fullPath, Common.Common.cntLayoutFileName);
                dockPanel1.SaveAsXml(fullPath);
            }
            catch { }

            base.OnFormClosing(e);

            // 開いていたファイルを全て記憶する
            setting.ui.OpenFiles = new List<string>();
            foreach(var project in fProjects)
            {
                if (project?.Tag is not MIDIProject prj)
                    continue;

                if (string.IsNullOrWhiteSpace(prj.Information.FileFullName))
                    continue;

                setting.ui.OpenFiles.Add(((MIDIProject)project.Tag).Information.FileFullName);
            }

            setting.Save();
        }

        private void tsmiSaveSequenceFile_Click(object sender, EventArgs e)
        {
            MIDIProject prj = currentProject;
            string fn = prj.Information.FileFullName;

            var sfd = new SaveFileDialog();
            sfd.FileName = fn;
            sfd.Filter = "RMC Files (*.rmc)|*.rmc";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            prj.Save(sfd.FileName);

            prj.Information.FileFullName = sfd.FileName;
            prj.Information.FileName = Path.GetFileNameWithoutExtension(sfd.FileName);
            ((frmProject)currentProject.parentForm).Text = prj.Information.FileName;
        }

        private void tsmiOpenSequenceFile_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "RMC Files (*.rmc)|*.rmc";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            // プロジェクトをロード
            MIDIProject prj = MIDIProject.Load(ofd.FileName);
            if (prj == null)
            {
                MessageBox.Show("プロジェクトの読み込みに失敗しました。",
                                "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // タブを作成
            TabPage tab = new TabPage();
            tab.Tag = prj;
            tab.Text = prj.Information.FileName;

            // プロジェクトの UI を構築（あなたの既存関数）
            AddProjectTab(prj);

            // ファイル名を更新
            prj.Information.FileFullName = ofd.FileName;
            prj.Information.FileName = Path.GetFileNameWithoutExtension(ofd.FileName);
        }

        private void tsmiOpenOpePanel_Click(object sender, EventArgs e)
        {
            opePanel.Show();
            opePanel.BringToFront();
        }

        private void tsmiOpenInformation_Click(object sender, EventArgs e)
        {
            var info = new frmInformation(currentProject);
            info.Show(dockPanel1, new Rectangle(0,0, 640, 480));
            info.Size = new Size(640, 480);
        }

        internal void tsmiPlay_Click(object sender, EventArgs e)
        {
            Play();
        }

        private void tsmiStop_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void tsbMIDIOut_A_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_A);
            setting.midiOut.CurrentDev = 0;
            SetDevice();
        }

        private void tsbMIDIOut_B_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_B);
            setting.midiOut.CurrentDev = 1;
            SetDevice();
        }

        private void tsbMIDIOut_C_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_C);
            setting.midiOut.CurrentDev = 2;
            SetDevice();
        }

        private void tsbMIDIOut_D_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_D);
            setting.midiOut.CurrentDev = 3;
            SetDevice();
        }

        private void tsbMIDIOut_E_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_E);
            setting.midiOut.CurrentDev = 4;
            SetDevice();
        }

        private void tsbMIDIOut_F_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_F);
            setting.midiOut.CurrentDev = 5;
            SetDevice();
        }

        private void tsbMIDIOut_G_Click(object sender, EventArgs e)
        {
            SelectPresetButton(tsbMIDIOut_G);
            setting.midiOut.CurrentDev = 6;
            SetDevice();
        }






        private void Initial()
        {
            InitializeComponent();

            Log.Write(LogLevel.Information, "Start initialization.");

            dockPanel1.Theme = vS2015DarkTheme1;

            Common.Common.SetExecutablePath(Application.ExecutablePath);
            setting = Setting.Load();


            undoMng = new UndoRedoManager.UndoRedoManager();
            player = new RmPlayer(setting);

            switch (setting.midiOut.CurrentDev)
            {
                case 0:
                default:
                    SelectPresetButton(tsbMIDIOut_A);
                    break;
                case 1:
                    SelectPresetButton(tsbMIDIOut_B);
                    break;
                case 2:
                    SelectPresetButton(tsbMIDIOut_C);
                    break;
                case 3:
                    SelectPresetButton(tsbMIDIOut_D);
                    break;
                case 4:
                    SelectPresetButton(tsbMIDIOut_E);
                    break;
                case 5:
                    SelectPresetButton(tsbMIDIOut_F);
                    break;
                case 6:
                    SelectPresetButton(tsbMIDIOut_G);
                    break;
            }

            RestoreLayout();

            foreach (string fn in setting.ui.OpenFiles)
            {
                if (File.Exists(fn))
                    ImportMidiFile(fn);
            }

        }

        private void ImportMidiFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "All support files (*.mid;*.rcp;*.g36)|*.mid;*.rcp;*.g36|MIDI Files (*.mid)|*.mid|RCP Files (*.rcp;*.g36)|*.rcp;*.g36";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            ImportMidiFile(ofd.FileName);
        }

        private void ImportMidiFile(string fileName)
        {
            MIDIProject project = null;

            string ext = Path.GetExtension(fileName).ToLower();
            if (ext == ".mid")
            {
                // SMF → RMC 変換器を作成
                ImportStandardMIDI importer = new ImportStandardMIDI(fileName, setting);
                // プロジェクトをロード
                project = importer.Load();
            }
            else if (ext == ".rcp" || ext == ".g36")
            {
                // RCP → RMC 変換器を作成
                ImportRcp importer = new ImportRcp(fileName, setting);
                // プロジェクトをロード
                project = importer.Load();
            }
            else if (ext == ".rmc")
            {
                project = MIDIProject.Load(fileName);
            }

            if (project == null)
            {
                Log.Write(LogLevel.Error, $"Import {fileName} is fail.");
                return;
            }

            Log.Write(LogLevel.Information, $"Import {fileName} is succed.");

            projects.Add(project);
            currentProject = project;

            AddProjectTab(project);
            //UpdateInformation(project);
        }

        private void AddProjectTab(MIDIProject project)
        {
            // 新しいタブページを作成
            frmProject tab = new frmProject(this,project);
            tab.Text = project.Information.FileName;

            // プロジェクトをタブに紐づける
            tab.Tag = project;
            project.parentForm = tab;
            fProjects.Add(tab);

            DataGridView dgvTracks = new DataGridView();
            dgvTracks.Dock = DockStyle.Fill;
            dgvTracks.AllowUserToAddRows = false;
            dgvTracks.AllowUserToDeleteRows = false;
            dgvTracks.AllowUserToResizeRows = false;
            dgvTracks.RowHeadersVisible = false;
            dgvTracks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvTracks.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvTracks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            dgvTracks.Columns.Clear();
            dgvTracks.Columns.Add("No", "No");
            dgvTracks.Columns.Add("Name", "Name");
            dgvTracks.Columns.Add("Device", "Device");
            dgvTracks.Columns.Add("Channel", "Ch");
            dgvTracks.Columns.Add("Parts", "Parts");
            dgvTracks.Columns["Parts"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dgvTracks.Columns["Parts"].ReadOnly = true;
            dgvTracks.Columns["Parts"].Width = 80;
            dgvTracks.Columns.Add("Spacer", "");
            dgvTracks.Columns["Spacer"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvTracks.Columns["Spacer"].ReadOnly = true;
            dgvTracks.CellDoubleClick += (s, e) =>
            {
                // Parts 列以外は無視
                if (e.ColumnIndex != dgvTracks.Columns["Parts"].Index)
                    return;

                int trackIndex = e.RowIndex;
                MIDITrack trk = project.getTrack4TrackNumber(trackIndex);

                LinkedListNode<MIDIPart> part = trk.getStartPartNode();
                if (part == null)
                    return;

                frmStepEditor frm = new frmStepEditor(trk, part.Value, undoMng);
                frm.Show(dockPanel1,new Rectangle(0,0, 816, 480));
                frm.Size = new Size(816, 480);
            };

            dgvTracks.Rows.Clear();

            Setting.MidiOut mo = setting.midiOut;
            LinkedListNode<MIDITrack> trk = project.getStartTrackNode();
            while (trk != null)
            {
                string name = "";
                if (mo != null
                    && mo.lstMidiOutInfo != null
                    && mo.lstMidiOutInfo.Count > 0
                    && mo.lstMidiOutInfo.Count > mo.CurrentDev
                    && mo.lstMidiOutInfo[mo.CurrentDev] != null
                )
                    name = mo.lstMidiOutInfo[mo.CurrentDev][Math.Max(Math.Min(trk.Value.OutDevice, mo.lstMidiOutInfo[mo.CurrentDev].Length - 1), 0)].name;

                dgvTracks.Rows.Add(
                    trk.Value.Number + 1,
                    trk.Value.Name,
                    name,
                    trk.Value.OutChannel + 1,
                    MakePartDots(trk.Value)
                );
                dgvTracks.Rows[dgvTracks.Rows.Count - 1].Tag = trk;

                trk = project.getNextTrackNode(trk);
            }

            tab.Controls.Add(dgvTracks);

            // タブを追加
            tab.Show(dockPanel1, DockState.Document);

            // そのタブを選択
            tab.Activate();
        }

        private void UpdateInformation(MIDIProject project)
        {
            frmInformation info = new frmInformation(project);
            info.Show(dockPanel1, new Rectangle(0, 0, 640, 480));
            info.Size = new Size(640, 480);
        }

        private string MakePartDots(MIDITrack trk)
        {
            int count = 0;
            LinkedListNode<MIDIPart> part = trk.getStartPartNode();
            while (part != null)
            {
                count++;
                part = trk.getNextPartNode(part);
            }

            return new string('●', count);
        }

        private void SelectPresetButton(ToolStripButton selected)
        {
            foreach (ToolStripItem item in toolStrip1.Items)
            {
                if (item is ToolStripButton btn)
                {
                    btn.Checked = (btn == selected);
                }
            }
        }

        private void dockPanel1_ActiveContentChanged(object sender, EventArgs e)
        {
            var active = dockPanel1.ActiveContent as frmProject;
            if (active != null)
            {
                currentProject = (MIDIProject)active.Tag;
                // StepEditor がアクティブ
            }
        }

        private void dockPanel1_ActiveDocumentChanged(object sender, EventArgs e)
        {
            var active = dockPanel1.ActiveDocument as frmProject;
            if (active != null)
            {
                currentProject = (MIDIProject)active.Tag;
            }
        }

        public void CloseProject(MIDIProject project)
        {
            if (currentProject == project)
            {
                currentProject = null;
            }
        }


        private void RestoreLayout()
        {
            console = new frmConsole(setting);
            opePanel = new frmOpePanel(this);

            string fullPath = Path.Combine(Common.Common.settingFilePath,
                                Common.Common.cntLayoutFileName);
            if (File.Exists(fullPath))
                dockPanel1.LoadFromXml(fullPath, DeserializeDockContent);

            if (console.DockState == DockState.Unknown)
            {
                console.Show(dockPanel1, new Rectangle(0, 0, 816, 489));
                console.Size = new Size(816, 489);
            }

            if (opePanel.DockState == DockState.Unknown)
            {
                opePanel.Show(dockPanel1, new Rectangle(0,0, 372, 121));
                opePanel.Size = new Size(372, 121);
            }
        }

        private IDockContent DeserializeDockContent(string persistString)
        {
            if (persistString.IndexOf("frmConsole") >= 0) return console;
            if (persistString.IndexOf("frmOpePanel") >= 0) return opePanel;

            return null;
        }

    }
}
