using Remcoposer64.StepEditorPanelControl;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class AsciiMonoCache : IDisposable
    {
        int GlyphWidth = 12;
        int GlyphHeight = 16;

        readonly IntPtr _screenDC;
        readonly IntPtr _font;

        public IntPtr MaskBitmap;   // 全文字が並んだ巨大マスク
        public IntPtr MaskDC;       // MaskBitmap を SelectObject 済みの DC

        public struct Glyph
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
        }

        readonly Dictionary<char, Glyph> _glyphs = new();

        public AsciiMonoCache(Font font)
        {
            _screenDC = WinApi.GetDC(IntPtr.Zero);
            _font = font.ToHfont();
        }

        public AsciiMonoCache(Bitmap bitmap, int glyphWidth, int glyphHeight)
        {
            GlyphWidth = glyphWidth;
            GlyphHeight = glyphHeight;

            _screenDC = WinApi.GetDC(IntPtr.Zero);
            _font = IntPtr.Zero; // フォントは使わない

            GenerateGlyphsFromBitmap(bitmap);
        }

        public void Dispose()
        {
            if (MaskDC != IntPtr.Zero)
            {
                WinApi.DeleteDC(MaskDC);
                MaskDC = IntPtr.Zero;
            }

            if (MaskBitmap != IntPtr.Zero)
            {
                WinApi.DeleteObject(MaskBitmap);
                MaskBitmap = IntPtr.Zero;
            }

            WinApi.ReleaseDC(IntPtr.Zero, _screenDC);

            if (_font != IntPtr.Zero)
                WinApi.DeleteObject(_font);
        }

        private void GenerateGlyphsFromBitmap(Bitmap bmp)
        {
            MaskBitmap = CreateMonoMaskFromBitmap(bmp); // ← 全体を1bit化
            MaskDC = WinApi.CreateCompatibleDC(_screenDC);
            WinApi.SelectObject(MaskDC, MaskBitmap);

            for (char c = (char)0x20; c <= (char)0x7E; c++)
            {
                int index = c - 0x20;
                int columns = bmp.Width / GlyphWidth;

                int x = (index % columns) * GlyphWidth;
                int y = (index / columns) * GlyphHeight;

                _glyphs[c] = new Glyph
                {
                    X = x,
                    Y = y,
                    Width = GlyphWidth,
                    Height = GlyphHeight
                };
            }
        }

        public Glyph GetGlyph(char c)
        {
            return _glyphs.TryGetValue(c, out var g) ? g : default;
        }

        private IntPtr CreateMonoMaskFromBitmap(Bitmap src)
        {
            IntPtr memDC = WinApi.CreateCompatibleDC(_screenDC);

            // 1bit DIBSection を作成
            WinApi.BITMAPINFO bmi = new WinApi.BITMAPINFO();
            bmi.bmiHeader.biSize = (uint)Marshal.SizeOf<WinApi.BITMAPINFOHEADER>();
            bmi.bmiHeader.biWidth = src.Width;// GlyphWidth;
            bmi.bmiHeader.biHeight = -src.Height;// GlyphHeight; // top-down
            bmi.bmiHeader.biPlanes = 1;
            bmi.bmiHeader.biBitCount = 1;
            bmi.bmiHeader.biCompression = WinApi.BI_RGB;

            // ★ ここ追加：1bit用に2色パレット（黒・白）
            bmi.bmiColors = new uint[2];
            bmi.bmiColors[0] = 0x00000000; // 黒
            bmi.bmiColors[1] = 0x00FFFFFF; // 白

            IntPtr bits;
            IntPtr monoBmp = WinApi.CreateDIBSection(memDC, ref bmi, WinApi.DIB_RGB_COLORS, out bits, IntPtr.Zero, 0);

            IntPtr oldBmp = WinApi.SelectObject(memDC, monoBmp);

            // src を走査して 1bit に変換
            for (int y = 0; y < src.Height; y++)
            {
                for (int x = 0; x < src.Width; x++)
                {
                    Color c = src.GetPixel(x, y);

                    bool isForeground = c.R > 128 || c.G > 128 || c.B > 128; // 白系を文字とみなす

                    Set1BitPixel(bits, x, y, isForeground, src.Width);
                }
            }

            WinApi.SelectObject(memDC, oldBmp);
            WinApi.DeleteDC(memDC);

            return monoBmp;
        }

        private unsafe void Set1BitPixel(IntPtr bits, int x, int y, bool on, int width)
        {
            byte* ptr = (byte*)bits.ToPointer();
            int stride = ((width + 31) / 32) * 4; // 1bit の行サイズ（DWORD境界）

            int byteIndex = y * stride + (x >> 3);
            int bitMask = 0x80 >> (x & 7);

            if (on)
                ptr[byteIndex] |= (byte)bitMask;
            else
                ptr[byteIndex] &= (byte)~bitMask;
        }
    }
}