using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Command;
using Remcoposer64.StepEditorPanelControl.EventArgs;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Windows.Forms;
using static Remcoposer64.StepEditorPanelControl.Keyboard;
using static Remcoposer64.StepEditorPanelControl.WinApi;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace Remcoposer64.StepEditorPanelControl
{
    public class StepEditorPanel : Panel
    {
        public UndoRedoManager.UndoRedoManager undoRedoManager = null;

        public Keyboard keyb;
        private ContextMenuStrip eventMenu;
        public ControlChangeCandidateForm CCcandidateForm;
        public ProgramChangeCandidateForm PCcandidateForm;

        // イベントの宣言
        public event EventHandler<ModeChangedEventArgs> InputModeChanged;
        public event EventHandler ExitStepEditor;
        public event EventHandler<StatusBarMessageEventArgs> ChangeStatusBarMessage;
        public Func<SysExEditRequest, SysExEditResult> SysExEditRequested;
        public event EventHandler<PlayNoteEventArgs> PlayNoteRequested;

        // 公開プロパティ
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool OverwriteMode { get; set; } = false;

        public bool imeMode = false;
        private bool isDragging = false;
        private int dragStartIndex = -1;
        private System.Windows.Forms.Timer autoScrollTimer;
        private int autoScrollDirection = 0; // -1 = 上, +1 = 下, 0 = 停止
        private float autoScrollSpeed = 1.0f; // 距離に応じた倍率
        private IntPtr oldWndProc;
        private WinApi.WndProcDelegate newWndProc;
        private bool IsInDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public int scrollOffset = 0;
        private VScrollBar vscroll;
        public int VisibleRows = 8; // 画面に見える行数
        public CursorInfo cursor = new CursorInfo();
        public int iconMergin;

        private int playingCursorIndex = -1; // 演奏位置のインデックス(初期値は-1(非表示)で、演奏中ではないことを示す)

        private IGraph grp;
        private EventListRenderer renderer;

        public int charWidth = 16;//1文字分の幅(ピクセル)
        //private int charHeight = 16;//1文字分の高さ(ピクセル)
        public int lineHeight = 16;//1行分の高さ(ピクセル)

        public readonly int[] columnWidths = {
            1,6, // MEAS:
            1,6, // STEP:
            0,4, // NOTE
            1,4, // K#
            1,5, // ST
            1,5, // GT
            1,4  // VEL
        };
        private readonly string[] headers = { "MEAS :", "STEP :", "NOTE", " K#", " ST", " GT", " VEL" };

        public EventManager eventManager = null;

        public enum EditorFontSize
        {
            Small8 = 8,
            Medium12 = 12,
            Large16 = 16
        }

        private EditorFontSize fontSize = EditorFontSize.Large16;

        [DefaultValueAttribute(EditorFontSize.Large16)]
        public EditorFontSize FontSize
        {
            get => fontSize;
            set
            {
                fontSize = value;

                if (!IsInDesignMode)
                    ApplyFontSize();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UpdateFlg { get; set; } = false;

        public MIDIEvent backupEvent;
        internal bool imeComposing = false;


        public void SetFontSize(EditorFontSize fontSize)
        {
            this.fontSize = fontSize;
            ApplyFontSize();
        }

        private void ApplyFontSize()
        {
            if (IsInDesignMode)
                return;

            int size = (int)fontSize;

            charWidth = size;
            lineHeight = size;

            // Graph のフォントサイズ変更
            grp.SetFontSize(size);

            // Renderer を再生成
            renderer = new EventListRenderer(this, grp, charWidth, lineHeight, columnWidths, headers);

            // VisibleRows を再計算
            VisibleRows = Math.Max(1, (this.ClientSize.Height - lineHeight) / lineHeight);

            // スクロールバー更新
            if (vscroll != null)
                vscroll.LargeChange = VisibleRows;

            // 再描画
            this.Invalidate();
        }


        public StepEditorPanel()
        {
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.Selectable
                , true);
            if (IsInDesignMode) return;

            TabStop = true;
            VisibleRows = (this.ClientSize.Height - lineHeight) / lineHeight; // 画面に見える行数を計算

            keyb = new Keyboard(this);
            eventManager = new EventManager(this);
            CCcandidateForm = new ControlChangeCandidateForm(this, UIControlChangeEvent.CCNames);
            CCcandidateForm.FillCandidates("");
            PCcandidateForm = new ProgramChangeCandidateForm(this, UIProgramChangeEvent.GMNames);
            PCcandidateForm.FillCandidates("");

            autoScrollTimer = new System.Windows.Forms.Timer();
            autoScrollTimer.Interval = 120; // 30msごとにスクロール
            autoScrollTimer.Tick += AutoScrollTimer_Tick;

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (grp != null)
                {
                    grp.Dispose();
                    grp = null;
                }
            }
            base.Dispose(disposing);
        }


        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsInDesignMode) return;

            iconMergin = (int)(charWidth * 1.2f);
            //grp = new GraphGDI(this.Handle);
            grp = new GraphVortice(this.Handle);
            renderer = new EventListRenderer(this, grp, charWidth, lineHeight, columnWidths, headers);

            vscroll = new VScrollBar();
            vscroll.Dock = DockStyle.Right;
            vscroll.ValueChanged += (s, e) =>
            {
                scrollOffset = vscroll.Value;
                this.Invalidate(); // 再描画
            };
            Controls.Add(vscroll);

            // Win32 WndProc に差し替え
            newWndProc = CustomWndProc;
            oldWndProc = WinApi.SetWindowLongPtr(this.Handle, WinApi.GWL_WNDPROC,
                Marshal.GetFunctionPointerForDelegate(newWndProc));

            //SetEventManager();

        }

        protected override void OnHandleDestroyed(System.EventArgs e)
        {
            if (IsInDesignMode) return;
            // Win32 WndProc を元に戻す
            WinApi.SetWindowLongPtr(this.Handle, WinApi.GWL_WNDPROC, oldWndProc);
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //DO_NOT//base.OnPaintBackground( e );
        }

        protected override void OnResize(System.EventArgs e)
        {
            base.OnResize(e);

            if (grp != null) grp.Resize(this.ClientSize.Width, this.ClientSize.Height);

            VisibleRows = (this.ClientSize.Height - lineHeight) / lineHeight; // 画面に見える行数を計算
            if (vscroll == null) return;

            if (VisibleRows < 1)
            {
                vscroll.Enabled = false;
                vscroll.LargeChange = 0;
            }
            else
            {
                vscroll.Enabled = true;
                vscroll.LargeChange = VisibleRows;
            }
            // ★スクロール位置が最大値を超えないように調整
            if (vscroll.Value > vscroll.Maximum - vscroll.LargeChange + 1)
            {
                vscroll.Value = Math.Max(0, vscroll.Maximum - vscroll.LargeChange + 1);
                scrollOffset = vscroll.Value;
            }
            this.Invalidate();
        }

        protected override void OnEnter(System.EventArgs e)
        {
            base.OnEnter(e);
            this.Focus(); // フォーカスを確実に取る
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (vscroll == null) return;
            int newValue = vscroll.Value - e.Delta / 120; // 1回のスクロールで1行分移動
            newValue = Math.Max(vscroll.Minimum, Math.Min(vscroll.Maximum - vscroll.LargeChange + 1, newValue));
            vscroll.Value = newValue;
            scrollOffset = vscroll.Value;
            this.Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            int row = e.Y / lineHeight;
            if (row < 1) return;
            row--;

            int absoluteIndex = scrollOffset + row;

            bool ctrl = (ModifierKeys & Keys.Control) != 0;
            bool shift = (ModifierKeys & Keys.Shift) != 0;

            if (isDragging)
            {
                isDragging = false;
                return;
            }

            if (keyb.inputState == InputState.EditEvent)
            {
                keyb.CommitActiveEvent(cursor.EventNode.Value, CommitKeyPattern.OtherKey);
            }

            // ★ Ctrl＋クリック → 個別選択トグル
            if (ctrl)
            {
                if (cursor.MultiSelectIndices.Contains(absoluteIndex))
                    cursor.MultiSelectIndices.Remove(absoluteIndex);
                else
                    cursor.MultiSelectIndices.Add(absoluteIndex);

                cursor.SelectionStart = -1;
                cursor.SelectionEnd = -1;

                cursor.Index = absoluteIndex;
                cursor.EventNode = eventManager.GetEventNodeAtIndex(absoluteIndex);

                this.Invalidate();
                return;
            }

            // ★ Shift＋クリック → 範囲選択
            if (shift && !isDragging)
            {
                if (cursor.SelectionStart < 0)
                    cursor.SelectionStart = cursor.Index;

                cursor.SelectionEnd = absoluteIndex;

                cursor.Index = absoluteIndex;
                cursor.EventNode = eventManager.GetEventNodeAtIndex(absoluteIndex);

                this.Invalidate();
                return;
            }

            // ★ 通常クリック → 単一選択
            cursor.ClearSelection();

            if (eventManager.EventCount <= absoluteIndex)
                absoluteIndex = eventManager.EventCount - 1;

            cursor.Index = absoluteIndex;

            int col = e.X / (int)fontSize;

            if (col < 6 + 6 + 5) cursor.Position = 0;
            else if (col < 6 + 6 + 5 + 3) cursor.Position = 1;
            else if (col < 6 + 6 + 5 + 3 + 5) cursor.Position = 2;
            else if (col < 6 + 6 + 5 + 3 + 5 + 5) cursor.Position = 3;
            else cursor.Position = 4;

            cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);

            isDragging = false;

            this.Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Right)
            {

                if (OverwriteMode)
                {
                    // 上書きモード中に右クリックした場合は、上書きモードを解除する。但し、編集中のイベントがある場合は解除しない
                    if (keyb.inputState == InputState.EditEvent)
                    {
                        WinApi.Beep();
                        return;
                    }
                    else
                    {
                        // 上書きモードを解除し挿入モードに切り替える
                        ToggleOverwriteMode();
                    }
                }

                if (eventMenu == null) InitializeContextMenu();
                eventMenu.Show(this, e.Location);
                return;
            }

            if (keyb.inputState == InputState.EditEvent)
            {
                keyb.CommitActiveEvent(cursor.EventNode.Value, CommitKeyPattern.OtherKey);
            }

            this.Focus();

            int row = e.Y / lineHeight - 1;
            if (row < 0) return;

            int absoluteIndex = scrollOffset + row;

            bool ctrl = (ModifierKeys & Keys.Control) != 0;
            bool shift = (ModifierKeys & Keys.Shift) != 0;

            // ★ Ctrlドラッグ → 個別選択の追加モード
            if (ctrl)
            {
                isDragging = true;
                dragStartIndex = absoluteIndex;

                // まず最初の行をトグル
                if (cursor.MultiSelectIndices.Contains(absoluteIndex))
                    cursor.MultiSelectIndices.Remove(absoluteIndex);
                else
                    cursor.MultiSelectIndices.Add(absoluteIndex);

                cursor.Index = absoluteIndex;
                cursor.EventNode = eventManager.GetEventNodeAtIndex(absoluteIndex);

                this.Invalidate();
                return;
            }

            // ★ 通常ドラッグ → 範囲選択モード
            isDragging = true;
            dragStartIndex = absoluteIndex;

            cursor.ClearSelection();
            cursor.SelectionStart = absoluteIndex;
            cursor.SelectionEnd = absoluteIndex;

            cursor.Index = absoluteIndex;
            cursor.EventNode = eventManager.GetEventNodeAtIndex(absoluteIndex);

            this.Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!isDragging) return;

            int row = e.Y / lineHeight - 1;
            if (row < 0) return;

            int absoluteIndex = scrollOffset + row;

            bool ctrl = (ModifierKeys & Keys.Control) != 0;

            // ★ Ctrlドラッグ → 個別選択の追加
            if (ctrl)
            {
                if (!cursor.MultiSelectIndices.Contains(absoluteIndex))
                    cursor.MultiSelectIndices.Add(absoluteIndex);

                cursor.Index = absoluteIndex;
                cursor.EventNode = eventManager.GetEventNodeAtIndex(absoluteIndex);

                this.Invalidate();
                return;
            }

            // ★ 通常ドラッグ / Shiftドラッグ → 範囲選択
            cursor.SelectionStart = Math.Min(dragStartIndex, absoluteIndex);
            cursor.SelectionEnd = Math.Max(dragStartIndex, absoluteIndex);

            cursor.Index = absoluteIndex;
            cursor.EventNode = eventManager.GetEventNodeAtIndex(absoluteIndex);

            // ★ オートスクロール判定（距離に応じて速度を変える）
            int margin = lineHeight * 2;

            int distanceTop = e.Y;                     // ← 生のマウス位置を使う
            int distanceBottom = this.Height - e.Y;    // ← 下端も同様

            if (distanceTop < margin)
            {
                autoScrollDirection = -1;
                autoScrollSpeed = 1.0f + (float)(margin - distanceTop) / margin; // 1.0〜2.0
                autoScrollTimer.Start();
            }
            else if (distanceBottom < margin)
            {
                autoScrollDirection = +1;
                autoScrollSpeed = 1.0f + (float)(margin - distanceBottom) / margin;
                autoScrollTimer.Start();
            }
            else
            {
                autoScrollDirection = 0;
                autoScrollSpeed = 0.0f;
                autoScrollTimer.Stop();
            }

            this.Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            autoScrollDirection = 0;
            autoScrollTimer.Stop();
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            base.OnMouseLeave(e);
            isDragging = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled)
            {
                return;
            }

            if (keyb == null)
            {
                return;
            }

            if (imeMode)
            {
                return; // Panel 側のキー処理を止める
            }

            // ★ 選択解除していいのは「通常の文字入力」だけ
            bool isEditingKey =
                e.KeyCode == Keys.Delete ||
                e.KeyCode == Keys.Back ||
                e.KeyCode == Keys.Left ||
                e.KeyCode == Keys.Right ||
                e.KeyCode == Keys.Up ||
                e.KeyCode == Keys.Down ||
                e.KeyCode == Keys.PageUp ||
                e.KeyCode == Keys.PageDown ||
                e.KeyCode == Keys.Home ||
                e.KeyCode == Keys.End;

            bool isModifier =
                WinApi.IsKeyDown(Keys.ControlKey) ||
                WinApi.IsKeyDown(Keys.ShiftKey);

            // ★ 通常キー入力のみ選択解除
            if (!isModifier && !isEditingKey)
            {
                cursor.ClearSelection();
            }

            keyb.HandleKeyDown((uint)e.KeyData);

            Debug.WriteLine(undoRedoManager.GetDebugInfo());
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.Handled)
            {
                return;
            }

            if (keyb == null)
            {
                return;
            }

            if (imeMode)
            {
                return; // Panel 側のキー処理を止める
            }

            if ((e.KeyChar == '\t' && WinApi.IsKeyDown(Keys.I))
                || (e.KeyChar == '\r' && WinApi.IsKeyDown(Keys.M))
                || (e.KeyChar == '\n' && WinApi.IsKeyDown(Keys.J))
                || (e.KeyChar == '\r' && WinApi.IsKeyDown(Keys.ShiftKey))
                || (e.KeyChar == '\n' && WinApi.IsKeyDown(Keys.ControlKey))
                || (e.KeyChar == '\r' && WinApi.IsKeyDown(Keys.LWin))
                || (e.KeyChar == '\r' && WinApi.IsKeyDown(Keys.RWin)))
            {
                return;
            }

            // ★ 通常入力があったら選択をクリアする
            if (!WinApi.IsKeyDown(Keys.ControlKey) && !WinApi.IsKeyDown(Keys.ShiftKey))
            {
                cursor.ClearSelection();
            }

            // otherwise, handle key-char event normally
            keyb.HandleKeyPress(e.KeyChar);
            e.Handled = true;

            Debug.WriteLine(undoRedoManager.GetDebugInfo());
        }

        /////////
        ///
        private void AutoScrollTimer_Tick(object sender, System.EventArgs e)
        {
            if (!isDragging) return;
            if (autoScrollDirection == 0) return;

            // speed に応じて「たまに」動かす感じにする
            // ここでは単純に speed 回に1回動かすより、
            // 行数に speed を掛けるほうが分かりやすい
            int delta = (int)Math.Round(autoScrollDirection * autoScrollSpeed);

            if (delta == 0)
                delta = autoScrollDirection; // 極端に小さいときは最低1行

            int newOffset = scrollOffset + delta;
            newOffset = Math.Max(0, Math.Min(eventManager.EventCount - VisibleRows, newOffset));

            if (newOffset != scrollOffset)
            {
                scrollOffset = newOffset;
                vscroll.Value = scrollOffset;
                Invalidate();
            }
        }

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (IsInDesignMode) return WinApi.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);

            //if (msg == WinApi.WM_IME_SETCONTEXT)
            //{
            //    //Imeを関連付ける
            //    IntPtr himc = ImmCreateContext();
            //    ImmAssociateContextEx(this.Handle, himc, ImmAssociateContextExFlags.IACE_DEFAULT);
            //}

            MIDIMemoEvent me = null;
            MdlMemoEvent mme = null;

            if (cursor?.EventNode?.Value is MIDIMemoEvent memoEvent)
            {
                me = memoEvent;
                mme = (MdlMemoEvent)eventManager.GetModel(me.Type);
            }

            if (msg == WinApi.WM_IME_STARTCOMPOSITION)
            {
                imeComposing = true;

                // ★ カーソル位置のイベントが MemoEvent か判定
                if (me != null && !me.editing && OverwriteMode)
                {
                    keyb.inputState = InputState.EditEvent;
                    mme.BeginEdit(me,cursor, keyb);
                }

                IntPtr hIMC = WinApi.ImmGetContext(hWnd);

                // ★ DirectX描画座標 → Win32クライアント座標に変換
                int caretDxX = 0;
                int caretDxY = 0;
                if (me != null)
                {
                    caretDxX = mme.memoBoxDrawX + (string.IsNullOrEmpty(mme.memoBoxText) ? 0 : ((GraphVortice)grp).MeasureStringOSFont(mme.memoBoxText.Substring(0, mme.memoBoxCaretIndex)));
                    caretDxY = mme.memoBoxDrawY;
                }

                // DirectX → Client → Screen
                Point caretClient = this.PointToClient(new Point(caretDxX, caretDxY));
                Point caretScreen = this.PointToScreen(caretClient);

                // ----------------------------
                // ① Composition Window（前編集）
                // ----------------------------
                COMPOSITIONFORM cf = new COMPOSITIONFORM();
                cf.dwStyle = 0x0002; // CFS_POINT
                cf.ptCurrentPos.x = caretScreen.X;
                cf.ptCurrentPos.y = caretScreen.Y;
                WinApi.ImmSetCompositionWindow(hIMC, ref cf);

                // ----------------------------
                // ② Candidate Window（候補）★重要★
                // ----------------------------
                CANDIDATEFORM cand = new CANDIDATEFORM();
                cand.dwIndex = 0;
                cand.dwStyle = 0x0040; // CFS_CANDIDATEPOS
                cand.ptCurrentPos.x = caretScreen.X;
                cand.ptCurrentPos.y = caretScreen.Y;
                WinApi.ImmSetCandidateWindow(hIMC, ref cand);

                WinApi.ImmReleaseContext(hWnd, hIMC);

                return IntPtr.Zero;
            }

            if (msg == WinApi.WM_IME_COMPOSITION)
            {
                IntPtr hIMC = WinApi.ImmGetContext(hWnd);

                // ★ composition string（変換中文字）
                int size = WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_COMPSTR, null, 0);
                if (size > 0)
                {
                    // ★ UTF-16 は 2バイト単位なので丸める
                    if ((size % 2) != 0)
                    {
                        size -= 1;
                    }

                    byte[] buffer = new byte[size];
                    WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_COMPSTR, buffer, size);

                    mme?.memoBoxComposition = Encoding.Unicode.GetString(buffer);
                }
                mme?.IMECursorPos = WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_CURSORPOS, null, 0);
                Debug.WriteLine("cursorPos:{0}", mme?.IMECursorPos);
                // ★ result string（確定文字列）
                int resultSize = WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_RESULTSTR, null, 0);
                if (resultSize > 0)
                {
                    byte[] buffer = new byte[resultSize];
                    WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_RESULTSTR, buffer, resultSize);

                    string result = Encoding.Unicode.GetString(buffer);


                    // MemoEvent に反映
                    if (me != null)
                    {
                        // composition caret の位置を使う
                        int insertPos = mme.memoBoxCaretIndex + mme.IMECursorPos;

                        // 文字を追加
                        mme.memoBoxText = mme.memoBoxText.Insert(insertPos, result);
                        mme.memoBoxCaretIndex = insertPos + result.Length;
                        me.Text = mme.memoBoxText;
                        mme.memoBoxComposition = "";
                    }
                }

                WinApi.ImmReleaseContext(hWnd, hIMC);

                Invalidate();
                return IntPtr.Zero;
            }

            if (msg == WinApi.WM_IME_ENDCOMPOSITION)
            {
                imeComposing = false;
                if (me != null)
                {
                    mme.memoBoxComposition = "";
                }
                Invalidate();
                return IntPtr.Zero;
            }

            if (msg == WinApi.WM_PAINT)
            {
                WinApi.PAINTSTRUCT ps;
                unsafe
                {
                    WinApi.BeginPaint(hWnd, &ps);

                    ((StepEditorPanel)this).RenderPanel();

                    WinApi.EndPaint(hWnd, &ps);
                }


                // ★標準描画を完全に抑止する
                return IntPtr.Zero;
            }

            // その他のメッセージは標準処理へ
            return WinApi.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }

        public void RenderPanel()
        {
            vscroll.Visible = (eventManager != null);

            Rectangle rect = this.ClientRectangle;
            renderer.Render(eventManager, rect, scrollOffset, cursor, playingCursorIndex);
        }


        public EventManager GetEventManager()
        {
            eventManager.Init();
            return eventManager;
        }

        public void Init(UndoRedoManager.UndoRedoManager undoRedoManager)
        {
            if (IsInDesignMode) return;

            this.undoRedoManager = undoRedoManager;

            cursor.EventNode = eventManager.FirstEvent;
            cursor.Index = 0;

            UpdateFlg = false;

            RefreshScrollBar();
        }

        public void RefreshView()
        {
            this.Invalidate();
        }

        public void StartImeMode()
        {
            imeMode = true;
        }

        public void EndImeMode()
        {
            imeMode = false;
            // TextBox を消す処理
        }

        protected override bool IsInputKey(Keys keyData)
        {
            return true; // 矢印キーなども Panel に届ける
        }

        internal void ScreenUpdate()
        {

            if (cursor.Index < 0) cursor.Index = 0;
            if (cursor.Index >= eventManager.EventCount) cursor.Index = eventManager.EventCount - 1;
            if (scrollOffset > cursor.Index) scrollOffset = cursor.Index;
            if (scrollOffset + VisibleRows <= cursor.Index) scrollOffset = cursor.Index - VisibleRows + 1;

            vscroll.Maximum = Math.Max(0, eventManager.EventCount);
            vscroll.Value = scrollOffset;

            this.Invalidate();
        }

        internal void ScreenUpdate(int scrollOffset)
        {
            this.scrollOffset = scrollOffset;
            vscroll.Maximum = Math.Max(0, eventManager.EventCount);
            vscroll.Value = scrollOffset;

            this.Invalidate();
        }

        internal void ToggleOverwriteMode()
        {
            OverwriteMode = !OverwriteMode;

            // イベント発火
            InputModeChanged?.Invoke(this, new ModeChangedEventArgs(OverwriteMode));
        }

        internal void InsertEventAtCoursor(MIDIEvent ev)
        {
            if (cursor.EventNode != null)
            {
                var cmd = new InsertEventCommand(eventManager, this, ev, cursor.Index, cursor.Index);
                undoRedoManager.Execute(cmd);

                cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);
            }
        }

        internal void DeleteEventAtCoursor(MIDIEvent ev)
        {
            if (ev == null) return;
            if (ev.Type == MIDIEventType.MetaEndOfTrack)
            {
                WinApi.Beep();
                return;
            }
            var cmd = new RemoveEventCommand(eventManager, this, cursor.Index, cursor.Index);
            undoRedoManager.Execute(cmd);

            cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);
        }

        internal void DeleteSelectedRows()
        {
            var indices = GetSelectedIndices();
            if (indices.Count == 0)
            {
                WinApi.Beep();
                return;
            }

            // 後ろから削除（インデックスずれ防止）
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                int idx = indices[i];

                var cmd = new RemoveEventCommand(eventManager, this, idx, idx);
                undoRedoManager.Execute(cmd);
            }

            // カーソル位置を自然な位置に戻す
            cursor.Index = Math.Min(indices[0], eventManager.EventCount - 1);
            cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);

            // 選択状態クリア
            cursor.ClearSelection();

            ScreenUpdate();
        }

        List<int> GetSelectedIndices()
        {
            var list = new List<int>();

            // 範囲選択
            if (cursor.SelectionStart >= 0 && cursor.SelectionEnd >= 0)
            {
                int start = Math.Min(cursor.SelectionStart, cursor.SelectionEnd);
                int end = Math.Max(cursor.SelectionStart, cursor.SelectionEnd);

                for (int i = start; i <= end; i++)
                {
                    var ev = eventManager.GetEventNodeAtIndex(i).Value;
                    if (ev.Type != MIDIEventType.MetaEndOfTrack)       // ★ EndEvent は除外
                        list.Add(i);
                }
            }

            // 個別選択
            foreach (int idx in cursor.MultiSelectIndices)
            {
                var ev = eventManager.GetEventNodeAtIndex(idx).Value;
                if (ev.Type != MIDIEventType.MetaEndOfTrack)       // ★ EndEvent は除外
                    if (!list.Contains(idx))
                        list.Add(idx);
            }

            list.Sort();
            return list;
        }

        internal void BackSpaceEventAtCoursor(MIDIEvent ev)
        {
            if (ev == null) return;
            if (cursor.EventNode.Previous == null)
            {
                WinApi.Beep();
                return;
            }
            var cmd = new RemoveEventCommand(eventManager, this, cursor.Index - 1, cursor.Index - 1);
            undoRedoManager.Execute(cmd);

        }

        internal void CancelEventAtCoursor(MIDIEvent ev)
        {
            LinkedListNode<MIDIEvent> en = cursor.EventNode;
            if (!en.Value.editing) return;
            var cmd = new RemoveEventCommand(eventManager, this, cursor.Index, cursor.Index);
            undoRedoManager.Execute(cmd);

        }

        internal void RestoreEventAtCoursor(MIDIEvent ev)
        {
            if (backupEvent == null) return;

            // 編集前の内容を復元
            ev.CopyFrom(backupEvent);

            backupEvent = null;

            RefreshView(); // 再描画
        }

        internal void FireExitEvent()
        {
            // イベント発火
            ExitStepEditor?.Invoke(this, System.EventArgs.Empty);
        }

        internal void Redo()
        {
            undoRedoManager?.Redo();
            // イベント発火
            ChangeStatusBarMessage?.Invoke(this, new StatusBarMessageEventArgs("Redo"));
        }

        internal void Undo()
        {
            undoRedoManager?.Undo();
            // イベント発火
            ChangeStatusBarMessage?.Invoke(this, new StatusBarMessageEventArgs("Undo"));
        }

        public void ForceEndEditing()
        {
            var ev = cursor.EventNode?.Value;
            if (ev == null) return;
            MdlEvent mdl = eventManager.GetModel(ev.Type);
            mdl.EndEdit(ev, keyb);
        }

        internal void CopyCurrentRow()
        {
            if (cursor?.EventNode?.Value == null)
            {
                WinApi.Beep();
                return;
            }

            MIDIEvent ev = cursor.EventNode.Value;

            // ★ Clone（Undo/Redo に影響しない安全な複製）
            MIDIEvent cloned = ev.Clone();
            MdlEvent mdl = eventManager.GetModel(cloned.Type);

            // ★ クリップボード用の文字列（Event が自分で生成する）
            string text = mdl.ToClipboardString(cloned) + Environment.NewLine;

            // ★ クリップボードへ
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                WinApi.Beep();
            }
        }

        internal void CopySelectedRows()
        {
            var indices = GetSelectedIndices();
            if (indices.Count == 0)
            {
                WinApi.Beep();
                return;
            }

            List<string> lines = new List<string>();

            foreach (int idx in indices)
            {
                MIDIEvent ev = eventManager.GetEventNodeAtIndex(idx).Value;
                MIDIEvent cloned = ev.Clone();
                MdlEvent mdl = eventManager.GetModel(cloned.Type);
                lines.Add(mdl.ToClipboardString(cloned));
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, lines) + Environment.NewLine);
            }
            catch
            {
                WinApi.Beep();
            }
        }

        internal void CutCurrentRow()
        {
            if (cursor?.EventNode?.Value == null)
            {
                WinApi.Beep();
                return;
            }

            MIDIEvent ev = cursor.EventNode.Value;

            // EndEvent は削除禁止
            if (ev.Type == MIDIEventType.MetaEndOfTrack)
            {
                WinApi.Beep();
                return;
            }

            // ★ 今のカーソル位置・スクロール位置を保存
            int oldIndex = cursor.Index;
            int oldScrollOffset = scrollOffset;

            // ★ まずコピー
            MIDIEvent cloned = ev.Clone();
            MdlEvent mdl = eventManager.GetModel(cloned.Type);
            string text = mdl.ToClipboardString(cloned) + Environment.NewLine;

            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                WinApi.Beep();
                return;
            }

            // ★ 次に削除（Undo/Redo 対応）
            var cmd = new RemoveEventCommand(eventManager, this, cursor.Index, cursor.Index);
            undoRedoManager.Execute(cmd);

            // ★ カーソルの「行」をキープする
            cursor.Index = Math.Min(oldIndex, eventManager.EventCount - 1);
            cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);

            // ★ スクロール位置もキープしたいならこちら
            ScreenUpdate(oldScrollOffset);
        }

        internal void CutSelectedRows()
        {
            var indices = GetSelectedIndices();
            if (indices.Count == 0)
            {
                WinApi.Beep();
                return;
            }

            // ★ まずコピー（複数行をまとめてクリップボードへ）
            List<string> lines = new List<string>();
            foreach (int idx in indices)
            {
                MIDIEvent ev = eventManager.GetEventNodeAtIndex(idx).Value;
                MdlEvent mev = eventManager.GetModel(ev.Type);
                lines.Add(mev.ToClipboardString(ev.Clone()));
            }

            try
            {
                Clipboard.SetText(string.Join(Environment.NewLine, lines) + Environment.NewLine);
            }
            catch
            {
                WinApi.Beep();
                return;
            }

            // ★ 次に削除（後ろから削除）
            for (int i = indices.Count - 1; i >= 0; i--)
            {
                int idx = indices[i];
                var cmd = new RemoveEventCommand(eventManager, this, idx, idx);
                undoRedoManager.Execute(cmd);
            }

            // ★ カーソル位置を自然な位置に戻す
            cursor.Index = Math.Min(indices[0], eventManager.EventCount - 1);
            cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);

            // ★ 選択状態クリア
            cursor.ClearSelection();

            ScreenUpdate();
        }

        internal void PasteToCurrentRow()
        {
            string text = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(text))
            {
                WinApi.Beep();
                return;
            }

            // ★ 行単位で分割
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            int insertIndex = cursor.Index;

            foreach (var line in lines)
            {
                MIDIEvent ev = ParseClipboardLine(line);
                if (ev == null)
                {
                    WinApi.Beep();
                    continue;
                }

                var cmd = new InsertEventCommand(eventManager, this, ev, insertIndex, insertIndex);
                undoRedoManager.Execute(cmd);

                insertIndex++;
            }

            // ★ ペースト後カーソルを「貼り付けたブロックの直後」に置く
            cursor.Index = insertIndex;
            if (cursor.Index >= eventManager.EventCount)
                cursor.Index = eventManager.EventCount - 1;
            cursor.EventNode = eventManager.GetEventNodeAtIndex(cursor.Index);

            ScreenUpdate();
        }

        private MIDIEvent ParseClipboardLine(string line)
        {
            // "2,1: NoteEvent: C4,60,0,460,70"
            var parts = line.Split(':');
            if (parts.Length < 3) return null;

            var measStep = parts[0].Split(',');
            if (measStep.Length < 2) return null;

            if (!int.TryParse(measStep[0], out int meas)) return null;
            if (!int.TryParse(measStep[1], out int step)) return null;

            meas--;
            step--;

            string type = parts[1].Trim();
            string body = parts[2].Trim();

            return MdlEvent.FromClipboard(type, eventManager, meas, step, body);
        }

        private MIDIEventType lastEventType = MIDIEventType.None;
        private ToolStripMenuItem lastEventMenuItem;

        private const string csLastEvent = "Last Event: ";
        private const string csMIDIChannelEvents = "MIDI Channel Events ($An〜$En)";
        private const string csAn = "$An Add Polyphonic Key Pressure(After K.)";
        private const string csBn = "$Bn Add Control Change";
        private const string csCn = "$Cn Add Program Change";
        private const string csDn = "$Dn Add Channel Pressure(After C.)";
        private const string csEn = "$En Add Pitch Bend";
        private const string csF0 = "$F0 Add SysEx";
        private const string csFF = "$FF Add Meta";
        private const string csXXBarLine = "$xx Add BarLine.";
        private const string csXXSameMeas = "$xx Add Same Meas.";
        private const string csXXMemo = "$xx Add memo.";
        private const string csXXTempo = "$xx Add Tempo.";


        private void InitializeContextMenu()
        {
            eventMenu = new ContextMenuStrip();

            lastEventMenuItem = new ToolStripMenuItem($"0: {csLastEvent}(none)");
            lastEventMenuItem.Click += (s, e) =>
            {
                if (lastEventType != MIDIEventType.None)
                    InsertEvent(lastEventType);
            };
            eventMenu.Items.Add(lastEventMenuItem);

            // ★ 演奏系サブメニュー
            var midiChannelMenu = new ToolStripMenuItem($"1: {csMIDIChannelEvents}");
            midiChannelMenu.DropDownItems.Add($"0: {csAn}.", null, (s, e) => InsertEvent(MIDIEventType.KeyAfterTouch));
            midiChannelMenu.DropDownItems.Add($"1: {csBn}.", null, (s, e) => InsertEvent(MIDIEventType.ControlChange));
            midiChannelMenu.DropDownItems.Add($"2: {csCn}.", null, (s, e) => InsertEvent(MIDIEventType.ProgramChange));
            midiChannelMenu.DropDownItems.Add($"3: {csDn}.", null, (s, e) => InsertEvent(MIDIEventType.ChannelAfterTouch));
            midiChannelMenu.DropDownItems.Add($"4: {csEn}.", null, (s, e) => InsertEvent(MIDIEventType.PitchBend));
            eventMenu.Items.Add(midiChannelMenu);

            eventMenu.Items.Add($"2: {csXXBarLine}.", null, (s, e) => InsertEvent(MIDIEventType.BarLine));
            eventMenu.Items.Add($"3: {csXXSameMeas}.", null, (s, e) => InsertEvent(MIDIEventType.SameMeas));
            eventMenu.Items.Add($"4: {csXXMemo}.", null, (s, e) => InsertEvent(MIDIEventType.Memo));
            eventMenu.Items.Add($"5: {csXXTempo}.", null, (s, e) => InsertEvent(MIDIEventType.MetaTempo));

            eventMenu.Items.Add($"6: {csF0}.", null, (s, e) => InsertEvent(MIDIEventType.SysExF0));
            eventMenu.Items.Add($"7: {csFF}.", null, (s, e) => InsertEvent(MIDIEventType.Meta));

        }

        public void InsertEvent(MIDIEventType type)
        {
            var ev = eventManager.CreateEvent(type);
            MdlEvent mev = eventManager.GetModel(ev.Type);

            InsertEventAtCoursor(ev);

            keyb.inputState = InputState.EditEvent;
            mev.BeginEdit(ev, cursor, keyb);

            lastEventType = type;
            UpdateLastEventMenuItem();
        }

        private void UpdateLastEventMenuItem()
        {
            string label = $"0: {csLastEvent} {EventTypeToLabel(lastEventType)}";
            lastEventMenuItem.Text = label;
        }

        private string EventTypeToLabel(MIDIEventType type)
        {
            return type switch
            {
                MIDIEventType.KeyAfterTouch => csAn.Replace("ADD ", ""),
                MIDIEventType.ControlChange => csBn.Replace("ADD ", ""),
                MIDIEventType.ProgramChange => csCn.Replace("ADD ", ""),
                MIDIEventType.ChannelAfterTouch => csDn.Replace("ADD ", ""),
                MIDIEventType.PitchBend => csEn.Replace("ADD ", ""),
                MIDIEventType.SysExF0 => csF0.Replace("ADD ", ""),
                MIDIEventType.Meta => csFF.Replace("ADD ", ""),
                MIDIEventType.MetaTempo => csXXTempo.Replace("ADD ", ""),
                MIDIEventType.BarLine => csXXBarLine.Replace("ADD ", ""),
                MIDIEventType.SameMeas => csXXSameMeas.Replace("ADD ", ""),
                MIDIEventType.Memo => csXXMemo.Replace("ADD ", ""),
                _ => "(none)"
            };
        }

        public void ShowEventContextMenu()
        {
            // カーソル行の描画位置にメニューを出す
            int rowY = (cursor.Index + 1) * lineHeight;
            // カーソル列の X 座標を計算
            int colX = iconMergin + (6 + 6) * charWidth;// 6 + 6 は「小節番号 + ステップ番号」の幅
            // メニューを少し右にずらす
            int menuX = colX + charWidth;

            if (eventMenu == null) InitializeContextMenu();
            eventMenu.Show(this, new Point(menuX, rowY));
        }

        public void RequestPlayNote(int keyNumber)
        {
            PlayNoteRequested?.Invoke(this, new PlayNoteEventArgs(keyNumber));
        }

        internal Point PointToScreenFromCursorInfo(CursorInfo ci)
        {
            if (ci == null) return Point.Empty;

            int len = 0;
            for (int i = 0; i < ci.Position + 2; i++)//2はMEASとSTEPの文字数も足すため
            {
                len += columnWidths[i * 2 + 1];
            }

            Point p = this.PointToScreen(new Point(
                (len + 3) * charWidth //3 少し余裕を持たせるため
                , (ci.Index - scrollOffset + 2) * lineHeight));//2 ヘッダー行と、一行下に描画したいため

            return p;
        }

        internal void SetControlChange(int ccNumber)
        {
            if (keyb.inputState == InputState.None) return;

            var ev = cursor.EventNode.Value as MIDIControlChangeEvent;
            if (ev == null) return;

            MdlControlChangeEvent mev = (MdlControlChangeEvent)eventManager.GetModel(ev.Type);
            ev.editVal = ccNumber.ToString();
            CursorInfo newCursorInfo = new CursorInfo();
            if (OverwriteMode)
                mev.MoveNext_Number2Value(ev, cursor, newCursorInfo, ccNumber, false, MoveNextCursorAction.NextEvent);
            else
                mev.MoveNext_Number2Value(ev, cursor, newCursorInfo, ccNumber, true, MoveNextCursorAction.NextField);

            ScreenUpdate();
        }

        internal void SetProgramChange(int programNumber)
        {
            if (keyb.inputState == InputState.None) return;

            var ev = cursor.EventNode.Value as MIDIProgramChangeEvent;
            if (ev == null) return;

            MdlProgramChangeEvent mev = (MdlProgramChangeEvent)eventManager.GetModel(ev.Type);
            // 数値を直接確定
            ev.editVal = programNumber.ToString();

            CursorInfo newCursorInfo = new CursorInfo();

            if (OverwriteMode)
                mev.MoveNext_ProgramNumber(ev, cursor, newCursorInfo, programNumber, false, MoveNextCursorAction.NextEvent);
            else
                mev.MoveNext_ProgramNumber(ev, cursor, newCursorInfo, programNumber, false, MoveNextCursorAction.NextEvent);

            ScreenUpdate();
        }

        public void SetPartData(MIDIPart prt)
        {
            eventManager.SetPartData(prt);
        }

        public void RefreshScrollBar()
        {
            if (vscroll == null) return;
            
            vscroll.Minimum = 0;
            vscroll.Maximum = Math.Max(0, eventManager.EventCount);
            vscroll.LargeChange = VisibleRows;
            vscroll.SmallChange = 1;
        }
    }
}
