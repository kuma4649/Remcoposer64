using NAudio.Midi;
using NAudio.Utils;
using Remcoposer64.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static Remcoposer64.Common.Setting;

namespace Remcoposer64.App.form
{
    public partial class frmSetting : Form
    {
        private Setting setting = null;
        private DataGridView[] dgv = null;
        private DataGridView[] dgvIn = null;
        private string[] midiOutTypeList = new string[] { "GM", "XG", "GS", "LA", "GS(SC-55_1)", "GS(SC-55_2)" };
        private string[] midiOutBeforeSendList = new string[] { "None", "GM Reset", "XG Reset", "GS Reset", "Custom" };

        public frmSetting()
        {
            InitializeComponent();
        }

        public frmSetting(Setting setting)
        {
            InitializeComponent();
            this.setting = setting.Copy();

            Init();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void Init()
        {

            dgv = [dgvMIDIoutListA];
            dgvIn = [dgvMIDIinListA];

            // 認識しているMIDI デバイスをDataGridViewに列挙する
            dgvMIDIoutPallet.Rows.Clear();
            for (int i = 0; i < NAudio.Midi.MidiOut.NumberOfDevices; i++)
            {
                var moc = NAudio.Midi.MidiOut.DeviceInfo(i);
                dgvMIDIoutPallet.Rows.Add(i, moc.ProductName, moc.Manufacturer.ToString() != "-1" ? moc.Manufacturer.ToString() : "Unknown");
            }
            dgvMIDIinPallet.Rows.Clear();
            for (int i = 0; i < NAudio.Midi.MidiIn.NumberOfDevices; i++)
            {
                var moc = NAudio.Midi.MidiIn.DeviceInfo(i);
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
                        for (int k = 0; k < NAudio.Midi.MidiOut.NumberOfDevices; k++)
                        {
                            NAudio.Midi.MidiOutCapabilities moc = NAudio.Midi.MidiOut.DeviceInfo(k);
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
            if (setting.midiIn.lstMidiInInfo != null && setting.midiIn.lstMidiInInfo.Count > 0)
            {
                for (int i = 0; i < setting.midiOut.lstMidiOutInfo.Count; i++)
                {
                    dgvIn[i].Rows.Clear();
                    HashSet<int> midiinNotFound = new HashSet<int>();
                    if (setting.midiIn.lstMidiInInfo[i] == null || setting.midiIn.lstMidiInInfo[i].Length <= 0)
                        continue;

                    for (int j = 0; j < setting.midiIn.lstMidiInInfo[i].Length; j++)
                    {
                        Setting.midiInInfo mii = setting.midiIn.lstMidiInInfo[i][j];
                        int found = -999;
                        for (int k = 0; k < NAudio.Midi.MidiIn.NumberOfDevices; k++)
                        {
                            NAudio.Midi.MidiInCapabilities mic = NAudio.Midi.MidiIn.DeviceInfo(k);
                            if (mii.name != mic.ProductName) continue;
                            midiinNotFound.Add(k);
                            found = k;
                            break;
                        }
                        mii.id = found;
                        string stype = midiOutTypeList[mii.type];
                        dgvIn[i].Rows.Add(
                            mii.id
                            , mii.isVST
                            , mii.fileName
                            , mii.name
                            , stype
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

        private void btnAddMIDIin_Click(object sender, EventArgs e)
        {
            if (dgvMIDIinPallet.SelectedRows == null || dgvMIDIinPallet.SelectedRows.Count < 1) return;

            int p = tbcMIDIinList.SelectedIndex;

            foreach (DataGridViewRow row in dgvMIDIinPallet.SelectedRows)
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

                if (!found) dgvIn[p].Rows.Add(row.Cells[0].Value, false, "", row.Cells[1].Value, "GM", row.Cells[2].Value);
            }

        }

        private void btnSubMIDIin_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIinList.SelectedIndex;

            if (dgvIn[p].SelectedRows == null || dgvIn[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvIn[p].SelectedRows)
            {
                dgvIn[p].Rows.Remove(row);
            }
        }

        private void btnUPMIDIin_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIinList.SelectedIndex;

            if (dgvIn[p].SelectedRows == null || dgvIn[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvIn[p].SelectedRows)
            {
                if (row.Index < 1) continue;

                int i = row.Index - 1;
                dgvIn[p].Rows.Insert(i, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value);
                dgvIn[p].Rows.Remove(row);
                dgvIn[p].Rows[i].Selected = true;
            }
        }

        private void btnDownMIDIin_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIinList.SelectedIndex;

            if (dgvIn[p].SelectedRows == null || dgvIn[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvIn[p].SelectedRows)
            {
                if (row.Index > dgvIn[p].Rows.Count - 2) continue;

                int i = row.Index + 1;
                dgvIn[p].Rows.Insert(row.Index + 2, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value);
                dgvIn[p].Rows.Remove(row);
                dgvIn[p].Rows[i].Selected = true;
            }

        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            int i;

            // 設定ファイルに保存するために、MIDIデバイスの設定内容をSettingクラスに格納する

            setting.midiOut.lstMidiOutInfo = new List<Setting.midiOutInfo[]>();
            foreach (DataGridView d in dgv)
            {
                if (d.Rows.Count < 1)
                {
                    setting.midiOut.lstMidiOutInfo.Add(null);
                    continue;
                }
                List<midiOutInfo> lstMoi = new List<midiOutInfo>();
                for (i = 0; i < d.Rows.Count; i++)
                {
                    midiOutInfo moi = new midiOutInfo();
                    moi.id = (int)d.Rows[i].Cells[0].Value;
                    moi.isVST = (bool)d.Rows[i].Cells[1].Value;
                    moi.fileName = (string)d.Rows[i].Cells[2].Value;
                    moi.name = (string)d.Rows[i].Cells[3].Value;
                    string stype = (string)d.Rows[i].Cells[4].Value;
                    //GM / XG / GS / LA / GS(SC - 55_1) / GS(SC - 55_2)
                    moi.type = 0;
                    if (stype == "XG") moi.type = 1;
                    else if (stype == "GS") moi.type = 2;
                    else if (stype == "LA") moi.type = 3;
                    else if (stype == "GS(SC - 55_1)") moi.type = 4;
                    else if (stype == "GS(SC - 55_2)") moi.type = 5;
                    string sbeforeSend = (string)d.Rows[i].Cells[5].Value;
                    moi.beforeSendType = 0;
                    if (sbeforeSend == "GM Reset") moi.beforeSendType = 1;
                    else if (sbeforeSend == "XG Reset") moi.beforeSendType = 2;
                    else if (sbeforeSend == "GS Reset") moi.beforeSendType = 3;
                    else if (sbeforeSend == "Custom") moi.beforeSendType = 4;

                    string mn = (string)d.Rows[i].Cells[6].Value;
                    moi.vendor = mn;
                    moi.manufacturer = -1;
                    if (!moi.isVST)
                    {
                        moi.vendor = "";
                        if (Enum.TryParse(typeof(NAudio.Manufacturers), mn, out var manufacturerEnum))
                            moi.manufacturer = (int)manufacturerEnum;
                        else
                            moi.manufacturer = -1; // Unknown manufacturer
                    }

                    lstMoi.Add(moi);
                }
                setting.midiOut.lstMidiOutInfo.Add(lstMoi.ToArray());
            }

            setting.midiIn.lstMidiInInfo = new List<Setting.midiInInfo[]>();
            foreach (DataGridView d in dgvIn)
            {
                if (d.Rows.Count < 1)
                {
                    setting.midiIn.lstMidiInInfo.Add(null);
                    continue;
                }

                List<midiInInfo> lstMii = new List<midiInInfo>();
                for (i = 0; i < d.Rows.Count; i++)
                {
                    midiInInfo mii = new midiInInfo();
                    mii.id = (int)d.Rows[i].Cells[0].Value;
                    mii.isVST = (bool)d.Rows[i].Cells[1].Value;
                    mii.fileName = (string)d.Rows[i].Cells[2].Value;
                    mii.name = (string)d.Rows[i].Cells[3].Value;
                    string stype = (string)d.Rows[i].Cells[4].Value;
                    //GM / XG / GS / LA / GS(SC - 55_1) / GS(SC - 55_2)
                    mii.type = 0;
                    if (stype == "XG") mii.type = 1;
                    else if (stype == "GS") mii.type = 2;
                    else if (stype == "LA") mii.type = 3;
                    else if (stype == "GS(SC - 55_1)") mii.type = 4;
                    else if (stype == "GS(SC - 55_2)") mii.type = 5;
                    string mn = (string)d.Rows[i].Cells[5].Value;
                    mii.vendor = mn;
                    mii.manufacturer = -1;
                    if (!mii.isVST)
                    {
                        mii.vendor = "";
                        if(Enum.TryParse(typeof(NAudio.Manufacturers), mn, out var manufacturerEnum))
                            mii.manufacturer = (int)manufacturerEnum;
                        else
                            mii.manufacturer = -1; // Unknown manufacturer
                    }
                    lstMii.Add(mii);
                }
                setting.midiIn.lstMidiInInfo.Add(lstMii.ToArray());
            }


            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        public Setting GetSetting()
        {
            return setting;
        }
    }
}
