using Microsoft.VisualBasic;
using Remcoposer64.Common;
using static Remcoposer64.Common.Setting;

namespace Remcoposer64.App.form
{
    public partial class frmSetting : Form
    {
        private Setting setting = null;
        private string[] midiTypeList = new string[] { "GM", "XG", "GS", "LA", "GS(SC-55_1)", "GS(SC-55_2)" };
        private string[] midiOutBeforeSendList = new string[] { "None", "GM Reset", "XG Reset", "GS Reset", "Custom" };
        private DataGridView[] dgvMIDIOutList;
        private DataGridView[] dgvMIDIInList;

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

            // コンボボックス列の設定候補を設定する

            dgvMIDIOutList = [
                dgvMIDIoutListA, dgvMIDIoutListB, dgvMIDIoutListC, dgvMIDIoutListD,
                dgvMIDIoutListE, dgvMIDIoutListF, dgvMIDIoutListG
                ];
            dgvMIDIInList = [
                dgvMIDIinListA
                ];
            for (int i = 0; i < dgvMIDIOutList.Length; i++)
            {
                var dgv = dgvMIDIOutList[i];
                DataGridViewComboBoxColumn clmType = ((DataGridViewComboBoxColumn)dgv.Columns["clmType" + ((char)('A' + i)).ToString()]);
                clmType.Items.Clear(); foreach (string midiOutType in midiTypeList) { clmType.Items.Add(midiOutType); }
                DataGridViewComboBoxColumn clmBeforeSend = ((DataGridViewComboBoxColumn)dgv.Columns["clmBeforeSend" + ((char)('A' + i)).ToString()]);
                clmBeforeSend.Items.Clear(); foreach (string midiOutBeforeSend in midiOutBeforeSendList) { clmBeforeSend.Items.Add(midiOutBeforeSend); }
            }
            for (int i = 0; i < dgvMIDIOutList.Length; i++)
            {
                var dgvIn = dgvMIDIinListA;
                DataGridViewComboBoxColumn clmTypeIn = ((DataGridViewComboBoxColumn)dgvIn.Columns["clmTypeIn" + ((char)('A' + 0)).ToString()]);
                clmTypeIn.Items.Clear(); foreach (string midiOutType in midiTypeList) { clmTypeIn.Items.Add(midiOutType); }
            }



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
                    dgvMIDIOutList[i].Rows.Clear();
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
                        string stype = midiTypeList[moi.type];
                        string sbeforeSend = midiOutBeforeSendList[moi.beforeSendType];

                        dgvMIDIOutList[i].Rows.Add(
                            moi.id
                            , moi.isVST
                            , moi.fileName
                            , moi.name
                            , moi.alias
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
                for (int i = 0; i < setting.midiIn.lstMidiInInfo.Count; i++)
                {
                    dgvMIDIInList[i].Rows.Clear();
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
                        string stype = midiTypeList[mii.type];
                        dgvMIDIInList[i].Rows.Add(
                            mii.id
                            , mii.isVST
                            , mii.fileName
                            , mii.name
                            , mii.alias
                            , stype
                            , mii.isVST ? mii.vendor : (mii.manufacturer != -1 ? ((NAudio.Manufacturers)mii.manufacturer).ToString() : "Unknown")
                            );
                    }
                }
            }


            // Playerのオプション

            cbRCPMuteMode.Checked = setting.player.RCPMuteMode;

            rbFast.Checked = setting.player.SkipModeType == 0;
            rbMiddle.Checked = setting.player.SkipModeType == 1;
            rbSlow.Checked = setting.player.SkipModeType == 2;

            // Reset
            tbBeforeSend_GMReset.Text = setting.midiOut.GMReset;
            tbBeforeSend_XGReset.Text = setting.midiOut.XGReset;
            tbBeforeSend_GSReset.Text = setting.midiOut.GSReset;
            tbBeforeSend_Custom.Text = setting.midiOut.Custom;

        }

        private void btnAddMIDIout_Click(object sender, EventArgs e)
        {
            if (dgvMIDIoutPallet.SelectedRows == null || dgvMIDIoutPallet.SelectedRows.Count < 1) return;

            int p = tbcMIDIOutList.SelectedIndex;

            foreach (DataGridViewRow row in dgvMIDIoutPallet.SelectedRows)
            {
                bool found = false;
                foreach (DataGridViewRow r in dgvMIDIOutList[p].Rows)
                {
                    if (r.Cells[3].Value.ToString() == row.Cells[1].Value.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) dgvMIDIOutList[p].Rows.Add(row.Cells[0].Value, false, "", row.Cells[1].Value, row.Cells[1].Value, "GM", "None", row.Cells[2].Value);
            }
        }

        private void btnSubMIDIout_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIOutList.SelectedIndex;

            if (dgvMIDIOutList[p].SelectedRows == null || dgvMIDIOutList[p].SelectedRows.Count < 1) return;

            // 消した後に選択する位置を予め覚えておく
            int selectedIndex = (dgvMIDIOutList[p].SelectedRows.Count > 0) ? (dgvMIDIOutList[p].SelectedRows[0].Index - 1) : -1;

            // 消す
            foreach (DataGridViewRow row in dgvMIDIOutList[p].SelectedRows)
            {
                dgvMIDIOutList[p].Rows.Remove(row);
            }

            // 覚えておいた選択位置を設定する
            if (selectedIndex > -1) dgvMIDIOutList[p].Rows[selectedIndex].Selected = true;
        }

        private void btnUPMIDIOut_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIOutList.SelectedIndex;

            if (dgvMIDIOutList[p].SelectedRows == null || dgvMIDIOutList[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvMIDIOutList[p].SelectedRows)
            {
                if (row.Index < 1) continue;

                int i = row.Index - 1;
                dgvMIDIOutList[p].Rows.Insert(i, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value, row.Cells[6].Value, row.Cells[7].Value, row.Cells[8].Value);
                dgvMIDIOutList[p].Rows.Remove(row);
                dgvMIDIOutList[p].Rows[i].Selected = true;
            }
        }

        private void btnDownMIDIOut_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIOutList.SelectedIndex;

            if (dgvMIDIOutList[p].SelectedRows == null || dgvMIDIOutList[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvMIDIOutList[p].SelectedRows)
            {
                if (row.Index > dgvMIDIOutList[p].Rows.Count - 2) continue;

                int i = row.Index + 1;
                dgvMIDIOutList[p].Rows.Insert(row.Index + 2, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value, row.Cells[6].Value, row.Cells[7].Value, row.Cells[8].Value);
                dgvMIDIOutList[p].Rows.Remove(row);
                dgvMIDIOutList[p].Rows[i].Selected = true;
            }
        }


        private void btnAddMIDIin_Click(object sender, EventArgs e)
        {
            if (dgvMIDIinPallet.SelectedRows == null || dgvMIDIinPallet.SelectedRows.Count < 1) return;

            int p = tbcMIDIinList.SelectedIndex;

            foreach (DataGridViewRow row in dgvMIDIinPallet.SelectedRows)
            {
                bool found = false;
                foreach (DataGridViewRow r in dgvMIDIInList[p].Rows)
                {
                    if (r.Cells[1].Value.ToString() == row.Cells[1].Value.ToString())
                    {
                        found = true;
                        break;
                    }
                }

                if (!found) dgvMIDIInList[p].Rows.Add(row.Cells[0].Value, false, "", row.Cells[1].Value, row.Cells[1].Value, "GM", row.Cells[2].Value);
            }

        }

        private void btnSubMIDIin_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIinList.SelectedIndex;

            if (dgvMIDIInList[p].SelectedRows == null || dgvMIDIInList[p].SelectedRows.Count < 1) return;

            // 消した後に選択する位置を予め覚えておく
            int selectedIndex = (dgvMIDIInList[p].SelectedRows.Count > 0) ? (dgvMIDIInList[p].SelectedRows[0].Index - 1) : -1;

            // 消す
            foreach (DataGridViewRow row in dgvMIDIInList[p].SelectedRows)
            {
                dgvMIDIInList[p].Rows.Remove(row);
            }

            // 覚えておいた選択位置を設定する
            if (selectedIndex > -1) dgvMIDIInList[p].Rows[selectedIndex].Selected = true;
        }

        private void btnUPMIDIin_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIinList.SelectedIndex;

            if (dgvMIDIInList[p].SelectedRows == null || dgvMIDIInList[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvMIDIInList[p].SelectedRows)
            {
                if (row.Index < 1) continue;

                int i = row.Index - 1;
                dgvMIDIInList[p].Rows.Insert(i, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value, row.Cells[6].Value);
                dgvMIDIInList[p].Rows.Remove(row);
                dgvMIDIInList[p].Rows[i].Selected = true;
            }
        }

        private void btnDownMIDIin_A_Click(object sender, EventArgs e)
        {
            int p = tbcMIDIinList.SelectedIndex;

            if (dgvMIDIInList[p].SelectedRows == null || dgvMIDIInList[p].SelectedRows.Count < 1) return;

            foreach (DataGridViewRow row in dgvMIDIInList[p].SelectedRows)
            {
                if (row.Index > dgvMIDIInList[p].Rows.Count - 2) continue;

                int i = row.Index + 1;
                dgvMIDIInList[p].Rows.Insert(row.Index + 2, row.Cells[0].Value, row.Cells[1].Value, row.Cells[2].Value, row.Cells[3].Value, row.Cells[4].Value, row.Cells[5].Value, row.Cells[6].Value);
                dgvMIDIInList[p].Rows.Remove(row);
                dgvMIDIInList[p].Rows[i].Selected = true;
            }

        }

        private void dgvMIDIoutPallet_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            dgvMIDIoutPallet.Rows[e.RowIndex].Selected = true;
            btnAddMIDIout_Click(null, null);
        }

        private void dgvMIDIoutListA_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int p = tbcMIDIOutList.SelectedIndex;
            dgvMIDIOutList[p].Rows[e.RowIndex].Selected = true;
            btnSubMIDIout_Click(null, null);
        }

        private void dgvMIDIinPallet_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            dgvMIDIinPallet.Rows[e.RowIndex].Selected = true;
            btnAddMIDIin_Click(null, null);
        }

        private void dgvMIDIinListA_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int p = tbcMIDIinList.SelectedIndex;
            dgvMIDIInList[p].Rows[e.RowIndex].Selected = true;
            btnSubMIDIin_Click(null, null);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int i;

            // 設定ファイルに保存するために、MIDIデバイスの設定内容をSettingクラスに格納する

            setting.midiOut.lstMidiOutInfo = new List<Setting.midiOutInfo[]>();
            foreach (DataGridView d in dgvMIDIOutList)
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
                    moi.alias = (string)d.Rows[i].Cells[4].Value;
                    string stype = (string)d.Rows[i].Cells[5].Value;
                    //GM / XG / GS / LA / GS(SC - 55_1) / GS(SC - 55_2)
                    moi.type = 0;
                    if (stype == "XG") moi.type = 1;
                    else if (stype == "GS") moi.type = 2;
                    else if (stype == "LA") moi.type = 3;
                    else if (stype == "GS(SC - 55_1)") moi.type = 4;
                    else if (stype == "GS(SC - 55_2)") moi.type = 5;
                    string sbeforeSend = (string)d.Rows[i].Cells[6].Value;
                    moi.beforeSendType = 0;
                    if (sbeforeSend == "GM Reset") moi.beforeSendType = 1;
                    else if (sbeforeSend == "XG Reset") moi.beforeSendType = 2;
                    else if (sbeforeSend == "GS Reset") moi.beforeSendType = 3;
                    else if (sbeforeSend == "Custom") moi.beforeSendType = 4;

                    string mn = (string)d.Rows[i].Cells[7].Value;
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
            foreach (DataGridView d in dgvMIDIInList)
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
                    mii.alias = (string)d.Rows[i].Cells[4].Value;
                    string stype = (string)d.Rows[i].Cells[5].Value;
                    //GM / XG / GS / LA / GS(SC - 55_1) / GS(SC - 55_2)
                    mii.type = 0;
                    if (stype == "XG") mii.type = 1;
                    else if (stype == "GS") mii.type = 2;
                    else if (stype == "LA") mii.type = 3;
                    else if (stype == "GS(SC - 55_1)") mii.type = 4;
                    else if (stype == "GS(SC - 55_2)") mii.type = 5;
                    string mn = (string)d.Rows[i].Cells[6].Value;
                    mii.vendor = mn;
                    mii.manufacturer = -1;
                    if (!mii.isVST)
                    {
                        mii.vendor = "";
                        if (Enum.TryParse(typeof(NAudio.Manufacturers), mn, out var manufacturerEnum))
                            mii.manufacturer = (int)manufacturerEnum;
                        else
                            mii.manufacturer = -1; // Unknown manufacturer
                    }
                    lstMii.Add(mii);
                }
                setting.midiIn.lstMidiInInfo.Add(lstMii.ToArray());
            }

            setting.player.RCPMuteMode = cbRCPMuteMode.Checked;
            setting.player.SkipModeType = rbFast.Checked ? 0 : (rbMiddle.Checked ? 1 : 2);

            setting.midiOut.GMReset = tbBeforeSend_GMReset.Text;
            setting.midiOut.XGReset = tbBeforeSend_XGReset.Text;
            setting.midiOut.GSReset = tbBeforeSend_GSReset.Text;
            setting.midiOut.Custom = tbBeforeSend_Custom.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        public Setting GetSetting()
        {
            return setting;
        }

        private void btnBeforeSend_Default_Click(object sender, EventArgs e)
        {
            Setting.MidiOut mo = new Setting.MidiOut();
            tbBeforeSend_GMReset.Text = mo.GMReset;
            tbBeforeSend_XGReset.Text = mo.XGReset;
            tbBeforeSend_GSReset.Text = mo.GSReset;
            tbBeforeSend_Custom.Text = mo.Custom;
        }
    }
}
