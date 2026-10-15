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
            //TBD
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
            //TBD
        }

        private void bt1_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt2_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt3_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt4_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt5_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt6_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt7_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void bt8_Click(object sender, EventArgs e)
        {
            //TBD
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            lblLocation.Text = main.GetLocationText();
            lblTempo.Text = main.GetTempoText();
            lblTime.Text = main.GetTimeText();
            lblBeat.Text = main.GetBeatText();
        }

        private void frmOpePanel_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; // Close をキャンセル
            this.Hide();     // 非表示にするだけ
        }
    }
}
