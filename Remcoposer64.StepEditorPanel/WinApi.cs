using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
//using Vortice.Mathematics;

namespace Remcoposer64.StepEditorPanelControl
{
    public static class WinApi
    {
        #region DLL Names
        const string kernel32_dll = "kernel32";
        const string user32_dll = "user32";
        const string gdi32_dll = "gdi32";
        const string imm32_dll = "imm32";
        #endregion
        const uint TA_LEFT = 0;
        const uint TA_RIGHT = 2;
        const uint TA_CENTER = 6;
        const uint TA_NOUPDATECP = 0;
        const uint TA_TOP = 0;
        public const int TRANSPARENT = 1;
        public const int OPAQUE = 2;
        public const uint SRCCOPY = 0x00CC0020;
        public const uint SRCAND = 0x008800C6;
        public const uint NOTSRCAND = 0x002200A8;
        public const uint SRCPAINT = 0x00EE0086;
        public const uint MASKPAINT = 0x00E20746;
        public const int BI_RGB = 0;
        public const int DIB_RGB_COLORS = 0;

        [DllImport(user32_dll)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport(gdi32_dll)]
        public unsafe static extern IntPtr SelectObject(IntPtr hdc, IntPtr gdiObj);

        [DllImport(user32_dll)]
        public static extern Int32 ReleaseDC(IntPtr hWnd, IntPtr dc);

        [DllImport(gdi32_dll)]
        public static unsafe extern Int32 DeleteDC(IntPtr hdc);

        [DllImport(gdi32_dll)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport(gdi32_dll)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, Int32 width, Int32 height);

        [DllImport(gdi32_dll)]
        public static extern Int32 BitBlt(
            IntPtr destination, Int32 destX, Int32 destY, Int32 width, Int32 height,
            IntPtr source, Int32 srcX, Int32 srcY, UInt32 rasterOpCode);

        [DllImport(gdi32_dll)]
        public static extern Int32 SelectClipRgn(IntPtr hdc, IntPtr regionHdl);

        [DllImport(gdi32_dll)]
        public unsafe static extern Int32 DeleteObject(IntPtr gdiObj);

        [DllImport(gdi32_dll)]
        public unsafe static extern Int32 Rectangle(IntPtr hdc, Int32 left, Int32 top, Int32 right, Int32 bottom);

        [DllImport(gdi32_dll)]
        public unsafe static extern IntPtr CreatePen(Int32 style, Int32 width, Int32 color);

        [DllImport(gdi32_dll)]
        public unsafe static extern IntPtr CreateSolidBrush(Int32 color);

        [DllImport(gdi32_dll)]
        public static extern Int32 SetTextColor(IntPtr hdc, Int32 color);
        public static Int32 SetTextColor(IntPtr hdc, Color color)
        {
            int bgr = (color.B << 16) | (color.G << 8) | color.R;
            return SetTextColor(hdc, bgr);
        }

        [DllImport(gdi32_dll)]
        public static extern Int32 SetBkColor(IntPtr hdc, Int32 color);
        public static Int32 SetBkColor(IntPtr hdc, Color color)
        {
            int bgr = (color.B << 16) | (color.G << 8) | color.R;
            return SetBkColor(hdc, bgr);
        }

        [DllImport(gdi32_dll)]
        public static extern Int32 SetBkMode(IntPtr hdc, Int32 mode);

        [DllImport(gdi32_dll)]
        static extern UInt32 SetTextAlign(IntPtr hdc, UInt32 mode);
        public static void SetTextAlign(IntPtr hdc, bool alignRight)
        {
            uint flag = TA_TOP | TA_NOUPDATECP;
            flag |= alignRight ? TA_RIGHT : TA_LEFT;
            uint rc = SetTextAlign(hdc, flag);
            Debug.Assert(rc != UInt32.MaxValue, "failed to set text alignment by SetTextAlign.");
        }

        [DllImport(gdi32_dll, CharSet = CharSet.Unicode)]
        unsafe static extern bool ExtTextOutW(IntPtr hdc, Int32 x, Int32 y, UInt32 formatOptions, RECT* bounds, string text, UInt32 textLength, IntPtr zero);
        public static bool ExtTextOut(IntPtr hdc, int x, int y, int formatOptions, string text)
        {
            unsafe
            {
                return ExtTextOutW(hdc, x, y, (uint)formatOptions, null, text, (UInt32)text.Length, IntPtr.Zero);
            }
        }
        public static bool ExtTextOut(IntPtr hdc, int x, int y, Rectangle bounds, int formatOptions, string text)
        {
            unsafe
            {
                RECT rect = new RECT(bounds);
                return ExtTextOutW(hdc, x, y, (uint)formatOptions, &rect, text, (UInt32)text.Length, IntPtr.Zero);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public RECT(Rectangle rect)
            {
                left = rect.Left;
                top = rect.Top;
                right = rect.Right;
                bottom = rect.Bottom;
            }
            public Int32 left, top, right, bottom;
        }

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateBitmap(int nWidth, int nHeight, uint nPlanes, uint nBitCount, IntPtr lpBits);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool MaskBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, IntPtr hbmMask, int xMask, int yMask, uint rop);

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;

            // 1bit / 4bit / 8bit の場合はパレットが続くが、
            // 今回は 1bit 固定なので 2色だけ確保しておく
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] bmiColors;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public uint biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BITMAPINFO pbmi, uint iUsage, out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

        public const int GWL_WNDPROC = -4;
        public const uint WM_PAINT = 0x000F;

        public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll")]
        public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static unsafe extern IntPtr BeginPaint(IntPtr hWnd, PAINTSTRUCT* ps);

        [DllImport("user32.dll")]
        public static extern unsafe Int32 EndPaint(IntPtr hWnd, PAINTSTRUCT* ps);

        [StructLayout(LayoutKind.Sequential)]
        public struct PAINTSTRUCT
        {
            public IntPtr hDC;
            public Int32 bErase;
            public RECT paint;
            public Int32 bRestore;
            public Int32 bIncUpdate;
            public Int64 reserved0, reserved1, reserved2, reserved3, reserved4;
        }

        [DllImport("gdi32.dll")]
        public static extern bool MoveToEx(
            IntPtr hdc,
            int X,
            int Y,
            IntPtr lpPoint);

        [DllImport("gdi32.dll")]
        public static extern bool LineTo(
            IntPtr hdc,
            int nXEnd,
            int nYEnd);

        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern bool PatBlt(
            IntPtr hdc,
            int x,
            int y,
            int width,
            int height,
            uint rop
        );
        [DllImport("user32.dll")]
        public static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hBrush);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetTextExtentPoint32(
            IntPtr hdc,
            string lpString,
            int cbString,
            out SIZE lpSize);

        [DllImport("gdi32.dll")]
        public static extern bool GetTextMetrics(IntPtr hdc, out TEXTMETRIC lptm);

        [StructLayout(LayoutKind.Sequential)]
        public struct TEXTMETRIC
        {
            public int tmHeight;
            public int tmAscent;
            public int tmDescent;
            public int tmInternalLeading;
            public int tmExternalLeading;
            public int tmAveCharWidth;
            public int tmMaxCharWidth;
            public int tmWeight;
            public int tmOverhang;
            public int tmDigitizedAspectX;
            public int tmDigitizedAspectY;
            public char tmFirstChar;
            public char tmLastChar;
            public char tmDefaultChar;
            public char tmBreakChar;
            public byte tmItalic;
            public byte tmUnderlined;
            public byte tmStruckOut;
            public byte tmPitchAndFamily;
            public byte tmCharSet;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern bool ExtTextOut(
            IntPtr hdc,
            int x,
            int y,
            uint options,
            IntPtr lprect,      // ← RECT* なので IntPtr で OK
            string lpString,
            int cchString,
            IntPtr lpDx         // ← int* なので IntPtr で OK
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public int lfWeight;
            public byte lfItalic;
            public byte lfUnderline;
            public byte lfStrikeOut;
            public byte lfCharSet;
            public byte lfOutPrecision;
            public byte lfClipPrecision;
            public byte lfQuality;
            public byte lfPitchAndFamily;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateFontIndirect(ref LOGFONT lplf);

        public const uint PATCOPY = 0x00F00021;

        [DllImport("imm32.dll")]
        public static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
        public static extern int ImmGetCompositionStringW(
            IntPtr hIMC,
            int dwIndex,
            byte[] lpBuf,
            int dwBufLen);

        [DllImport("imm32.dll")]
        public static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        public const int WM_IME_STARTCOMPOSITION = 0x010D;
        public const int WM_IME_ENDCOMPOSITION = 0x010E;
        public const int WM_IME_COMPOSITION = 0x010F;
        public const int WM_IME_SETCONTEXT = 0x0281;
        public const int GCS_COMPSTR = 0x0008;   // 変換中文字（composition string）
        public const int GCS_RESULTSTR = 0x0800;   // 確定文字列（result string）
        public const int GCS_CURSORPOS = 0x0080;

        [StructLayout(LayoutKind.Sequential)]
        public struct COMPOSITIONFORM
        {
            public uint dwStyle;
            public POINT ptCurrentPos;
            public RECT2 rcArea;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT2
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        public enum ImmAssociateContextExFlags : uint
        {
            IACE_CHILDREN = 0x0001,
            IACE_DEFAULT = 0x0010,
            IACE_IGNORENOCONTEXT = 0x0020
        }

        [DllImport("imm32.dll", CharSet = CharSet.Unicode)]
        public static extern bool ImmSetCompositionWindow(IntPtr hIMC, ref COMPOSITIONFORM compForm);
        [DllImport("imm32.dll")]
        public static extern IntPtr ImmCreateContext();
        [DllImport("imm32.dll")]
        public static extern bool ImmAssociateContextEx(IntPtr hWnd, IntPtr hIMC, ImmAssociateContextExFlags dwFlags);
        [StructLayout(LayoutKind.Sequential)]
        public struct CANDIDATEFORM
        {
            public int dwIndex;
            public int dwStyle;
            public POINT ptCurrentPos;
            public RECT rcArea;
        }
        [DllImport("imm32.dll")]
        public static extern bool ImmSetCandidateWindow(IntPtr hIMC, ref CANDIDATEFORM candForm);

        #region Keyboard
        public static Keys GetCurrentModifierKeyStates()
        {
            Keys modifiers = Keys.None;

            modifiers |= IsKeyDown(Keys.Menu) ? Keys.Alt : Keys.None;
            modifiers |= IsKeyDown(Keys.ControlKey) ? Keys.Control : Keys.None;
            modifiers |= IsKeyDown(Keys.ShiftKey) ? Keys.Shift : Keys.None;

            return modifiers;
        }

        [DllImport(user32_dll)]
        static extern Int16 GetKeyState(Int32 vKeyCode);
        public static bool IsKeyDown(Keys keyCode)
        {
            return GetKeyState((Int32)keyCode) < 0;
        }

        [DllImport(user32_dll)]
        static extern Int16 GetAsyncKeyState(Int32 vKeyCode);
        public static bool IsKeyDownAsync(Keys keyCode)
        {
            return GetAsyncKeyState((Int32)keyCode) < 0;
        }

        [DllImport("user32.dll")]
        public static extern bool MessageBeep(uint uType);

        public static void Beep()
        {
            MessageBeep(0xFFFFFFFF); // デフォルトの短いビープ音
        }
        #endregion

    }
}
