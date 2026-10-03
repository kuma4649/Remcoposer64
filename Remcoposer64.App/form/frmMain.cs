using Remcoposer64.App.form;
using Remcoposer64.Core;
using Remcoposer64.ProjectData;
using Remcoposer64.UndoRedoManager;

using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Remcoposer64.App
{
    public partial class frmMain : Form
    {
        private Setting setting;
        private frmConsole console;
        private UndoRedoManager.UndoRedoManager undoMng;

        public frmMain()
        {
            InitializeComponent();

            Common.SetExecutablePath(Application.ExecutablePath);
            setting = Setting.Load();

            console = new frmConsole(setting);
#if DEBUG
            console.Show();
            console.BringToFront();
#endif
            undoMng = new UndoRedoManager.UndoRedoManager();
        }


        //
        // Event
        //

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
            base.OnFormClosing(e);
            setting.Save();
        }



        private void ImportMidiFile()
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "MIDI Files (*.mid)|*.mid";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            // SMF → RCP 変換器を作成
            ImportStandardMIDI importer = new ImportStandardMIDI(ofd.FileName, setting);
            // プロジェクトをロード
            MIDIProject project = importer.Load();
            Log.Write(LogLevel.Information, $"Import {ofd.FileName} is succed.");

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

            LinkedListNode<MIDITrack> trk = project.getStartTrackNode();
            while (trk != null)
            {
                dgvTracks.Rows.Add(
                    trk.Value.Number + 1,
                    trk.Value.Name,
                    trk.Value.OutDeviceName,
                    trk.Value.OutChannel + 1,
                    MakePartDots(trk.Value)
                );

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
