using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.Common
{
    public partial class SimpleCandidateForm : Form
    {
        public class CandidateItem
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        public enum CandidateFilterMode
        {
            Number,
            Text
        }
        public enum CandidateLayoutMode
        {
            VerticalList,
            MultiColumn
        }

        public event Action<CandidateItem> CandidateSelected;




        private int selectedIndex = 0;

        private List<Label> labels = new List<Label>();
        private int itemsPerColumn;
        private CandidateLayoutMode layoutMode;
        private int itemWidth;

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

        private List<CandidateItem> candidates = new List<CandidateItem>();
        private CandidateFilterMode filterMode;
        public CandidateItem SelectedItem
            => selectedIndex >= 0
            ? candidates[selectedIndex]
            : null;


        // デザイナ向けコンストラクタ
        public SimpleCandidateForm()
        {
            InitializeComponent();

            this.FormBorderStyle = FormBorderStyle.None;
            Hide();
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
                HighlightSelection();
        }


        public SimpleCandidateForm(
            List<CandidateItem> candidates,
            int currentIndex,
            CandidateFilterMode filterMode,
            int itemsPerColumn = 8,
            CandidateLayoutMode layoutMode = CandidateLayoutMode.MultiColumn,
            int itemWidth = 120)
        {
            this.candidates = candidates;
            this.filterMode = filterMode;
            this.itemsPerColumn = itemsPerColumn;
            this.layoutMode = layoutMode;
            this.itemWidth = itemWidth;

            InitializeComponent();

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
            //this.Width = 980;   // 960 + 余白
            //this.Height = 400;  // 352 + 余白
            this.AutoScroll = true;

            // FlowLayoutPanel の正しい設定
            if (layoutMode == CandidateLayoutMode.VerticalList)
            {
                flowLayoutPanel1.WrapContents = false;
                flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            }
            else
            {
                flowLayoutPanel1.WrapContents = true;
                flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            }
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Padding = new Padding(0);
            flowLayoutPanel1.Margin = new Padding(0);

            flowLayoutPanel1.SuspendLayout();
            // Labelを事前生成して敷き詰める
            for (int i = 0; i < candidates.Count; i++)
            {
                int index = i;

                var lbl = new Label();
                lbl.AutoSize = false;
                lbl.Size = new Size(itemWidth, 22);   // ★ これが「8列」を実現する
                lbl.TextAlign = ContentAlignment.MiddleLeft;
                lbl.Margin = new Padding(1);
                lbl.Padding = new Padding(4, 0, 0, 0);
                lbl.BackColor = Color.Black;
                lbl.ForeColor = Color.White;
                lbl.MouseEnter += (s, e) =>
                {
                    Debug.WriteLine($"MouseEnter {index}");
                    selectedIndex = index;
                    HighlightSelection();
                };

                lbl.Click += (s, e) =>
                {
                    selectedIndex = index;
                    CommitSelection();
                };

                lbl.Text = candidates[i].Text;

                labels.Add(lbl);
                flowLayoutPanel1.Controls.Add(lbl);
            }
            flowLayoutPanel1.ResumeLayout();

            if (layoutMode == CandidateLayoutMode.VerticalList)
            {
                Width = itemWidth + 4;
                Height = candidates.Count * 24 + 10;
            }
            else
            {
                int columnCount =
                Math.Max(
                    1,
                    (candidates.Count + itemsPerColumn - 1)
                    / itemsPerColumn);

                int rowCount =
                    Math.Min(
                        candidates.Count,
                        itemsPerColumn);

                Width =
                    columnCount * (itemWidth + 2);

                Height =
                    rowCount * 24 + 10;
            }

            selectedIndex = currentIndex;
            HighlightSelection();
        }

        public void FillCandidates(string inputValue)
        {
            Span<bool> visible = stackalloc bool[candidates.Count];

            switch (filterMode)
            {
                case CandidateFilterMode.Number:
                    FillNumberCandidates(inputValue, visible);
                    break;

                case CandidateFilterMode.Text:
                    FillTextCandidates(inputValue, visible);
                    break;
            }
        }

        private void FillNumberCandidates(string inputValue, Span<bool> visible)
        {

            // マッチング
            if (string.IsNullOrEmpty(inputValue))
            {
                for (int i = 0; i < candidates.Count; i++)
                    visible[i] = true;
            }
            else
            {
                if (int.TryParse(inputValue, out _))
                {
                    string v = inputValue.Trim();
                    if (v.Length > 3) v = v.Substring(v.Length - 3);

                    for (int i = 0; i < candidates.Count; i++)
                    {
                        bool match = true;

                        string numStr3 = candidates[i].Value.ToString("D3");
                        string numStr = numStr3.Substring(0, v.Length);

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

            if (int.TryParse(inputValue, out int n))
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (candidates[i].Value == n)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            ApplyVisible(visible);
        }

        private void FillTextCandidates(string inputValue, Span<bool> visible)
        {
            // マッチング
            if (string.IsNullOrEmpty(inputValue))
            {
                for (int i = 0; i < candidates.Count; i++)
                    visible[i] = true;
            }
            else
            {
                string v = inputValue.Trim().ToLower();
                for (int i = 0; i < candidates.Count; i++)
                {
                    string text = candidates[i].Text.ToLower();
                    visible[i] = text.Contains(v);
                }
            }

            ApplyVisible(visible);

        }

        private void ApplyVisible(Span<bool> visible)
        {
            flowLayoutPanel1.SuspendLayout();

            for (int i = 0; i < candidates.Count; i++)
            {
                labels[i].Visible = visible[i];
            }

            flowLayoutPanel1.ResumeLayout();

            int count = 0;
            for (int i = 0; i < candidates.Count; i++)
                if (visible[i]) count++;

            //Height = Math.Max(34, ((count + 7) / 8) * 24 + 10);
            if (layoutMode == CandidateLayoutMode.VerticalList)
            {
                Height = Math.Max(34, count * 24 + 10);
            }
            else
            {
                Height = Math.Max(34, ((count + itemsPerColumn - 1) / itemsPerColumn) * 24 + 10);
            }

            if (selectedIndex >= 0
                && selectedIndex < visible.Length
                && !visible[selectedIndex])
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    if (visible[i])
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }

            HighlightSelection();
        }

        public void MoveSelection(Keys key)
        {
            // visible なラベル一覧を作る
            List<int> visibleList = new List<int>();
            for (int i = 0; i < labels.Count; i++)
                if (labels[i].Visible)
                    visibleList.Add(i);

            if (visibleList.Count == 0)
                return;

            // 初回選択
            if (selectedIndex < 0)
                selectedIndex = visibleList[0];

            int pos = visibleList.IndexOf(selectedIndex);
            if (pos < 0) pos = 0;

            switch (key)
            {
                case Keys.Left:
                    pos = Math.Max(0, pos - 1);
                    break;

                case Keys.Right:
                    pos = Math.Min(visibleList.Count - 1, pos + 1);
                    break;

                case Keys.Up:
                    // 1列あたりの要素数分移動
                    pos = Math.Max(0, pos - itemsPerColumn);
                    break;

                case Keys.Down:
                    // 1列あたりの要素数分移動
                    pos = Math.Min(visibleList.Count - 1, pos + itemsPerColumn);
                    break;

                case Keys.Enter:
                case Keys.Space:
                    CommitSelection();
                    return;
            }

            selectedIndex = visibleList[pos];
            HighlightSelection();
            flowLayoutPanel1.ScrollControlIntoView(labels[selectedIndex]);
        }

        private void HighlightSelection()
        {
            for (int i = 0; i < labels.Count; i++)
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

            // ★ ここで 選択値を親に渡す
            CandidateSelected?.Invoke(candidates[selectedIndex]);

            Hide();

        }

    }
}
