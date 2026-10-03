using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net.ServerSentEvents;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.SysExEditorPanel
{
    public partial class SysExEditorForm : Form
    {
        public byte[] EditedData { get; private set; } = Array.Empty<byte>();
        public string EditedName { get; private set; } = "";

        public SysExEditorForm()
        {
            InitializeComponent();

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
        }

        private void OnAddSysEx(object sender, EventArgs e)
        {
            var parent = treeView1.SelectedNode;
            if (parent == null) return;

            var newNode = parent.Nodes.Add("New SysEx");
            newNode.Tag = new SysExItem(); // 空の SysEx データ
            treeView1.SelectedNode = newNode;
            newNode.BeginEdit(); // 名前編集開始
        }

        private void OnDeleteSysEx(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null) return;

            // プリセットは削除不可にするならここで判定
            node.Remove();
        }

        private void OnSetEvent(object sender, EventArgs e)
        {
            {
                var node = treeView1.SelectedNode;
                if (node == null || node.Tag == null) return;

                var item = (SysExItem)node.Tag;

                // ここで SysExEditResult を返す
                this.EditedName = item.Name;
                this.EditedData = ToBytes(item.HexString);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null) return; // キャンセル

            if (e.Node == null) return;
            if (e.Node.Tag == null) return;
            var item = (SysExItem)e.Node.Tag;
            item.Name = e.Label;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null) return;
            if (e.Node.Tag == null) return;
            var item = (SysExItem)e.Node.Tag;
            label2.Text = item.Name;
            hexTextBox.Text = item.HexString;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null) return;
            if (node.Tag == null) return;
            var item = (SysExItem)node.Tag;

            label2.Text = item.Name;
            item.HexString = hexTextBox.Text;
        }

        public byte[] ToBytes(string HexString)
        {
            var parts = HexString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var bytes = new List<byte>();
            foreach (var p in parts)
                bytes.Add(Convert.ToByte(p, 16));
            return bytes.ToArray();
        }

    }
}
