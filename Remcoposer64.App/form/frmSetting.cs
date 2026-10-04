using Remcoposer64.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NAudio.Utils;
using NAudio.Midi;

namespace Remcoposer64.App.form
{
    public partial class frmSetting : Form
    {
        private Setting setting = null;
        private DataGridView[] dgv = null;
        private string[] midiOutTypeList = new string[] { "GM", "XG", "GS", "LA", "GS(SC-55_1)", "GS(SC-55_2)" };
        private string[] midiOutBeforeSendList = new string[] { "None", "GM Reset", "XG Reset", "GS Reset", "Custom" };

        public frmSetting()
        {
            InitializeComponent();
        }

        public frmSetting(Setting setting)
        {
            InitializeComponent();
            this.setting = setting;

            Init();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Init()
        {

            dgv = new DataGridView[] {
                dgvMIDIoutListA
            };

            // 認識しているMIDI デバイスをDataGridViewに列挙する
            dgvMIDIoutPallet.Rows.Clear();
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                var moc = MidiOut.DeviceInfo(i);
                dgvMIDIoutPallet.Rows.Add(i, moc.ProductName, moc.Manufacturer.ToString() != "-1" ? moc.Manufacturer.ToString() : "Unknown");
            }
            dgvMIDIinPallet.Rows.Clear();
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var moc = MidiIn.DeviceInfo(i);
                dgvMIDIinPallet.Rows.Add(i, moc.ProductName, moc.Manufacturer.ToString() != "-1" ? moc.Manufacturer.ToString() : "Unknown");
            }

            // 設定ファイルに保存されているMIDI出力設定をDataGridViewに列挙する
            if (setting.midiOut.lstMidiOutInfo != null && setting.midiOut.lstMidiOutInfo.Count > 0)
            {
                for (int i = 0; i < setting.midiOut.lstMidiOutInfo.Count; i++)
                {
                    dgv[i].Rows.Clear();
                    HashSet<int> midioutNotFound = new HashSet<int>();
                    if (setting.midiOut.lstMidiOutInfo[i] == null || setting.midiOut.lstMidiOutInfo[i].Length <= 0)
                        continue;
                    for (int j = 0; j < setting.midiOut.lstMidiOutInfo[i].Length; j++)
                    {
                        Setting.midiOutInfo moi = setting.midiOut.lstMidiOutInfo[i][j];
                        int found = -999;
                        for (int k = 0; k < MidiOut.NumberOfDevices; k++)
                        {
                            MidiOutCapabilities moc = MidiOut.DeviceInfo(k);
                            if (moi.name != moc.ProductName) continue;
                            midioutNotFound.Add(k);
                            found = k;
                            break;
                        }

                        moi.id = found;
                        string stype = midiOutTypeList[moi.type];
                        string sbeforeSend = midiOutBeforeSendList[moi.beforeSendType];

                        dgv[i].Rows.Add(
                            moi.id
                            , moi.isVST
                            , moi.fileName
                            , moi.name
                            , stype
                            , sbeforeSend
                            , moi.isVST ? moi.vendor : (moi.manufacturer != -1 ? ((NAudio.Manufacturers)moi.manufacturer).ToString() : "Unknown")
                            );

                    }
                }
            }

            // 設定ファイルに保存されているMIDI入力設定をDataGridViewに列挙する
            if(setting.midiIn.lstMidiInInfo != null && setting.midiIn.lstMidiInInfo.Count > 0)
            {
                for (int i = 0; i < setting.midiOut.lstMidiOutInfo.Count; i++)
                {
                    dgvMIDIinListA.Rows.Clear();
                    HashSet<int> midiinNotFound = new HashSet<int>();
                    if (setting.midiIn.lstMidiInInfo[i] == null || setting.midiIn.lstMidiInInfo[i].Length <= 0)
                        continue;

                    for (int j = 0; j < setting.midiIn.lstMidiInInfo[i].Length; j++)
                    {
                        Setting.midiInInfo mii = setting.midiIn.lstMidiInInfo[i][j];
                        int found = -999;
                        for (int k = 0; k < MidiIn.NumberOfDevices; k++)
                        {
                            MidiInCapabilities mic = MidiIn.DeviceInfo(k);
                            if (mii.name != mic.ProductName) continue;
                            midiinNotFound.Add(k);
                            found = k;
                            break;
                        }
                        mii.id = found;
                        dgvMIDIinListA.Rows.Add(
                            mii.id
                            , mii.name
                            , mii.isVST
                            , mii.fileName
                            ,""
                            , mii.isVST ? mii.vendor : (mii.manufacturer != -1 ? ((NAudio.Manufacturers)mii.manufacturer).ToString() : "Unknown")
                            );
                    }
                }
            }
        }

        private void btnAddMIDIout_Click(object sender, EventArgs e)
        {
            if (dgvMIDIoutPallet.SelectedRows == null || dgvMIDIoutPallet.SelectedRows.Count < 1) return;

            int p = tbcMIDIoutList.SelectedIndex;

            foreach (DataGridViewRow row in dgvMIDIoutPallet.SelectedRows)
            {
                bool found = false;
                foreach (DataGridViewRow r in dgv[p].Rows)
                {
                    if (r.Cells[1].Value.ToString() == row.Cells[1].Value.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) dgv[p].Rows.Add(row.Cells[0].Value, false, "", row.Cells[1].Value, "GM", "None", row.Cells[2].Value);
            }
        }

        private void btnSubMIDIout_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIoutList.SelectedIndex;

            if (dgv[p].SelectedRows == null || dgv[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgv[p].SelectedRows)
            {
                dgv[p].Rows.Remove(row);
            }
        }

        private void btnUPMIDIOut_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIoutList.SelectedIndex;

            if (dgv[p].SelectedRows == null || dgv[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgv[p].SelectedRows)
            {
                if (row.Index < 1) continue;

                int i = row.Index - 1;
                dgv[p].Rows.Insert(i, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value);
                dgv[p].Rows.Remove(row);
                dgv[p].Rows[i].Selected = true;
            }
        }

        private void btnDownMIDIOut_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIoutList.SelectedIndex;

            if (dgv[p].SelectedRows == null || dgv[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgv[p].SelectedRows)
            {
                if (row.Index > dgv[p].Rows.Count - 2) continue;

                int i = row.Index + 1;
                dgv[p].Rows.Insert(row.Index + 2, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value);
                dgv[p].Rows.Remove(row);
                dgv[p].Rows[i].Selected = true;
            }
        }

    }
}
