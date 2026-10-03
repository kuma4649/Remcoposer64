using Remcoposer64.Core;
using Remcoposer64.ProjectData;
using System;
using System.Windows.Forms;

namespace Remcoposer64.App.form
{
    public partial class frmTrackList : Form
    {
        private MIDIProject project;

        public frmTrackList(MIDIProject prj)
        {
            InitializeComponent();
            this.project = prj;

            LoadTracks();
        }

        private void LoadTracks()
        {
            lstTracks.Items.Clear();

            LinkedListNode<MIDITrack> trk = project.getStartTrackNode();
            while (trk != null)
            {
                lstTracks.Items.Add($"{trk.Value.Number}: {trk.Value.Name}");
                trk = project.getNextTrackNode(trk);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
