using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Remcoposer64.StepEditorPanelControl
{
    public partial class ProgramChangeCandidateForm : Form
    {
        private StepEditorPanel parent;
        private int selectedIndex = 0;
        private Label[] labels = new Label[128];

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return cp;
            }
        }

        protected override bool ShowWithoutActivation => true;

        // デザイナ向けコンストラクタ
        public ProgramChangeCandidateForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            this.Visible = false;
        }

        public ProgramChangeCandidateForm(StepEditorPanel parent, Dictionary<int, string> ccNames)
        {
            this.parent = parent;

            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();

            this.Width = 980;   // 960 + 余白
            this.Height = 400;  // 352 + 余白
            this.AutoScroll = true;

            // FlowLayoutPanel の正しい設定
            flowLayoutPanel1.WrapContents = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Padding = new Padding(0);
            flowLayoutPanel1.Margin = new Padding(0);

            flowLayoutPanel1.SuspendLayout();
            // 128個のLabelを事前生成して敷き詰める
            for (int i = 0; i < 128; i++)
            {
                string name = ccNames.TryGetValue(i, out var n) ? n : "Program";

                var lbl = new Label();
                lbl.AutoSize = false;
                lbl.Size = new Size(120, 22);
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Margin = new Padding(1);
                lbl.Padding = new Padding(4, 0, 0, 0);
                lbl.BackColor = Color.Black;
                lbl.ForeColor = Color.White;
                lbl.MouseEnter += (s, e) =>
                {
                    int idx = Array.IndexOf(labels, lbl);
                    selectedIndex = idx;
                    HighlightSelection();
                };

                lbl.Click += (s, e) =>
                {
                    int idx = Array.IndexOf(labels, lbl);
                    selectedIndex = idx;
                    CommitSelection();
                };

                lbl.Text = $"{i:D3}:{name}";

                labels[i] = lbl;
                flowLayoutPanel1.Controls.Add(lbl);
            }
            flowLayoutPanel1.ResumeLayout();

        }

        public void FillCandidates(string inputValue)
        {
            Span<bool> visible = stackalloc bool[128];

            // マッチング
            if (string.IsNullOrEmpty(inputValue))
            {
                for (int i = 0; i < 128; i++)
                    visible[i] = true;
            }
            else
            {
                if (int.TryParse(inputValue, out int n))
                {
                    string v = inputValue.Trim();
                    if (v.Length > 3) v = v.Substring(v.Length - 3);

                    for (int i = 0; i < 128; i++)
                    {
                        bool match = true;

                        string numStr3 = i.ToString("D3");
                        string numStr = i.ToString("D3").Substring(0, v.Length);

                        for (int j = 0; j < 4 - v.Length; j++)
                        {
                            if (int.Parse(v) == int.Parse(numStr)) break;

                            if ((j == 3 - v.Length) || (numStr[0] != '0'))
                            {
                                match = false;
                                break;
                            }

                            numStr = numStr3.Substring(j + 1, v.Length);
                        }

                        visible[i] = match;
                    }
                }
            }

            // ★ Visible 切り替え（FlowLayoutPanel なら高速）
            flowLayoutPanel1.SuspendLayout();
            for (int i = 0; i < 128; i++)
            {
                labels[i].Visible = visible[i];
                if (int.TryParse(inputValue, out int n) && n == i) selectedIndex = i;
                if (i == selectedIndex)
                {
                    labels[i].BackColor = Color.LightBlue;
                    labels[i].ForeColor = Color.Black;
                }
                else
                {
                    labels[i].BackColor = Color.Black;
                    labels[i].ForeColor = Color.White;
                }
            }
            flowLayoutPanel1.ResumeLayout();

            // ★ 高さ調整（8列固定）
            int count = 0;
            for (int i = 0; i < 128; i++)
                if (visible[i]) count++;

            int rowCount = (count + 7) / 8;
            this.Height = rowCount * 24 + 10;
        }

        public void MoveSelection(Keys key)
        {
            // visible なラベル一覧を作る
            List<int> visibleList = new List<int>();
            for (int i = 0; i < labels.Length; i++)
                if (labels[i].Visible)
                    visibleList.Add(i);

            if (visibleList.Count == 0)
                return;

            // 初回選択
            if (selectedIndex < 0)
                selectedIndex = visibleList[0];

            int pos = visibleList.IndexOf(selectedIndex);

            switch (key)
            {
                case Keys.Left:
                    pos = Math.Max(0, pos - 1);
                    break;

                case Keys.Right:
                    pos = Math.Min(visibleList.Count - 1, pos + 1);
                    break;

                case Keys.Up:
                    // 8列固定なので左は -8
                    pos = Math.Max(0, pos - 8);
                    break;

                case Keys.Down:
                    pos = Math.Min(visibleList.Count - 1, pos + 8);
                    break;

                case Keys.Enter:
                    CommitSelection();
                    return;
            }

            selectedIndex = visibleList[pos];
            HighlightSelection();
            flowLayoutPanel1.ScrollControlIntoView(labels[selectedIndex]);
        }

        private void HighlightSelection()
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (!labels[i].Visible) continue;

                if (i == selectedIndex)
                {
                    labels[i].BackColor = Color.LightBlue;
                    labels[i].ForeColor = Color.Black;
                }
                else
                {
                    labels[i].BackColor = Color.Black;
                    labels[i].ForeColor = Color.White;
                }
            }
        }

        public void CommitSelection()
        {
            if (selectedIndex < 0) return;

            parent.SetProgramChange(selectedIndex);

            this.Visible = false;
            selectedIndex = 0;


        }


    }
}
