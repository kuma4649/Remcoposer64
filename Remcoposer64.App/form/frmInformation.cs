using Remcoposer64.ProjectData;

namespace Remcoposer64.App.form
{
    public partial class frmInformation : Form
    {
        private MIDIProject project;

        public frmInformation(MIDIProject prj)
        {
            InitializeComponent();
            this.project = prj;

            // 初期値を反映
            txtFileName.Text = prj.Information.FileName;
            txtTitle.Text = prj.Information.Title;
            txtTempo.Text = prj.Information.Tempo.ToString();
            txtTimeBase.Text = prj.Information.TimeBase.ToString();
            txtBeat.Text = $"{prj.Information.BeatDen}";
            txtBeat2.Text = $"{prj.Information.BeatMol}";
            txtKey.Text = prj.Information.Key.ToString();
            txtCopyright.Text = prj.Information.Copyright;
            txtMemo.Text = prj.Information.Memo;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            project.Information.FileName = txtFileName.Text;
            project.Information.Title = txtTitle.Text;
            project.Information.Tempo = int.Parse(txtTempo.Text);
            project.Information.TimeBase = int.Parse(txtTimeBase.Text);

            project.Information.BeatDen = int.Parse(txtBeat.Text);
            project.Information.BeatMol = int.Parse(txtBeat2.Text);
            project.Information.Key = int.Parse(txtKey.Text);
            project.Information.Copyright = txtCopyright.Text;
            project.Information.Memo = txtMemo.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
