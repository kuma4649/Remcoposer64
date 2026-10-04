namespace Remcoposer64.App.form
{
    partial class frmSetting
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tcSetting = new TabControl();
            tpOutput = new TabPage();
            tcDeviceCategory = new TabControl();
            tpMIDIOut = new TabPage();
            scMIDIOutSplit = new SplitContainer();
            label16 = new Label();
            dgvMIDIoutPallet = new DataGridView();
            clmID = new DataGridViewTextBoxColumn();
            clmDeviceName = new DataGridViewTextBoxColumn();
            clmManufacturer = new DataGridViewTextBoxColumn();
            clmSpacer = new DataGridViewTextBoxColumn();
            tbcMIDIoutList = new TabControl();
            tabPage1 = new TabPage();
            dgvMIDIoutListA = new DataGridView();
            dataGridViewTextBoxColumn1 = new DataGridViewTextBoxColumn();
            clmIsVST = new DataGridViewCheckBoxColumn();
            clmFileName = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn2 = new DataGridViewTextBoxColumn();
            clmType = new DataGridViewComboBoxColumn();
            ClmBeforeSend = new DataGridViewComboBoxColumn();
            dataGridViewTextBoxColumn3 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn4 = new DataGridViewTextBoxColumn();
            btnUPMIDIOut_A = new Button();
            btnDownMIDIOut_A = new Button();
            btnAddMIDIout = new Button();
            label18 = new Label();
            btnAddVSTiOut = new Button();
            btnSubMIDIout = new Button();
            tpMIDIIN = new TabPage();
            splitContainer1 = new SplitContainer();
            label1 = new Label();
            dgvMIDIinPallet = new DataGridView();
            dataGridViewTextBoxColumn5 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn6 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn7 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn8 = new DataGridViewTextBoxColumn();
            tbcMIDIinList = new TabControl();
            tabPage3 = new TabPage();
            dgvMIDIinListA = new DataGridView();
            dataGridViewTextBoxColumn9 = new DataGridViewTextBoxColumn();
            dataGridViewCheckBoxColumn1 = new DataGridViewCheckBoxColumn();
            dataGridViewTextBoxColumn10 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn11 = new DataGridViewTextBoxColumn();
            dataGridViewComboBoxColumn1 = new DataGridViewComboBoxColumn();
            dataGridViewTextBoxColumn12 = new DataGridViewTextBoxColumn();
            dataGridViewTextBoxColumn13 = new DataGridViewTextBoxColumn();
            btnUPMIDIin_A = new Button();
            btnDownMIDIin_A = new Button();
            btnAddVSTiIn = new Button();
            label2 = new Label();
            btnAddMIDIin = new Button();
            btnSubMIDIin = new Button();
            btnOK = new Button();
            btnCancel = new Button();
            tcSetting.SuspendLayout();
            tpOutput.SuspendLayout();
            tcDeviceCategory.SuspendLayout();
            tpMIDIOut.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)scMIDIOutSplit).BeginInit();
            scMIDIOutSplit.Panel1.SuspendLayout();
            scMIDIOutSplit.Panel2.SuspendLayout();
            scMIDIOutSplit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMIDIoutPallet).BeginInit();
            tbcMIDIoutList.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMIDIoutListA).BeginInit();
            tpMIDIIN.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMIDIinPallet).BeginInit();
            tbcMIDIinList.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMIDIinListA).BeginInit();
            SuspendLayout();
            // 
            // tcSetting
            // 
            tcSetting.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tcSetting.Controls.Add(tpOutput);
            tcSetting.Location = new Point(12, 12);
            tcSetting.Name = "tcSetting";
            tcSetting.SelectedIndex = 0;
            tcSetting.Size = new Size(520, 548);
            tcSetting.TabIndex = 0;
            // 
            // tpOutput
            // 
            tpOutput.Controls.Add(tcDeviceCategory);
            tpOutput.Location = new Point(4, 24);
            tpOutput.Name = "tpOutput";
            tpOutput.Padding = new Padding(3);
            tpOutput.Size = new Size(512, 520);
            tpOutput.TabIndex = 0;
            tpOutput.Text = "Output";
            tpOutput.UseVisualStyleBackColor = true;
            // 
            // tcDeviceCategory
            // 
            tcDeviceCategory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tcDeviceCategory.Controls.Add(tpMIDIOut);
            tcDeviceCategory.Controls.Add(tpMIDIIN);
            tcDeviceCategory.Location = new Point(6, 6);
            tcDeviceCategory.Name = "tcDeviceCategory";
            tcDeviceCategory.SelectedIndex = 0;
            tcDeviceCategory.Size = new Size(500, 508);
            tcDeviceCategory.TabIndex = 0;
            // 
            // tpMIDIOut
            // 
            tpMIDIOut.Controls.Add(scMIDIOutSplit);
            tpMIDIOut.Location = new Point(4, 24);
            tpMIDIOut.Name = "tpMIDIOut";
            tpMIDIOut.Padding = new Padding(3);
            tpMIDIOut.Size = new Size(492, 480);
            tpMIDIOut.TabIndex = 0;
            tpMIDIOut.Text = "MIDI Out";
            tpMIDIOut.UseVisualStyleBackColor = true;
            // 
            // scMIDIOutSplit
            // 
            scMIDIOutSplit.Dock = DockStyle.Fill;
            scMIDIOutSplit.Location = new Point(3, 3);
            scMIDIOutSplit.Margin = new Padding(4);
            scMIDIOutSplit.Name = "scMIDIOutSplit";
            scMIDIOutSplit.Orientation = Orientation.Horizontal;
            // 
            // scMIDIOutSplit.Panel1
            // 
            scMIDIOutSplit.Panel1.Controls.Add(label16);
            scMIDIOutSplit.Panel1.Controls.Add(dgvMIDIoutPallet);
            // 
            // scMIDIOutSplit.Panel2
            // 
            scMIDIOutSplit.Panel2.Controls.Add(tbcMIDIoutList);
            scMIDIOutSplit.Panel2.Controls.Add(btnAddMIDIout);
            scMIDIOutSplit.Panel2.Controls.Add(label18);
            scMIDIOutSplit.Panel2.Controls.Add(btnAddVSTiOut);
            scMIDIOutSplit.Panel2.Controls.Add(btnSubMIDIout);
            scMIDIOutSplit.Size = new Size(486, 474);
            scMIDIOutSplit.SplitterDistance = 187;
            scMIDIOutSplit.SplitterWidth = 5;
            scMIDIOutSplit.TabIndex = 7;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(4, 0);
            label16.Margin = new Padding(4, 0, 4, 0);
            label16.Name = "label16";
            label16.Size = new Size(124, 15);
            label16.TabIndex = 0;
            label16.Text = "MIDI Out device pallet";
            // 
            // dgvMIDIoutPallet
            // 
            dgvMIDIoutPallet.AllowUserToAddRows = false;
            dgvMIDIoutPallet.AllowUserToDeleteRows = false;
            dgvMIDIoutPallet.AllowUserToResizeRows = false;
            dgvMIDIoutPallet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMIDIoutPallet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMIDIoutPallet.Columns.AddRange(new DataGridViewColumn[] { clmID, clmDeviceName, clmManufacturer, clmSpacer });
            dgvMIDIoutPallet.Location = new Point(2, 19);
            dgvMIDIoutPallet.Margin = new Padding(4);
            dgvMIDIoutPallet.MultiSelect = false;
            dgvMIDIoutPallet.Name = "dgvMIDIoutPallet";
            dgvMIDIoutPallet.RowHeadersVisible = false;
            dgvMIDIoutPallet.RowTemplate.Height = 21;
            dgvMIDIoutPallet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMIDIoutPallet.Size = new Size(480, 164);
            dgvMIDIoutPallet.TabIndex = 1;
            // 
            // clmID
            // 
            clmID.Frozen = true;
            clmID.HeaderText = "ID";
            clmID.Name = "clmID";
            clmID.ReadOnly = true;
            clmID.Visible = false;
            clmID.Width = 40;
            // 
            // clmDeviceName
            // 
            clmDeviceName.Frozen = true;
            clmDeviceName.HeaderText = "Device Name";
            clmDeviceName.Name = "clmDeviceName";
            clmDeviceName.ReadOnly = true;
            clmDeviceName.SortMode = DataGridViewColumnSortMode.NotSortable;
            clmDeviceName.Width = 200;
            // 
            // clmManufacturer
            // 
            clmManufacturer.Frozen = true;
            clmManufacturer.HeaderText = "Manufacturer";
            clmManufacturer.Name = "clmManufacturer";
            clmManufacturer.ReadOnly = true;
            clmManufacturer.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // clmSpacer
            // 
            clmSpacer.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            clmSpacer.HeaderText = "";
            clmSpacer.Name = "clmSpacer";
            clmSpacer.ReadOnly = true;
            clmSpacer.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // tbcMIDIoutList
            // 
            tbcMIDIoutList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbcMIDIoutList.Controls.Add(tabPage1);
            tbcMIDIoutList.Location = new Point(4, 39);
            tbcMIDIoutList.Margin = new Padding(4);
            tbcMIDIoutList.Name = "tbcMIDIoutList";
            tbcMIDIoutList.SelectedIndex = 0;
            tbcMIDIoutList.Size = new Size(478, 227);
            tbcMIDIoutList.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(dgvMIDIoutListA);
            tabPage1.Controls.Add(btnUPMIDIOut_A);
            tabPage1.Controls.Add(btnDownMIDIOut_A);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Margin = new Padding(4);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(4);
            tabPage1.Size = new Size(470, 199);
            tabPage1.TabIndex = 0;
            tabPage1.Tag = "0";
            tabPage1.Text = "...";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // dgvMIDIoutListA
            // 
            dgvMIDIoutListA.AllowUserToAddRows = false;
            dgvMIDIoutListA.AllowUserToDeleteRows = false;
            dgvMIDIoutListA.AllowUserToResizeRows = false;
            dgvMIDIoutListA.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMIDIoutListA.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMIDIoutListA.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn1, clmIsVST, clmFileName, dataGridViewTextBoxColumn2, clmType, ClmBeforeSend, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn4 });
            dgvMIDIoutListA.Location = new Point(4, 4);
            dgvMIDIoutListA.Margin = new Padding(4);
            dgvMIDIoutListA.MultiSelect = false;
            dgvMIDIoutListA.Name = "dgvMIDIoutListA";
            dgvMIDIoutListA.RowHeadersVisible = false;
            dgvMIDIoutListA.RowTemplate.Height = 21;
            dgvMIDIoutListA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMIDIoutListA.Size = new Size(428, 191);
            dgvMIDIoutListA.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn1
            // 
            dataGridViewTextBoxColumn1.Frozen = true;
            dataGridViewTextBoxColumn1.HeaderText = "ID";
            dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            dataGridViewTextBoxColumn1.ReadOnly = true;
            dataGridViewTextBoxColumn1.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn1.Visible = false;
            dataGridViewTextBoxColumn1.Width = 40;
            // 
            // clmIsVST
            // 
            clmIsVST.HeaderText = "IsVST";
            clmIsVST.Name = "clmIsVST";
            clmIsVST.Visible = false;
            // 
            // clmFileName
            // 
            clmFileName.HeaderText = "fileName";
            clmFileName.Name = "clmFileName";
            clmFileName.Resizable = DataGridViewTriState.True;
            clmFileName.SortMode = DataGridViewColumnSortMode.NotSortable;
            clmFileName.Visible = false;
            // 
            // dataGridViewTextBoxColumn2
            // 
            dataGridViewTextBoxColumn2.HeaderText = "Device Name";
            dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            dataGridViewTextBoxColumn2.ReadOnly = true;
            dataGridViewTextBoxColumn2.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn2.Width = 180;
            // 
            // clmType
            // 
            clmType.HeaderText = "Type";
            clmType.Items.AddRange(new object[] { "GM" });
            clmType.Name = "clmType";
            clmType.Resizable = DataGridViewTriState.True;
            clmType.Width = 70;
            // 
            // ClmBeforeSend
            // 
            ClmBeforeSend.HeaderText = "Before Send";
            ClmBeforeSend.Items.AddRange(new object[] { "None", "GM Reset", "XG Reset", "GS Reset", "Custom" });
            ClmBeforeSend.Name = "ClmBeforeSend";
            ClmBeforeSend.Resizable = DataGridViewTriState.True;
            ClmBeforeSend.SortMode = DataGridViewColumnSortMode.Automatic;
            // 
            // dataGridViewTextBoxColumn3
            // 
            dataGridViewTextBoxColumn3.HeaderText = "Manufacturer";
            dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            dataGridViewTextBoxColumn3.ReadOnly = true;
            dataGridViewTextBoxColumn3.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn4
            // 
            dataGridViewTextBoxColumn4.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn4.HeaderText = "";
            dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            dataGridViewTextBoxColumn4.ReadOnly = true;
            dataGridViewTextBoxColumn4.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // btnUPMIDIOut_A
            // 
            btnUPMIDIOut_A.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUPMIDIOut_A.Location = new Point(440, 4);
            btnUPMIDIOut_A.Margin = new Padding(4);
            btnUPMIDIOut_A.Name = "btnUPMIDIOut_A";
            btnUPMIDIOut_A.Size = new Size(26, 72);
            btnUPMIDIOut_A.TabIndex = 3;
            btnUPMIDIOut_A.Text = "↑";
            btnUPMIDIOut_A.UseVisualStyleBackColor = true;
            btnUPMIDIOut_A.Click += btnUPMIDIOut_A_Click;
            // 
            // btnDownMIDIOut_A
            // 
            btnDownMIDIOut_A.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDownMIDIOut_A.Location = new Point(440, 123);
            btnDownMIDIOut_A.Margin = new Padding(4);
            btnDownMIDIOut_A.Name = "btnDownMIDIOut_A";
            btnDownMIDIOut_A.Size = new Size(26, 72);
            btnDownMIDIOut_A.TabIndex = 3;
            btnDownMIDIOut_A.Text = "↓";
            btnDownMIDIOut_A.UseVisualStyleBackColor = true;
            btnDownMIDIOut_A.Click += btnDownMIDIOut_A_Click;
            // 
            // btnAddMIDIout
            // 
            btnAddMIDIout.Location = new Point(138, 5);
            btnAddMIDIout.Margin = new Padding(4);
            btnAddMIDIout.Name = "btnAddMIDIout";
            btnAddMIDIout.Size = new Size(76, 30);
            btnAddMIDIout.TabIndex = 3;
            btnAddMIDIout.Text = "↓ +";
            btnAddMIDIout.UseVisualStyleBackColor = true;
            btnAddMIDIout.Click += btnAddMIDIout_Click;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(7, 20);
            label18.Margin = new Padding(4, 0, 4, 0);
            label18.Name = "label18";
            label18.Size = new Size(73, 15);
            label18.TabIndex = 2;
            label18.Text = "MIDI Out list";
            // 
            // btnAddVSTiOut
            // 
            btnAddVSTiOut.Enabled = false;
            btnAddVSTiOut.Location = new Point(307, 5);
            btnAddVSTiOut.Margin = new Padding(4);
            btnAddVSTiOut.Name = "btnAddVSTiOut";
            btnAddVSTiOut.Size = new Size(76, 29);
            btnAddVSTiOut.TabIndex = 5;
            btnAddVSTiOut.Text = "Add VSTi";
            btnAddVSTiOut.UseVisualStyleBackColor = true;
            btnAddVSTiOut.Visible = false;
            // 
            // btnSubMIDIout
            // 
            btnSubMIDIout.Location = new Point(222, 5);
            btnSubMIDIout.Margin = new Padding(4);
            btnSubMIDIout.Name = "btnSubMIDIout";
            btnSubMIDIout.Size = new Size(77, 30);
            btnSubMIDIout.TabIndex = 3;
            btnSubMIDIout.Text = "-";
            btnSubMIDIout.UseVisualStyleBackColor = true;
            btnSubMIDIout.Click += btnSubMIDIout_Click;
            // 
            // tpMIDIIN
            // 
            tpMIDIIN.Controls.Add(splitContainer1);
            tpMIDIIN.Location = new Point(4, 24);
            tpMIDIIN.Name = "tpMIDIIN";
            tpMIDIIN.Padding = new Padding(3);
            tpMIDIIN.Size = new Size(492, 480);
            tpMIDIIN.TabIndex = 1;
            tpMIDIIN.Text = "MIDI In";
            tpMIDIIN.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Margin = new Padding(4);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(dgvMIDIinPallet);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(tbcMIDIinList);
            splitContainer1.Panel2.Controls.Add(btnAddVSTiIn);
            splitContainer1.Panel2.Controls.Add(label2);
            splitContainer1.Panel2.Controls.Add(btnAddMIDIin);
            splitContainer1.Panel2.Controls.Add(btnSubMIDIin);
            splitContainer1.Size = new Size(486, 474);
            splitContainer1.SplitterDistance = 187;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 8;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 0);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(114, 15);
            label1.TabIndex = 0;
            label1.Text = "MIDI In device pallet";
            // 
            // dgvMIDIinPallet
            // 
            dgvMIDIinPallet.AllowUserToAddRows = false;
            dgvMIDIinPallet.AllowUserToDeleteRows = false;
            dgvMIDIinPallet.AllowUserToResizeRows = false;
            dgvMIDIinPallet.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMIDIinPallet.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMIDIinPallet.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn5, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn7, dataGridViewTextBoxColumn8 });
            dgvMIDIinPallet.Location = new Point(2, 19);
            dgvMIDIinPallet.Margin = new Padding(4);
            dgvMIDIinPallet.MultiSelect = false;
            dgvMIDIinPallet.Name = "dgvMIDIinPallet";
            dgvMIDIinPallet.RowHeadersVisible = false;
            dgvMIDIinPallet.RowTemplate.Height = 21;
            dgvMIDIinPallet.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMIDIinPallet.Size = new Size(480, 164);
            dgvMIDIinPallet.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn5
            // 
            dataGridViewTextBoxColumn5.Frozen = true;
            dataGridViewTextBoxColumn5.HeaderText = "ID";
            dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            dataGridViewTextBoxColumn5.ReadOnly = true;
            dataGridViewTextBoxColumn5.Visible = false;
            dataGridViewTextBoxColumn5.Width = 40;
            // 
            // dataGridViewTextBoxColumn6
            // 
            dataGridViewTextBoxColumn6.Frozen = true;
            dataGridViewTextBoxColumn6.HeaderText = "Device Name";
            dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            dataGridViewTextBoxColumn6.ReadOnly = true;
            dataGridViewTextBoxColumn6.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn6.Width = 200;
            // 
            // dataGridViewTextBoxColumn7
            // 
            dataGridViewTextBoxColumn7.Frozen = true;
            dataGridViewTextBoxColumn7.HeaderText = "Manufacturer";
            dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            dataGridViewTextBoxColumn7.ReadOnly = true;
            dataGridViewTextBoxColumn7.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn8
            // 
            dataGridViewTextBoxColumn8.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn8.HeaderText = "";
            dataGridViewTextBoxColumn8.Name = "dataGridViewTextBoxColumn8";
            dataGridViewTextBoxColumn8.ReadOnly = true;
            dataGridViewTextBoxColumn8.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // tbcMIDIinList
            // 
            tbcMIDIinList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tbcMIDIinList.Controls.Add(tabPage3);
            tbcMIDIinList.Location = new Point(4, 39);
            tbcMIDIinList.Margin = new Padding(4);
            tbcMIDIinList.Name = "tbcMIDIinList";
            tbcMIDIinList.SelectedIndex = 0;
            tbcMIDIinList.Size = new Size(478, 227);
            tbcMIDIinList.TabIndex = 4;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(dgvMIDIinListA);
            tabPage3.Controls.Add(btnUPMIDIin_A);
            tabPage3.Controls.Add(btnDownMIDIin_A);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Margin = new Padding(4);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(4);
            tabPage3.Size = new Size(470, 199);
            tabPage3.TabIndex = 0;
            tabPage3.Tag = "0";
            tabPage3.Text = "...";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // dgvMIDIinListA
            // 
            dgvMIDIinListA.AllowUserToAddRows = false;
            dgvMIDIinListA.AllowUserToDeleteRows = false;
            dgvMIDIinListA.AllowUserToResizeRows = false;
            dgvMIDIinListA.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvMIDIinListA.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMIDIinListA.Columns.AddRange(new DataGridViewColumn[] { dataGridViewTextBoxColumn9, dataGridViewCheckBoxColumn1, dataGridViewTextBoxColumn10, dataGridViewTextBoxColumn11, dataGridViewComboBoxColumn1, dataGridViewTextBoxColumn12, dataGridViewTextBoxColumn13 });
            dgvMIDIinListA.Location = new Point(4, 4);
            dgvMIDIinListA.Margin = new Padding(4);
            dgvMIDIinListA.MultiSelect = false;
            dgvMIDIinListA.Name = "dgvMIDIinListA";
            dgvMIDIinListA.RowHeadersVisible = false;
            dgvMIDIinListA.RowTemplate.Height = 21;
            dgvMIDIinListA.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMIDIinListA.Size = new Size(428, 191);
            dgvMIDIinListA.TabIndex = 1;
            // 
            // dataGridViewTextBoxColumn9
            // 
            dataGridViewTextBoxColumn9.Frozen = true;
            dataGridViewTextBoxColumn9.HeaderText = "ID";
            dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            dataGridViewTextBoxColumn9.ReadOnly = true;
            dataGridViewTextBoxColumn9.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn9.Visible = false;
            dataGridViewTextBoxColumn9.Width = 40;
            // 
            // dataGridViewCheckBoxColumn1
            // 
            dataGridViewCheckBoxColumn1.HeaderText = "IsVST";
            dataGridViewCheckBoxColumn1.Name = "dataGridViewCheckBoxColumn1";
            dataGridViewCheckBoxColumn1.Visible = false;
            // 
            // dataGridViewTextBoxColumn10
            // 
            dataGridViewTextBoxColumn10.HeaderText = "fileName";
            dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            dataGridViewTextBoxColumn10.Resizable = DataGridViewTriState.True;
            dataGridViewTextBoxColumn10.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn10.Visible = false;
            // 
            // dataGridViewTextBoxColumn11
            // 
            dataGridViewTextBoxColumn11.HeaderText = "Device Name";
            dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            dataGridViewTextBoxColumn11.ReadOnly = true;
            dataGridViewTextBoxColumn11.SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewTextBoxColumn11.Width = 180;
            // 
            // dataGridViewComboBoxColumn1
            // 
            dataGridViewComboBoxColumn1.HeaderText = "Type";
            dataGridViewComboBoxColumn1.Items.AddRange(new object[] { "GM" });
            dataGridViewComboBoxColumn1.Name = "dataGridViewComboBoxColumn1";
            dataGridViewComboBoxColumn1.Resizable = DataGridViewTriState.True;
            dataGridViewComboBoxColumn1.Width = 70;
            // 
            // dataGridViewTextBoxColumn12
            // 
            dataGridViewTextBoxColumn12.HeaderText = "Manufacturer";
            dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            dataGridViewTextBoxColumn12.ReadOnly = true;
            dataGridViewTextBoxColumn12.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // dataGridViewTextBoxColumn13
            // 
            dataGridViewTextBoxColumn13.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewTextBoxColumn13.HeaderText = "";
            dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            dataGridViewTextBoxColumn13.ReadOnly = true;
            dataGridViewTextBoxColumn13.SortMode = DataGridViewColumnSortMode.NotSortable;
            // 
            // btnUPMIDIin_A
            // 
            btnUPMIDIin_A.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUPMIDIin_A.Location = new Point(440, 4);
            btnUPMIDIin_A.Margin = new Padding(4);
            btnUPMIDIin_A.Name = "btnUPMIDIin_A";
            btnUPMIDIin_A.Size = new Size(26, 72);
            btnUPMIDIin_A.TabIndex = 3;
            btnUPMIDIin_A.Text = "↑";
            btnUPMIDIin_A.UseVisualStyleBackColor = true;
            btnUPMIDIin_A.Click += btnUPMIDIin_A_Click;
            // 
            // btnDownMIDIin_A
            // 
            btnDownMIDIin_A.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDownMIDIin_A.Location = new Point(440, 123);
            btnDownMIDIin_A.Margin = new Padding(4);
            btnDownMIDIin_A.Name = "btnDownMIDIin_A";
            btnDownMIDIin_A.Size = new Size(26, 72);
            btnDownMIDIin_A.TabIndex = 3;
            btnDownMIDIin_A.Text = "↓";
            btnDownMIDIin_A.UseVisualStyleBackColor = true;
            btnDownMIDIin_A.Click += btnDownMIDIin_A_Click;
            // 
            // btnAddVSTiIn
            // 
            btnAddVSTiIn.Enabled = false;
            btnAddVSTiIn.Location = new Point(307, 5);
            btnAddVSTiIn.Margin = new Padding(4);
            btnAddVSTiIn.Name = "btnAddVSTiIn";
            btnAddVSTiIn.Size = new Size(76, 29);
            btnAddVSTiIn.TabIndex = 5;
            btnAddVSTiIn.Text = "Add VSTi";
            btnAddVSTiIn.UseVisualStyleBackColor = true;
            btnAddVSTiIn.Visible = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(7, 20);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 2;
            label2.Text = "MIDI In list";
            // 
            // btnAddMIDIin
            // 
            btnAddMIDIin.Location = new Point(138, 5);
            btnAddMIDIin.Margin = new Padding(4);
            btnAddMIDIin.Name = "btnAddMIDIin";
            btnAddMIDIin.Size = new Size(76, 30);
            btnAddMIDIin.TabIndex = 3;
            btnAddMIDIin.Text = "↓ +";
            btnAddMIDIin.UseVisualStyleBackColor = true;
            btnAddMIDIin.Click += btnAddMIDIin_Click;
            // 
            // btnSubMIDIin
            // 
            btnSubMIDIin.Location = new Point(222, 5);
            btnSubMIDIin.Margin = new Padding(4);
            btnSubMIDIin.Name = "btnSubMIDIin";
            btnSubMIDIin.Size = new Size(77, 30);
            btnSubMIDIin.TabIndex = 3;
            btnSubMIDIin.Text = "-";
            btnSubMIDIin.UseVisualStyleBackColor = true;
            btnSubMIDIin.Click += btnSubMIDIin_Click;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(372, 566);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(453, 566);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // frmSetting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(544, 601);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(tcSetting);
            MinimumSize = new Size(560, 640);
            Name = "frmSetting";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Setting";
            tcSetting.ResumeLayout(false);
            tpOutput.ResumeLayout(false);
            tcDeviceCategory.ResumeLayout(false);
            tpMIDIOut.ResumeLayout(false);
            scMIDIOutSplit.Panel1.ResumeLayout(false);
            scMIDIOutSplit.Panel1.PerformLayout();
            scMIDIOutSplit.Panel2.ResumeLayout(false);
            scMIDIOutSplit.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)scMIDIOutSplit).EndInit();
            scMIDIOutSplit.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMIDIoutPallet).EndInit();
            tbcMIDIoutList.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMIDIoutListA).EndInit();
            tpMIDIIN.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMIDIinPallet).EndInit();
            tbcMIDIinList.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMIDIinListA).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tcSetting;
        private TabPage tpOutput;
        private Button btnOK;
        private Button btnCancel;
        private TabControl tcDeviceCategory;
        private TabPage tpMIDIOut;
        private TabPage tpMIDIIN;
        private SplitContainer scMIDIOutSplit;
        private Label label16;
        private DataGridView dgvMIDIoutPallet;
        private DataGridViewTextBoxColumn clmID;
        private DataGridViewTextBoxColumn clmDeviceName;
        private DataGridViewTextBoxColumn clmManufacturer;
        private DataGridViewTextBoxColumn clmSpacer;
        private TabControl tbcMIDIoutList;
        private TabPage tabPage1;
        private DataGridView dgvMIDIoutListA;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private DataGridViewCheckBoxColumn clmIsVST;
        private DataGridViewTextBoxColumn clmFileName;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private DataGridViewComboBoxColumn clmType;
        private DataGridViewComboBoxColumn ClmBeforeSend;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private Button btnUPMIDIOut_A;
        private Button btnDownMIDIOut_A;
        private Button btnAddVSTiOut;
        private Label label18;
        private Button btnAddMIDIout;
        private Button btnSubMIDIout;
        private SplitContainer splitContainer1;
        private Label label1;
        private DataGridView dgvMIDIinPallet;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn8;
        private TabControl tbcMIDIinList;
        private TabPage tabPage3;
        private DataGridView dgvMIDIinListA;
        private Button btnUPMIDIin_A;
        private Button btnDownMIDIin_A;
        private Button btnAddVSTiIn;
        private Label label2;
        private Button btnAddMIDIin;
        private Button btnSubMIDIin;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private DataGridViewCheckBoxColumn dataGridViewCheckBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private DataGridViewComboBoxColumn dataGridViewComboBoxColumn1;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
    }
}