using Remcoposer64.StepEditorPanelControl.Render;
using System.Drawing;
using Vortice.Mathematics;
//using Vortice.Mathematics;
using static Remcoposer64.StepEditorPanelControl.WinApi;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    //
    // 参考:Azukiのソースコードを参考にしています。
    //      PlatWin.cs
    //

    public class GraphGDI : IGraph,IDisposable
    {
        IntPtr _Window = IntPtr.Zero;
        IntPtr _DC = IntPtr.Zero;
        IntPtr _MemDC = IntPtr.Zero;
        Point _Offset = Point.Empty;
        IntPtr _MemBmp = IntPtr.Zero;
        IntPtr _OrgMemBmp;
        System.Drawing.Size _MemDcSize;
        int _ForeColor;
        IntPtr _Pen = IntPtr.Zero;
        IntPtr _Brush = IntPtr.Zero;
        IntPtr _NullPen = IntPtr.Zero;
        IntPtr _Font = IntPtr.Zero;
        private AsciiMonoCache cache;
        private readonly System.Drawing.Font font = new("Courier New", 12, FontStyle.Regular);
        IntPtr _WorkOldBmp = IntPtr.Zero;
        IntPtr _WorkFgDC = IntPtr.Zero;
        IntPtr _WorkFgBmp = IntPtr.Zero;
        IntPtr _WorkOldFgBmp = IntPtr.Zero;
        IntPtr _WorkBgDC = IntPtr.Zero;
        IntPtr _WorkBgBmp = IntPtr.Zero;
        IntPtr _WorkOldBgBmp = IntPtr.Zero;
        System.Drawing.Size fontSize= new System.Drawing.Size();
        Dictionary<int, IntPtr> pens = new Dictionary<int, IntPtr>();
        Dictionary<int, IntPtr> brushes = new Dictionary<int, IntPtr>();


        public GraphGDI(IntPtr handle)
        {
            _Window = handle;
            _DC = WinApi.GetDC(handle);

            using var bmp = new Bitmap("Resource\\rFont_16.png");
            fontSize.Width =16; fontSize.Height =16;
            cache = new AsciiMonoCache(bmp,16,16);
            CreateMeiryoFont(16); // 好きなサイズ

        }

        private void CreateMeiryoFont(int size)
        {
            LOGFONT lf = new LOGFONT();
            lf.lfHeight = -size;        // 負値 → 高さを指定
            lf.lfWeight = 400;          // 通常
            lf.lfCharSet = 0;           // DEFAULT_CHARSET
            lf.lfFaceName = "Meiryo";   // ★ここが重要

            _Font = CreateFontIndirect(ref lf);
        }

        public void SetFontSize(int size)
        {
            // 古いキャッシュを破棄
            cache?.Dispose();

            // 新しいフォント画像を読み込む
            // 例: rFont_08.png / rFont_12.png / rFont_16.png を用意する
            string file = size switch
            {
                8 => "Resource\\rFont_08.png",
                12 => "Resource\\rFont_12.png",
                16 => "Resource\\rFont_16.png", // 既存の16pxフォント
                _ => "Resource\\rFont_16.png"
            };

            using var bmp = new Bitmap(file);
            fontSize.Width = size; fontSize.Height = size;
            cache = new AsciiMonoCache(bmp, size, size);
        }

        public void Dispose()
        {

            if (_MemDC != IntPtr.Zero)
            {
                SelectObject(_MemDC, _OrgMemBmp);
                if (_MemBmp != IntPtr.Zero) DeleteObject(_MemBmp);
                DeleteDC(_MemDC);
            }

            SafeDeleteObject(_Pen);
            SafeDeleteObject(_Brush);
            SafeDeleteObject(_NullPen);
            if (_Font != IntPtr.Zero)
                SafeDeleteObject(_Font);
            cache.Dispose();
            foreach (var item in pens.Values)SafeDeleteObject(item);
            foreach (var item in brushes.Values) SafeDeleteObject(item);
        }

        public void BeginPaint(Rectangle paintRect)
        {
            // メイン描画用 DC
            _MemDC = CreateCompatibleDC(_DC);
            _MemBmp = CreateCompatibleBitmap(_DC, paintRect.Width, paintRect.Height);
            _OrgMemBmp = SelectObject(_MemDC, _MemBmp);

            _Offset = paintRect.Location;
            _MemDcSize = paintRect.Size;

            // 前景用 DC
            _WorkFgDC = CreateCompatibleDC(_DC);
            _WorkFgBmp = CreateCompatibleBitmap(_DC, fontSize.Width, fontSize.Height);
            SelectObject(_WorkFgDC, _WorkFgBmp);

            // 背景用 DC
            _WorkBgDC = CreateCompatibleDC(_DC);
            _WorkBgBmp = CreateCompatibleBitmap(_DC, fontSize.Width, fontSize.Height);
            SelectObject(_WorkBgDC, _WorkBgBmp);

        }

        public void EndPaint()
        {
            if (_MemDC != IntPtr.Zero)
            {
                BitBlt(_DC, _Offset.X, _Offset.Y, _MemDcSize.Width, _MemDcSize.Height,
                       _MemDC, 0, 0, SRCCOPY);
            }

            // メイン描画用 DC の破棄
            if (_MemDC != IntPtr.Zero)
            {
                SelectObject(_MemDC, _OrgMemBmp);
                DeleteObject(_MemBmp);   // 先に Bitmap
                DeleteDC(_MemDC);        // 後に DC
                _MemDC = IntPtr.Zero;
                _MemBmp = IntPtr.Zero;
            }

            DeleteObject(_WorkFgBmp);
            DeleteDC(_WorkFgDC);
            _WorkFgDC = IntPtr.Zero;
            _WorkFgBmp = IntPtr.Zero;

            DeleteObject(_WorkBgBmp);
            DeleteDC(_WorkBgDC);
            _WorkBgDC = IntPtr.Zero;
            _WorkBgBmp = IntPtr.Zero;

            RemoveClipRect();
            _Offset = Point.Empty;
        }

        public void RemoveClipRect()
        {
            WinApi.SelectClipRgn(DC, IntPtr.Zero);
        }

        IntPtr NullPen
        {
            get
            {
                const int PS_NULL = 5;
                if (_NullPen == IntPtr.Zero)
                {
                    _NullPen = WinApi.CreatePen(PS_NULL, 0, 0);
                }
                return _NullPen;
            }
        }

        IntPtr DC
        {
            get
            {
                return (_MemDC != IntPtr.Zero) ? _MemDC
                                               : _DC;
            }
        }

        static void SafeDeleteObject(IntPtr gdiObj)
        {
            if (gdiObj != IntPtr.Zero)
                WinApi.DeleteObject(gdiObj);
        }

        public System.Drawing.Color ForeColor
        {
            set
            {
                int foreColor = (value.R) | (value.G << 8) | (value.B << 16);
                if (pens.ContainsKey(foreColor))
                {
                    _Pen = pens[_ForeColor];
                }
                else
                {
                    _Pen = WinApi.CreatePen(0, 1, foreColor);
                    pens.Add(foreColor, _Pen);
                }
            }
        }

        /// <summary>
        /// Background color used by drawing APIs.
        /// </summary>
        public System.Drawing.Color BackColor
        {
            set
            {
                int colorRef = (value.R) | (value.G << 8) | (value.B << 16);

                if (brushes.ContainsKey(colorRef))
                {
                    _Brush = brushes[colorRef];
                }
                else
                {
                    _Brush = WinApi.CreateSolidBrush(colorRef);
                    brushes.Add(colorRef, _Brush);
                }
            }
        }


        public void FillRectangle(int x, int y, int width, int height)
        {
            FillRectangle(DC, x, y, width, height);
        }

        public void FillRectangle(IntPtr dc,int x, int y, int width, int height)
        {
            x -= _Offset.X;
            y -= _Offset.Y;
            RECT r = new RECT { left = x, top = y, right = x + width, bottom = y + height };
            WinApi.FillRect(dc, ref r, _Brush);
        }

        public void DrawText(string text, ref Point position, System.Drawing.Color foreColor)
        {

            var x = position.X - _Offset.X;
            var y = position.Y - _Offset.Y;

            var newFont = _Font;
            var oldFont = WinApi.SelectObject(DC, newFont);
            var oldForeColor = WinApi.SetTextColor(DC, foreColor);

            WinApi.SetTextAlign(DC, false);
            WinApi.SetBkMode(DC,WinApi.TRANSPARENT);
            //WinApi.SetBkMode(DC, OPAQUE);
            WinApi.ExtTextOut(DC, x, y, 0, text);

            WinApi.SetTextColor(DC, oldForeColor);
            WinApi.SelectObject(DC, oldFont);
        }

        public void DrawGlyph(int x, int y, AsciiMonoCache.Glyph g, System.Drawing.Color fg, System.Drawing.Color bg)
        {
            int dx = x - _Offset.X;
            int dy = y - _Offset.Y;

            int w = g.Width;
            int h = g.Height;

            //_WorkOldBmp = WinApi.SelectObject(_WorkDC, g.MaskBitmap);
            //
            // 指定した文字色のフォントの画像を生成
            //
            // 1. 文字色矩形 AND MaskBitmap
            // 文字色で塗る
            BackColor = fg;
            FillRectangle(_WorkFgDC, 0, 0, w, h);
            // 文字色の背景から、文字の形の部分だけ抜き取る(AND)
            WinApi.BitBlt(_WorkFgDC, 0, 0, w, h, cache.MaskDC, g.X, g.Y, WinApi.SRCAND);

            //
            // 指定した背景色からフォントをくり抜いた画像を生成
            //
            // 2. 背景色矩形 AND !MaskBitmap（NOTSRCAND）
            // 背景色で塗る
            BackColor = bg;
            FillRectangle(_WorkBgDC, 0, 0, w, h);
            // 背景色から文字の形の部分だけくり抜く(nand)
            WinApi.BitBlt(_WorkBgDC, 0, 0, w, h, cache.MaskDC, g.X, g.Y, WinApi.NOTSRCAND);

            // 3. OR 合成(1と2を合成)
            WinApi.BitBlt(DC, dx, dy, w, h, _WorkBgDC, 0, 0, WinApi.SRCCOPY);//背景をそのまま描画
            WinApi.BitBlt(DC, dx, dy, w, h, _WorkFgDC, 0, 0, WinApi.SRCPAINT);//文字をORで描画

            // 後始末
            //WinApi.SelectObject(_WorkDC, _WorkOldBmp);
        }

        public void DrawString(string text, ref Point position, System.Drawing.Color fg, System.Drawing.Color bg)
        {
            int cx = position.X - _Offset.X;
            int cy = position.Y - _Offset.Y;

            int lineStartX = cx;

            foreach (char ch in text)
            {
                var glyph = cache.GetGlyph(ch);

                if (ch == '\n')
                {
                    cx = lineStartX;
                    cy += glyph.Height;
                    continue;
                }

                if (ch == '\t')
                {
                    cx += glyph.Width * 4;
                    continue;
                }

                DrawGlyph(cx + _Offset.X, cy + _Offset.Y, glyph, fg, bg);

                cx += glyph.Width;
            }

            position.X = cx + _Offset.X;
            position.Y = cy + _Offset.Y;
        }

        public void DrawStringOSFont(string text, ref Point pos, System.Drawing.Color fg, System.Drawing.Color bg)
        {
            // --- 1. フォント選択 ---
            IntPtr oldFont = SelectObject(DC, _Font);

            // --- 2. テキストサイズ取得 ---
            SIZE size;
            GetTextExtentPoint32(DC, text, text.Length, out size);

            // OSフォントの高さを取得
            TEXTMETRIC tm;
            GetTextMetrics(DC, out tm);
            int osHeight = tm.tmHeight;   // フォントの実際の高さ

            // lineHeight は ASCII グリフの高さとして扱う
            int lineHeight = fontSize.Height;

            // --- 3. lineHeight の中央に OSフォントを合わせる補正 ---
            int offsetY = (lineHeight - osHeight) / 2;

            int drawX = pos.X - _Offset.X;
            int drawY = pos.Y - _Offset.Y + offsetY;

            // --- 4. 背景塗りつぶし（行の高さで塗る） ---
            RECT r = new RECT
            {
                left = drawX,
                top = pos.Y - _Offset.Y,
                right = drawX + size.cx,
                bottom = pos.Y - _Offset.Y + lineHeight
            };
            FillRect(DC, ref r, _Brush);

            // --- 5. 文字描画 ---
            SetTextColor(DC, (fg.R | (fg.G << 8) | (fg.B << 16)));
            SetBkMode(DC, TRANSPARENT);

            ExtTextOut(DC, drawX, drawY, 0, IntPtr.Zero, text, text.Length, IntPtr.Zero);

            // --- 6. 後始末 ---
            SelectObject(DC, oldFont);

            // --- 7. pos.X を進める ---
            pos.X += size.cx;
        }

        public void DrawLine(int x1, int y1, int x2, int y2, System.Drawing.Color color)
        {
            // オフセット補正（BeginPaint の矩形位置）
            x1 -= _Offset.X;
            y1 -= _Offset.Y;
            x2 -= _Offset.X;
            y2 -= _Offset.Y;

            // ペン作成
            IntPtr pen = WinApi.CreatePen(0, 1, (color.R | (color.G << 8) | (color.B << 16)));
            IntPtr oldPen = WinApi.SelectObject(DC, pen);

            // 描画
            WinApi.MoveToEx(DC, x1, y1, IntPtr.Zero);
            WinApi.LineTo(DC, x2, y2);

            // 後始末
            WinApi.SelectObject(DC, oldPen);
            WinApi.DeleteObject(pen);
        }

        public void Resize(int width, int height)
        {
        }

    }
}
