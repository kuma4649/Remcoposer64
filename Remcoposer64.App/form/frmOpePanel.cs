using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Remcoposer64.App.form
{
    public partial class frmOpePanel : DockContent
    {
        public Action afterClose;
        private frmMain main;

        public frmOpePanel(frmMain main)
        {
            InitializeComponent();
            this.main = main;
        }

        //public frmOpePanel()
        //{
        //    InitializeComponent();
        //}

        private void btRewind_Click(object sender, EventArgs e)
        {
            main.SetFirstFrame();
        }

        private void btPlay_Click(object sender, EventArgs e)
        {
            main.Play();
        }

        private void btStop_Click(object sender, EventArgs e)
        {
            main.Stop();
        }

        private void btFF_Click(object sender, EventArgs e)
        {

        }

        private void cbFF_CheckedChanged(object sender, EventArgs e)
        {
            main.setting.player.playSpeed = cbFF.Checked ? 10.0 : 1.0;
            cbSlow.Checked = false;
        }

        private void cbSlow_CheckedChanged(object sender, EventArgs e)
        {
            main.setting.player.playSpeed = cbSlow.Checked ? 0.1 : 1.0;
            cbFF.Checked = false;

        }


        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            lblLocation.Text = main.GetLocationText();
            lblTempo.Text = main.GetTempoText();
            lblTime.Text = main.GetTimeText();
            lblBeat.Text = main.GetBeatText();
        }

        private void frmOpePanel_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (afterClose != null)
            {
                afterClose();
            }
        }

        public void SetMarker(MIDIProject currentProject)
        {
            List<MarkerPoint> mps = currentProject.Information.Markers;
            bt1.Tag = (0 <= mps.Count-10) ? mps[mps.Count - 10] : null;
            bt2.Tag = (1 <= mps.Count-9) ? mps[mps.Count - 9] : null;
            bt3.Tag = (2 <= mps.Count-8) ? mps[mps.Count - 8] : null;
            bt4.Tag = (3 <= mps.Count-7) ? mps[mps.Count - 7] : null;
            bt5.Tag = (4 <= mps.Count-6) ? mps[mps.Count - 6] : null;
            bt6.Tag = (5 <= mps.Count-5) ? mps[mps.Count - 5] : null;
            bt7.Tag = (6 <= mps.Count-4) ? mps[mps.Count - 4] : null;
            bt8.Tag = (7 <= mps.Count-3) ? mps[mps.Count - 3] : null;
            bt9.Tag = (8 <= mps.Count-2) ? mps[mps.Count - 2] : null;
            bt10.Tag = (9 <= mps.Count - 1) ? mps[mps.Count-1] : null;
        }

        private void bt1_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt1.Tag);
        }

        private void bt2_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt2.Tag);
        }

        private void bt3_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt3.Tag);
        }

        private void bt4_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt4.Tag);
        }

        private void bt5_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt5.Tag);
        }

        private void bt6_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt6.Tag);

        }

        private void bt7_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt7.Tag);
        }

        private void bt8_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt8.Tag);
        }

        private void bt9_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt9.Tag);
        }

        private void bt10_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt10.Tag);
        }
    }
}
