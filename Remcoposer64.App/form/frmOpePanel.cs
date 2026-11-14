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
            if (currentProject == null) return;
            List<MarkerPoint> mps = currentProject.Information.Markers;
            Button[] bt = [ bt0, bt1, bt2, bt3, bt4, bt5, bt6, bt7, bt8, bt9 ];

            for (int i = 0; i < bt.Length; i++)
            {
                int j = mps.Count - 10 + i;
                if (mps.Count < 10) j = i;
                MarkerPoint mp = j < mps.Count ? mps[j] : null;

                bt[i].Tag = mp;
                bt[i].Enabled = (mp != null);
            }
        }

        private void bt0_Click(object sender, EventArgs e)
        {
            main.Play((MarkerPoint)bt0.Tag);
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

    }
}
