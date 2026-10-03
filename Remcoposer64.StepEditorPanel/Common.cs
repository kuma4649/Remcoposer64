using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanelControl
{
    public static class Common
    {
        public static bool IsBetween(this int value, int min, int max)
            => value >= min && value <= max;

        public static bool IsEmptyOrWhite(this string s)
            => string.IsNullOrWhiteSpace(s);

        public static bool IsNullOrEmpty(this string s)
            => string.IsNullOrEmpty(s);

        public static bool TryParseEither(string a, string b, out int value)
        {
            if (int.TryParse(a, out value)) return true;
            if (int.TryParse(b, out value)) return true;
            return false;
        }

        public static string SafeSubstring(this string s, int start, int length)
        {
            if (string.IsNullOrEmpty(s)) return "";
            if (start < 0) start = 0;
            if (start >= s.Length) return "";
            if (start + length > s.Length) length = s.Length - start;
            return s.Substring(start, length);
        }

    }

    public enum RowState
    {
        Normal,
        Cursor,
        Selected
    }

}
