using Remcoposer64.StepEditorPanelControl.Render;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice;
using Vortice.DCommon;
using Vortice.Direct2D1;
using Vortice.DirectWrite;
using Vortice.DXGI;
using Vortice.Mathematics;
using Vortice.WIC;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class GraphVortice :IGraph, IDisposable
    {
        private ID2D1Factory1 d2dFactory;
        private IDWriteFactory dwFactory;
        private ID2D1HwndRenderTarget renderTarget;

        private IDWriteTextFormat textFormat;
        private ID2D1SolidColorBrush brushFg;
        private ID2D1SolidColorBrush brushBg;

        private int fontSize = 16;
        private AsciiMonoCache cache;
        //private ID2D1Bitmap cacheDxMaskBitmap;
        private Dictionary<int, ID2D1Bitmap> dicCacheDxMaskBitmap = new Dictionary<int, ID2D1Bitmap>();
        private string FontName = "Meiryo";//"Courier New";
        private Point _Offset = Point.Empty;
        private byte[] cacheDxMaskPixels;
        private SizeI cacheDxMaskSize;
        private uint cacheDxMaskStride;
        private IWICImagingFactory wicFactory = new IWICImagingFactory();

        public System.Drawing.Color ForeColor { set { brushFg = renderTarget.CreateSolidColorBrush(new Color4(value.R / 255f, value.G / 255f, value.B / 255f, 1)); } }
        public System.Drawing.Color BackColor { set { brushBg = renderTarget.CreateSolidColorBrush(new Color4(value.R / 255f, value.G / 255f, value.B / 255f, 1)); } }

        public GraphVortice(IntPtr hwnd)
        {
            // Direct2D Factory
            d2dFactory = D2D1.D2D1CreateFactory<ID2D1Factory1>();

            // DirectWrite Factory
            dwFactory = DWrite.DWriteCreateFactory<IDWriteFactory>();

            // RenderTargetProperties
            var rtProps = new RenderTargetProperties(
                RenderTargetType.Default,
                new Vortice.DCommon.PixelFormat(Format.B8G8R8A8_UNorm,Vortice.DCommon.AlphaMode.Ignore),
                96.0F, 96.0F,
                RenderTargetUsage.None,
                FeatureLevel.Level_10);

            var clientSize = GetClientSize(hwnd);

            // HwndRenderTargetProperties
            var hwndProps = new HwndRenderTargetProperties()
            {
                Hwnd = hwnd,
                PixelSize = new SizeI(clientSize.Width, clientSize.Height),
                PresentOptions = PresentOptions.None,                
            };

            // RenderTarget 作成
            renderTarget = d2dFactory.CreateHwndRenderTarget(rtProps, hwndProps);

            // フォント
            textFormat = dwFactory.CreateTextFormat(FontName, fontSize);

            // ブラシ
            brushFg = renderTarget.CreateSolidColorBrush(new Color4(1, 1, 1, 1));
            brushBg = renderTarget.CreateSolidColorBrush(new Color4(0, 0, 0, 1));

            using var bmp = new Bitmap("Resource\\rFont_16.png");
            fontSize = 16;
            cache = new AsciiMonoCache(bmp, 16, 16);

            CreateBitmapFromDIB(cache.MaskBitmap);
        }

        public void SetFontSize(int size)
        {
            fontSize = size;
            textFormat?.Dispose();
            //textFormat = dwFactory.CreateTextFormat("Courier New", fontSize);
            textFormat = dwFactory.CreateTextFormat(FontName, fontSize);
            using var bmp = new Bitmap(string.Format("Resource\\rFont_{0:d02}.png", fontSize));
            cache?.Dispose();
            cache = new AsciiMonoCache(bmp, fontSize, fontSize);

            CreateBitmapFromDIB(cache.MaskBitmap);
        }

        public void EndPaint()
        {
            renderTarget.EndDraw();
        }

        public void BeginPaint()
        {
            renderTarget.BeginDraw();
            renderTarget.Transform = Matrix3x2.Identity;
            _Offset = Point.Empty; // オフセットなし
        }

        public void BeginPaint(Rectangle rect)
        {
            renderTarget.BeginDraw();
            renderTarget.Transform = Matrix3x2.Identity;
            _Offset = rect.Location;
        }

        public void FillRectangle(int x, int y, int width, int height)
        {
            renderTarget.FillRectangle(new Vortice.RawRectF(x, y, x+width, y+height), brushBg);
        }

        public void DrawLine(int x1, int y1, int x2, int y2, System.Drawing.Color color)
        {
            brushFg.Color = new Color4(color.R / 255f, color.G / 255f, color.B / 255f, 1);

            renderTarget.DrawLine(
                new Vector2(x1, y1),
                new Vector2(x2, y2),
                brushFg,
                1.0f);
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
            brushFg.Color = new Color4(fg.R / 255f, fg.G / 255f, fg.B / 255f, 1);
            brushBg.Color = new Color4(bg.R / 255f, bg.G / 255f, bg.B / 255f, 1);
            FillRectangle(pos.X, pos.Y, (int)(text.Length * fontSize * 0.6), fontSize);

            using (var layout = dwFactory.CreateTextLayout(text, textFormat, 2000, 2000))
            {
                var metrics = layout.Metrics;

                int osHeight = (int)Math.Ceiling(metrics.Height);
                var lineHeight = fontSize;
                // ★ lineHeight の中央に OSフォントを合わせる補正
                int offsetY = (lineHeight - osHeight) / 2;

                // 描画位置を補正
                var drawPos = new Vector2(pos.X, pos.Y + offsetY);

                // 背景塗りつぶし（必要なら）
                FillRectangle(pos.X, pos.Y, (int)metrics.Width, lineHeight);

                // 描画
                renderTarget.DrawTextLayout(drawPos, layout, brushFg);

                // pos の更新（X だけ進める）
                pos.X += (int)metrics.Width;
            }
        }

        public int MeasureStringOSFont(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            var lineHeight = fontSize;
            var layout = dwFactory.CreateTextLayout(text, textFormat, 1000, lineHeight);
            var metrics = layout.Metrics;

            return (int)Math.Ceiling(metrics.Width);
        }

        public void DrawGlyph(int x, int y, AsciiMonoCache.Glyph g, System.Drawing.Color fg, System.Drawing.Color bg)
        {
            BackColor = bg;
            FillRectangle(x, y, g.Width, g.Height);

            int colKey = fg.B << 16 | fg.G << 8 | fg.R;
            if (!dicCacheDxMaskBitmap.ContainsKey(colKey))
            {
                MakeCacheDxMaskBitmap(fg);
            }

            renderTarget.DrawBitmap(
                dicCacheDxMaskBitmap[colKey]
                , new Rect(x, y, g.Width, g.Height)
                , 1.0f
                , Vortice.Direct2D1.BitmapInterpolationMode.NearestNeighbor
                , new Rect(g.X, g.Y, g.Width, g.Height)
                );
        }

        void IDisposable.Dispose()
        {
            brushFg?.Dispose();
            brushBg?.Dispose();
            textFormat?.Dispose();
            renderTarget?.Dispose();
            dwFactory?.Dispose();
            d2dFactory?.Dispose();
        }

        public void Resize(int width, int height)
        {
            if (renderTarget != null && width > 0 && height > 0)
            {
                renderTarget.Resize(new SizeI(width, height));
            }
        }

        // Win32 API の定義
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        /// <summary>
        /// ウィンドウハンドルからクライアント領域のサイズを取得する
        /// </summary>
        public static System.Drawing.Size GetClientSize(IntPtr hwnd)
        {
            if (GetClientRect(hwnd, out RECT rect))
            {
                // Right は幅 (Width)、Bottom は高さ (Height) になる
                return new System.Drawing.Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            return System.Drawing.Size.Empty;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public ushort bmPlanes;
            public ushort bmBitsPixel;
            public IntPtr bmBits;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern int GetObject(
            IntPtr hObject,
            int nSize,
            ref BITMAP lpObject);

        public void CreateBitmapFromDIB(IntPtr hBitmap)
        {
            // DIB の情報を取得
            BITMAP bmp = new BITMAP();
            GetObject(hBitmap, Marshal.SizeOf<BITMAP>(), ref bmp);

            uint width = (uint)bmp.bmWidth;
            uint height = (uint)bmp.bmHeight;
            uint stride = (uint)bmp.bmWidthBytes;

            // ピクセルデータをコピー
            byte[] pixels = new byte[stride * height];
            Marshal.Copy(bmp.bmBits, pixels, 0, pixels.Length);

            cacheDxMaskSize = new SizeI((int)width, (int)height);
            cacheDxMaskStride = stride;
            cacheDxMaskPixels = pixels;
        }

        private void MakeCacheDxMaskBitmap(System.Drawing.Color fg)
        {
            int w = cacheDxMaskSize.Width;
            int h = cacheDxMaskSize.Height;
            int stride = w * 4;

            byte[] bgra = new byte[w * h * 4];

            byte fgB = fg.B;
            byte fgG = fg.G;
            byte fgR = fg.R;

            for (int y = 0; y < h; y++)
            {
                int rowByteIndex = (int)(y * cacheDxMaskStride);
                int dst = y * w * 4;

                for (int x = 0; x < w; x++)
                {
                    int byteIndex = rowByteIndex + (x >> 3);
                    int bitIndex = 7 - (x & 7);
                    byte mask = (byte)(((cacheDxMaskPixels[byteIndex] >> bitIndex) & 1) * 255);
                    bgra[dst + 0] = (byte)(fgB & mask);
                    bgra[dst + 1] = (byte)(fgG & mask);
                    bgra[dst + 2] = (byte)(fgR & mask);
                    bgra[dst + 3] = mask;
                    dst += 4;
                }
            }

            using var wicBitmap = wicFactory.CreateBitmapFromMemory(
                (uint)w,
                (uint)h,
                Vortice.WIC.PixelFormat.Format32bppPBGRA,
                bgra,
                (uint)stride);

            int colKey = (fg.B << 16) | (fg.G << 8) | fg.R;
            dicCacheDxMaskBitmap[colKey] = renderTarget.CreateBitmapFromWicBitmap(wicBitmap);
        }

    }
}
