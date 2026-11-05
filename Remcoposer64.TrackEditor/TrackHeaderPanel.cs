using Remcoposer64.Common;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Remcoposer64.Common.WinApi;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Remcoposer64.TrackEditor.Internal
{
    [ToolboxItem(false)]
    public class TrackHeaderPanel : TrackPanelBase
    {

        //public int pxTrackHeight => TrackEditorControl.HeaderLine * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);

        public Color backColor = Color.FromArgb(0, 20, 20, 60);

        public TrackHeaderPanel()
        {
            SetStyle(ControlStyles.Selectable, true);
            TabStop = true;
        }

        public override void SetContext(
            TrackEditorControl trackEditorControl,
            MIDIProject project,
            Setting setting,
            RmPlayer player) => base.SetContext(trackEditorControl, project, setting, player);

        private const int COL_TRACKNO = 0;
        private const int COL_MUTE = 3;
        private const int COL_SOLO = 4;
        private const int COL_ST = 6;
        private const int COL_KEY = 11;
        private const int COL_TRACKNAME = 16;
        private const int COL_MIDIOUTCH = 6;
        private const int COL_MIDIOUT = 10;
        private const int COL_MIDIINCH = 6;
        private const int COL_MIDIIN = 10;
        

        private IntPtr oldWndProc;
        private WinApi.WndProcDelegate newWndProc;
        private bool IsInDesignMode => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;
        internal bool imeComposing = false;
        private string imeComposition = "";
        private Point imePos = Point.Empty;

        private int LineHeight => pxFontHeight + pxTrackMarginTop;

        protected override void Render()
        {
            bool hasSolo = Project.Track.Any(t => t.Solo);

            LinkedListNode<MIDITrack> trk = Project.Track.First;
            for (int i = 0; i < TrackEditorControl.FirstVisibleTrack; i++)
            {
                if (trk == null) return;
                trk = trk.Next;
            }

            Setting.midiOutInfo[] midiOutDevices = null;
            if (Setting?.midiOut?.lstMidiOutInfo != null
                && Setting.midiOut.lstMidiOutInfo.Count > Setting.midiOut.CurrentDev)
            {
                midiOutDevices =
                    Setting.midiOut.lstMidiOutInfo[
                        Setting.midiOut.CurrentDev];
            }

            Setting.midiInInfo[] midiInDevices = null;
            if (Setting?.midiIn?.lstMidiInInfo != null
                && Setting.midiIn.lstMidiInInfo.Count > Setting.midiIn.CurrentDev)
            {
                midiInDevices =
                    Setting.midiIn.lstMidiInInfo[
                        Setting.midiIn.CurrentDev];
            }

            Point pos = Point.Empty;

            graph.BackColor = backColor;
            graph.FillRectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);
            Color mojiColor = Color.FromArgb(0, 150, 140, 250);

            for (int i = TrackEditorControl.FirstVisibleTrack; i < TrackEditorControl.LastVisibleTrack; i++)
            {
                if (trk == null)
                    break;

                int trackPanelPosY = i * pxTrackHeight - TrackEditorControl.pxScrollY;
                string text;
                string name;

                // Back 
                pos.X = 0;
                pos.Y = 0 + trackPanelPosY;
                graph.BackColor = backColor;
                graph.FillRectangle(
                    pos.X,
                    pos.Y,
                    this.ClientSize.Width,
                    pxTrackHeight
                    );


                // Track Number
                pos.X = ColX(COL_TRACKNO);
                pos.Y = LineY(trackPanelPosY, 0);
                graph.DrawString(
                    $"{i + 1:D2} ",
                    ref pos,
                    mojiColor,
                    backColor);

                // Mute Button
                bool muteLamp = hasSolo ? !trk.Value.Solo : trk.Value.Mute;
                Color col = muteLamp ? Color.LightPink : Color.DarkMagenta;
                pos.X = ColX(COL_MUTE);
                pos.Y = LineY(trackPanelPosY, 0);
                graph.DrawString(
                    "M",
                    ref pos,
                    col,
                    backColor);

                // Solo Button
                col = trk.Value.Solo ? Color.LightBlue : Color.Blue;
                pos.X = ColX(COL_SOLO);
                pos.Y = LineY(trackPanelPosY, 0);
                graph.DrawString(
                    "S",
                    ref pos,
                    col,
                    backColor);

                // ST+
                pos.X = ColX(COL_ST);
                pos.Y = LineY(trackPanelPosY, 0);
                if (TrackEditorControl.TrackCursor.Track == trk
                    && TrackEditorControl.TrackCursor.HeaderColumn == TrackHeaderColumn.ST
                    && TrackEditorControl.editingST)
                {
                    text = TrackEditorControl.stInput + "_";
                }
                else
                {
                    text = $"{trk.Value.St:+000;-000; 000}";
                }
                graph.DrawString(
                    text,
                    ref pos,
                    mojiColor,
                    backColor);

                // KEY+
                pos.X = ColX(COL_KEY);
                pos.Y = LineY(trackPanelPosY, 0);
                if (TrackEditorControl.TrackCursor.Track == trk
                    && TrackEditorControl.TrackCursor.HeaderColumn == TrackHeaderColumn.KEY
                    && TrackEditorControl.editingKEY)
                {
                    text = TrackEditorControl.keyInput + "_";
                }
                else
                {
                    text = trk.Value.Key == null
                        ? "Off"
                        : $"{trk.Value.Key:+000;-000; 000}";
                }
                graph.DrawString(
                    text,
                    ref pos,
                    mojiColor,
                    backColor);

                // Track Name
                pos.X = ColX(COL_TRACKNAME);
                pos.Y = LineY(trackPanelPosY, 0);
                if (TrackEditorControl.editingTrackName && TrackEditorControl.TrackCursor.Track == trk)
                {
                    name = TrackEditorControl.trackNameInput;
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        mojiColor,
                        Color.Black
                        );
                    name = imeComposition + "_";
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        Color.Yellow,
                        Color.Black
                        );
                    imePos.X = pos.X;
                    imePos.Y = pos.Y;
                }
                else
                {
                    name = trk.Value.Name;
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        mojiColor,
                        Color.Black
                        );
                }


                if (TrackEditorControl.HeaderLine > 1)
                {
                    // MIDI Out Ch
                    pos.X = ColX(COL_MIDIOUTCH);
                    pos.Y = LineY(trackPanelPosY, 1);
                    string num = trk.Value.OutChannel == null ? "Off" : $"{trk.Value.OutChannel + 1:D2}";
                    
                    if (TrackEditorControl.TrackCursor.Track == trk
                        && TrackEditorControl.TrackCursor.HeaderColumn == TrackHeaderColumn.OutChannel
                        && TrackEditorControl.editingOutChannel)
                    {
                        num = TrackEditorControl.outChannelInput + "_";
                    }

                    graph.DrawString(
                        num,
                        ref pos,
                        mojiColor,
                        backColor);

                    // MIDI Out
                    name = "";
                    if (midiOutDevices != null && midiOutDevices.Length > 0)
                    {
                        int dev = Math.Clamp(trk.Value.OutDevice, 0, midiOutDevices.Length - 1);
                        name = string.IsNullOrWhiteSpace(midiOutDevices[dev]?.alias)
                            ? midiOutDevices[dev]?.name ?? ""
                            : midiOutDevices[dev].alias;
                    }
                    pos.X = ColX(COL_MIDIOUT);
                    pos.Y = LineY(trackPanelPosY, 1);
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        mojiColor,
                        Color.Black
                        );

                }

                if (TrackEditorControl.HeaderLine > 2)
                {
                    // MIDI In Ch
                    pos.X = ColX(COL_MIDIINCH);
                    pos.Y = LineY(trackPanelPosY, 2);
                    string num = trk.Value.InChannel == null ? "Off" : $"{trk.Value.InChannel + 1:D2}";

                    if (TrackEditorControl.TrackCursor.Track == trk
                        && TrackEditorControl.TrackCursor.HeaderColumn == TrackHeaderColumn.InChannel
                        && TrackEditorControl.editingInChannel)
                    {
                        num = TrackEditorControl.inChannelInput + "_";
                    }

                    graph.DrawString(
                        num,
                        ref pos,
                        mojiColor,
                        backColor);

                    // MIDI In
                    name = "";
                    if (midiInDevices != null
                        && midiInDevices.Length > 0)
                    {
                        int dev = Math.Clamp(trk.Value.InDevice, 0, midiInDevices.Length - 1);
                        name = string.IsNullOrWhiteSpace(midiInDevices[dev]?.alias)
                            ? midiInDevices[dev]?.name ?? ""
                            : midiInDevices[dev].alias;
                    }
                    pos.X = ColX(COL_MIDIIN);
                    pos.Y = LineY(trackPanelPosY, 2);
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        mojiColor,
                        Color.Black
                        );
                }

                // ベゼル(区切り線)
                graph.BackColor = Color.FromArgb(0, 90, 80, 190);
                pos.X = 0;
                pos.Y = 0 + trackPanelPosY + pxTrackHeight - pxTrackBorderLineThin;
                graph.FillRectangle(pos.X, pos.Y, this.ClientSize.Width, pxTrackBorderLineThin);

                // カーソル
                if (ReferenceEquals(TrackEditorControl.TrackCursor.Track, trk))
                {
                    DrawFocusRect(0, trackPanelPosY, ClientSize.Width, pxTrackHeight, Color.FromArgb(10, 200, 190, 255));

                    Rectangle rc = GetColumnRect(trackPanelPosY, TrackEditorControl.TrackCursor.HeaderColumn);
                    DrawFocusRect(
                        rc.X,
                        rc.Y,
                        rc.Width,
                        rc.Height,
                        Color.White);
                }

                trk = trk.Next;
            }

        }

        private void DrawFocusRect(
            int x,
            int y,
            int w,
            int h,
            Color color)
        {
            graph.BackColor = color;

            graph.FillRectangle(x, y, w, 1);
            graph.FillRectangle(x, y + h - 1, w, 1);

            graph.FillRectangle(x, y, 1, h);
            graph.FillRectangle(x + w - 1, y, 1, h);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Focus();

            base.OnMouseDown(e);

            int trackNo = (e.Y + TrackEditorControl.pxScrollY) / pxTrackHeight;
            
            TrackEditorControl.HideCandidateForm();

            LinkedListNode<MIDITrack> trk = TrackEditorControl.GetTrackByIndex(trackNo);

            if (trk == null) return;

            TrackHeaderColumn col = HitTestColumn(e.X, e.Y);

            TrackEditorControl.MoveCursorTrack(trk, col);

            if (IsPopupColumn(col)
                || col == TrackHeaderColumn.Mute
                || col == TrackHeaderColumn.Solo)
            {
                TrackEditorControl.ExecuteCurrentColumnAction();
            }
        }

        private bool IsPopupColumn(TrackHeaderColumn col)
        {
            return col == TrackHeaderColumn.OutDevice
                || col == TrackHeaderColumn.InDevice;
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            TrackEditorControl.HideCandidateForm();

            int trackNo =
                (e.Y + TrackEditorControl.pxScrollY)
                / pxTrackHeight;

            if (trackNo < 0
                || trackNo >= Project.Track.Count)
                return;

            LinkedListNode<MIDITrack> trk =
                Project.Track.First;

            for (int i = 0; i < trackNo; i++)
            {
                if (trk == null)
                    return;

                trk = trk.Next;
            }

            TrackEditorControl.RaiseTrackDoubleClicked(trk);
        }

        protected override void OnHandleCreated(System.EventArgs e)
        {
            base.OnHandleCreated(e);
            if (IsInDesignMode) return;

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

        private IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (IsInDesignMode) return WinApi.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);

            if (msg == WinApi.WM_IME_STARTCOMPOSITION)
            {
                Debug.WriteLine("WM_IME_STARTCOMPOSITION");

                imeComposing = true;

                // ★ カーソル位置のイベントが MemoEvent か判定
                //BeginTextEdit(...);

                IntPtr hIMC = WinApi.ImmGetContext(hWnd);

                // ★ DirectX描画座標 → Win32クライアント座標に変換
                int caretDxX = imePos.X;
                int caretDxY = imePos.Y;

                // DirectX → Client → Screen
                Point caretClient = this.PointToClient(new Point(caretDxX, caretDxY));
                Point caretScreen = this.PointToScreen(caretClient);

                // ----------------------------
                // ① Composition Window（前編集）
                // ----------------------------
                COMPOSITIONFORM cf = new()
                {
                    dwStyle = 0x0002 // CFS_POINT
                };
                cf.ptCurrentPos.x = caretScreen.X;
                cf.ptCurrentPos.y = caretScreen.Y;
                WinApi.ImmSetCompositionWindow(hIMC, ref cf);

                // ----------------------------
                // ② Candidate Window（候補）★重要★
                // ----------------------------
                CANDIDATEFORM cand = new()
                {
                    dwIndex = 0,
                    dwStyle = 0x0040 // CFS_CANDIDATEPOS
                };
                cand.ptCurrentPos.x = caretScreen.X;
                cand.ptCurrentPos.y = caretScreen.Y;
                WinApi.ImmSetCandidateWindow(hIMC, ref cand);

                WinApi.ImmReleaseContext(hWnd, hIMC);

                return IntPtr.Zero;
            }

            else if (msg == WinApi.WM_IME_COMPOSITION)
            {

                Debug.WriteLine("WM_IME_COMPOSITION");

                IntPtr hIMC = WinApi.ImmGetContext(hWnd);

                // ★ composition string（変換中文字）
                imeComposition = "";
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

                    //Debug.WriteLine(Encoding.Unicode.GetString(buffer));
                    imeComposition = Encoding.Unicode.GetString(buffer);
                }
                int IMECursorPos = WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_CURSORPOS, null, 0);
                Debug.WriteLine("cursorPos:{0}", IMECursorPos);

                // ★ result string（確定文字列）
                int resultSize = WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_RESULTSTR, null, 0);
                if (resultSize > 0)
                {
                    byte[] buffer = new byte[resultSize];
                    WinApi.ImmGetCompositionStringW(hIMC, WinApi.GCS_RESULTSTR, buffer, resultSize);

                    string result = Encoding.Unicode.GetString(buffer);

                    Debug.WriteLine($"Result:{result}");
                    if (TrackEditorControl.editingTrackName)
                    {
                        TrackEditorControl.trackNameInput += result;
                        imeComposition = "";
                    }
                }

                WinApi.ImmReleaseContext(hWnd, hIMC);

                Invalidate();
                return IntPtr.Zero;
            }

            else if (msg == WinApi.WM_IME_ENDCOMPOSITION)
            {
                Debug.WriteLine("WM_IME_ENDCOMPOSITION");
                imeComposing = false;
                //if (me != null)
                //{
                //    mme.memoBoxComposition = "";
                //}
                Invalidate();
                return IntPtr.Zero;
            }
            else if (msg == WinApi.WM_CHAR)
            {
                Debug.WriteLine(
                    $"WM_CHAR {(char)wParam} {(int)wParam}");

                if (!imeComposing)
                {

                    if (TrackEditorControl.editingTrackName)
                    {
                        EditTrackName(wParam);
                    }
                }
            }
            //else Debug.WriteLine($"{msg}");

            // その他のメッセージは標準処理へ
            return WinApi.CallWindowProc(oldWndProc, hWnd, msg, wParam, lParam);
        }

        private void EditTrackName(nint wParam)
        {
            char ch = (char)wParam;
            if (ch == '\b')
            {
                if (TrackEditorControl.trackNameInput.Length > 0)
                {
                    TrackEditorControl.trackNameInput =
                        TrackEditorControl.trackNameInput[..^1];
                }
            }
            else if (!char.IsControl(ch))
            {
                TrackEditorControl.trackNameInput += ch;
            }

            this.Refresh();
        }

        private int ColX(int col)
        {
            return pxTrackMarginLeft
                + pxFixedFontWidth * col
                - TrackEditorControl.pxHeaderScrollX;
        }
        
        private int ColAbs(int col)
        {
            return pxTrackMarginLeft
                 + pxFixedFontWidth * col;
        }

        public int GetMaxWidth()
        {
            int maxCol = COL_TRACKNAME;

            if (TrackEditorControl.HeaderLine > 1)
                maxCol = Math.Max(maxCol, COL_MIDIOUT);

            if (TrackEditorControl.HeaderLine > 2)
                maxCol = Math.Max(maxCol, COL_MIDIIN);

            return pxTrackMarginLeft
                 + pxFixedFontWidth * (maxCol + 40);
        }

        private int HitTestLine(int y)
        {
            int localY =
                (y + TrackEditorControl.pxScrollY)
                % pxTrackHeight;

            for (int line = 0; line < TrackEditorControl.HeaderLine; line++)
            {
                int y1 = LineY(0, line);
                int y2 = y1 + LineHeight;

                if (localY >= y1 &&
                    localY < y2)
                {
                    return line;
                }
            }

            return 0;
        }

        private TrackHeaderColumn HitTestColumn(int x, int y)
        {
            x += TrackEditorControl.pxHeaderScrollX;
            int line = HitTestLine(y);
            //int line =
            //    (y + TrackEditorControl.pxScrollY)
            //    % pxTrackHeight;

            //line /= pxFontHeight + pxTrackMarginTop;

            if (line == 0)
            {
                if (x < ColAbs(COL_MUTE))
                    //return TrackHeaderColumn.TrackNo;
                    return TrackHeaderColumn.Mute;

                if (x < ColAbs(COL_SOLO))
                    return TrackHeaderColumn.Mute;

                if (x < ColAbs(COL_ST))
                    return TrackHeaderColumn.Solo;

                if (x < ColAbs(COL_KEY))
                    return TrackHeaderColumn.ST;

                if (x < ColAbs(COL_TRACKNAME))
                    return TrackHeaderColumn.KEY;

                return TrackHeaderColumn.Name;
            }
            
            if (line == 1)
            {
                if (TrackEditorControl.HeaderLine < 2)
                    return TrackHeaderColumn.None;

                if (x < ColAbs(COL_MIDIOUT))
                    return TrackHeaderColumn.OutChannel;

                return TrackHeaderColumn.OutDevice;
            }
        
            if (line == 2)
            {
                if (TrackEditorControl.HeaderLine < 3)
                    return TrackHeaderColumn.None;

                if (x < ColAbs(COL_MIDIIN))
                    return TrackHeaderColumn.InChannel;

                return TrackHeaderColumn.InDevice;
            }

            return TrackHeaderColumn.None;
        }

        public Rectangle GetColumnRect(int trackY, TrackHeaderColumn column)
        {
            //int lineHeight = pxFontHeight + pxTrackMarginTop;

            switch (column)
            {
                case TrackHeaderColumn.TrackNo:
                    return new Rectangle(
                        ColX(COL_TRACKNO),
                        LineY(trackY, 0) - 1,
                        ColX(COL_MUTE) - ColX(COL_TRACKNO),
                        LineHeight);

                case TrackHeaderColumn.Mute:
                    return new Rectangle(
                        ColX(COL_MUTE),
                        LineY(trackY, 0) - 1,
                        ColX(COL_SOLO) - ColX(COL_MUTE),
                        LineHeight);

                case TrackHeaderColumn.Solo:
                    return new Rectangle(
                        ColX(COL_SOLO),
                        LineY(trackY, 0) - 1,
                        ColX(COL_ST - 1) - ColX(COL_SOLO),
                        LineHeight);

                case TrackHeaderColumn.ST:
                    return new Rectangle(
                        ColX(COL_ST),
                        LineY(trackY, 0) - 1,
                        ColX(COL_KEY) - ColX(COL_ST),
                        LineHeight);

                case TrackHeaderColumn.KEY:
                    return new Rectangle(
                        ColX(COL_KEY),
                        LineY(trackY, 0) - 1,
                        ColX(COL_TRACKNAME) - ColX(COL_KEY),
                        LineHeight);

                case TrackHeaderColumn.Name:
                    return new Rectangle(
                        ColX(COL_TRACKNAME),
                        LineY(trackY, 0) - 1,
                         GetMaxWidth() - ColAbs(COL_TRACKNAME),
                        LineHeight);



                case TrackHeaderColumn.OutChannel:
                    return new Rectangle(
                        ColX(COL_MIDIOUTCH),
                        LineY(trackY, 1) - 1,
                        ColX(COL_MIDIOUT - 1) - ColX(COL_MIDIOUTCH),
                        LineHeight);

                case TrackHeaderColumn.OutDevice:
                    return new Rectangle(
                        ColX(COL_MIDIOUT),
                        LineY(trackY, 1) - 1,
                        GetMaxWidth() - ColAbs(COL_MIDIOUT),
                        LineHeight);



                case TrackHeaderColumn.InChannel:
                    return new Rectangle(
                        ColX(COL_MIDIINCH),
                        LineY(trackY, 2) - 1,
                        ColX(COL_MIDIIN - 1) - ColX(COL_MIDIINCH),
                        LineHeight);

                case TrackHeaderColumn.InDevice:
                    return new Rectangle(
                        ColX(COL_MIDIIN),
                        LineY(trackY, 2) - 1,
                        GetMaxWidth() - ColAbs(COL_MIDIIN),
                        LineHeight);
            }

            return System.Drawing.Rectangle.Empty;
        }

        private int LineY(
            int trackY,
            int line)
        {
            return trackY
                + pxTrackMarginTop
                + line * LineHeight;
        }

        public TrackHeaderColumn NextColumn(TrackHeaderColumn col)
        {
            switch (col)
            {
                case TrackHeaderColumn.None:
                    return TrackHeaderColumn.Mute;

                case TrackHeaderColumn.TrackNo:
                    return TrackHeaderColumn.Mute;

                case TrackHeaderColumn.Mute:
                    return TrackHeaderColumn.Solo;

                case TrackHeaderColumn.Solo:
                    return TrackHeaderColumn.ST;

                case TrackHeaderColumn.ST:
                    return TrackHeaderColumn.KEY;

                case TrackHeaderColumn.KEY:
                    return TrackHeaderColumn.Name;

                case TrackHeaderColumn.Name:
                    if(TrackEditorControl.HeaderLine > 1)
                        return TrackHeaderColumn.OutChannel;
                    else
                        return TrackHeaderColumn.Mute;

                case TrackHeaderColumn.OutChannel:
                    return TrackHeaderColumn.OutDevice;

                case TrackHeaderColumn.OutDevice:
                    if(TrackEditorControl.HeaderLine > 2)
                        return TrackHeaderColumn.InChannel;
                    else
                        return TrackHeaderColumn.Mute;

                case TrackHeaderColumn.InChannel:
                    return TrackHeaderColumn.InDevice;

                case TrackHeaderColumn.InDevice:
                        return TrackHeaderColumn.Mute;
                default:
                    return col;
            }
        }

        public TrackHeaderColumn PrevColumn(TrackHeaderColumn col)
        {
            switch (col)
            {
                case TrackHeaderColumn.None:
                    return TrackHeaderColumn.Mute;

                case TrackHeaderColumn.TrackNo:
                    if (TrackEditorControl.HeaderLine > 2)
                        return TrackHeaderColumn.InDevice;
                    if (TrackEditorControl.HeaderLine > 1)
                        return TrackHeaderColumn.OutDevice;
                    return TrackHeaderColumn.Name;

                case TrackHeaderColumn.Mute:
                    if (TrackEditorControl.HeaderLine > 2)
                        return TrackHeaderColumn.InDevice;
                    if (TrackEditorControl.HeaderLine > 1)
                        return TrackHeaderColumn.OutDevice;
                    return TrackHeaderColumn.Name;

                case TrackHeaderColumn.Solo:
                    return TrackHeaderColumn.Mute;

                case TrackHeaderColumn.ST:
                    return TrackHeaderColumn.Solo;

                case TrackHeaderColumn.KEY:
                    return TrackHeaderColumn.ST;

                case TrackHeaderColumn.Name:
                    return TrackHeaderColumn.KEY;

                case TrackHeaderColumn.OutChannel:
                    return TrackHeaderColumn.Name;

                case TrackHeaderColumn.OutDevice:
                    return TrackHeaderColumn.OutChannel;

                case TrackHeaderColumn.InChannel:
                    return TrackHeaderColumn.OutDevice;

                case TrackHeaderColumn.InDevice:
                    return TrackHeaderColumn.InChannel;
            }

            return col;
        }

    }
}
