using Remcoposer64.App.form;
using Remcoposer64.Common;
using Remcoposer64.Core;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;

namespace Remcoposer64.App
{
    public partial class frmMain : Form
    {
        private Setting setting;
        private frmConsole console;
        private UndoRedoManager.UndoRedoManager undoMng;

        private List<MIDIProject> projects = new List<MIDIProject>();
        private MIDIProject currentProject = null;
        private RmPlayer player = null;

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

        //
        // Event
        //

        private void tsmiExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void tsmiPlay_Click(object sender, EventArgs e)
        {
            if (currentProject == null)
            {
                MessageBox.Show("再生するプロジェクトがありません。");
                return;
            }

            player.InitialPlay();
            player.PlayMusic(currentProject);
        }

        private void tsmiSetting_Click(object sender, EventArgs e)
        {
            frmSetting frm = new frmSetting(setting);
            DialogResult res = frm.ShowDialog();
            if (res != DialogResult.OK) return;

            setting = frm.GetSetting();
            setting.Save();

            // 演奏停止し、発音を消音し、更にMIDIデバイスをクローズする
            player.StopPlayback();
            player.Close(); 

            // MIDIデバイスの表示を更新する
            foreach (TabPage tab in tabControl1.TabPages)
            {
                MIDIProject project = (MIDIProject)tab.Tag;
                DataGridView dgv = (DataGridView)tab.Controls[0];

                foreach (DataGridViewRow row in dgv.Rows)
                {
                    LinkedListNode<MIDITrack> trkNode = (LinkedListNode<MIDITrack>)row.Tag;
                    MIDITrack trk = trkNode.Value;
                    Setting.MidiOut mo = setting.midiOut;
                    row.Cells["Device"].Value = mo.lstMidiOutInfo[mo.CurrentDev][Math.Min(trk.OutDevice, mo.lstMidiOutInfo[mo.CurrentDev].Length - 1)].name;
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

            base.OnFormClosing(e);
            setting.Save();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == null)
            {
                currentProject = null;
                return;
            }

            TabPage tp = tabControl1.SelectedTab;
            currentProject = (MIDIProject)tp.Tag;
        }



        public void Initial()
        {
            InitializeComponent();

            Common.Common.SetExecutablePath(Application.ExecutablePath);
            setting = Setting.Load();

            console = new frmConsole(setting);
#if DEBUG
            console.Show();
            console.BringToFront();
#endif
            undoMng = new UndoRedoManager.UndoRedoManager();
            player = new RmPlayer(setting);
        }

        private void ImportMidiFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "All support files (*.mid;*.rcp)|*.mid;*.rcp|MIDI Files (*.mid)|*.mid|RCP Files (*.rcp)|*.rcp";

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
            else if (ext == ".rcp")
            {
                // RCP → RMC 変換器を作成
                ImportRcp importer = new ImportRcp(fileName, setting);
                // プロジェクトをロード
                project = importer.Load();
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
            UpdateInformation(project);
        }

        private void AddProjectTab(MIDIProject project)
        {
            // 新しいタブページを作成
            TabPage tab = new TabPage(project.Information.FileName);

            // プロジェクトをタブに紐づける
            tab.Tag = project;

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
                frm.Show();
            };

            dgvTracks.Rows.Clear();

            Setting.MidiOut mo = setting.midiOut;
            LinkedListNode<MIDITrack> trk = project.getStartTrackNode();
            while (trk != null)
            {
                dgvTracks.Rows.Add(
                    trk.Value.Number + 1,
                    trk.Value.Name,
                    mo.lstMidiOutInfo[mo.CurrentDev][Math.Max( Math.Min(trk.Value.OutDevice, mo.lstMidiOutInfo[mo.CurrentDev].Length - 1),0)].name,
                    trk.Value.OutChannel + 1,
                    MakePartDots(trk.Value)
                );
                dgvTracks.Rows[dgvTracks.Rows.Count - 1].Tag = trk;

                trk = project.getNextTrackNode(trk);
            }

            tab.Controls.Add(dgvTracks);

            // タブを追加
            tabControl1.TabPages.Add(tab);

            // そのタブを選択
            tabControl1.SelectedTab = tab;
        }

        private void UpdateInformation(MIDIProject project)
        {
            frmInformation info = new frmInformation(project);
            info.Show();
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

    }
}
