using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using static Remcoposer64.Common.ShortCutKey;

namespace Remcoposer64.Common
{
    [Serializable]
    public class ShortCutKeyInfo
    {
        public int number = -1;
        public enmCategory category = enmCategory.Common;
        public enmContent func = enmContent.None;
        public string displayName = "";
        public bool shift = false;
        public bool ctrl = false;
        public bool alt = false;
        public Keys key = Keys.None;

        public bool shift2 = false;
        public bool ctrl2 = false;
        public bool alt2 = false;
        public Keys key2 = Keys.None;

        [XmlIgnore]
        public int step = 0;

        public ShortCutKeyInfo()
        {
        }

        public ShortCutKeyInfo(int number, enmCategory category, enmContent func, string displayName
            , bool shift, bool ctrl, bool alt, Keys key
            , bool shift2 = false, bool ctrl2 = false, bool alt2 = false, Keys key2 = Keys.None)
        {
            this.number = number;
            this.category = category;
            this.func = func;
            this.displayName = displayName;

            this.shift = shift;
            this.ctrl = ctrl;
            this.alt = alt;
            this.key = key;

            this.shift2 = shift2;
            this.ctrl2 = ctrl2;
            this.alt2 = alt2;
            this.key2 = key2;
        }

        public ShortCutKeyInfo Copy()
        {
            ShortCutKeyInfo ret = new ShortCutKeyInfo(number, category, func, displayName, shift, ctrl, alt, key, shift2, ctrl2, alt2, key2);
            return ret;
        }

        public bool Match(bool shift, bool ctrl, bool alt, Keys key)
        {
            return this.shift == shift
                && this.ctrl == ctrl
                && this.alt == alt
                && this.key == key;
        }
        public bool Match2(bool shift, bool ctrl, bool alt, Keys key)
        {
            return this.shift2 == shift
                && this.ctrl2 == ctrl
                && this.alt2 == alt
                && this.key2 == key;
        }

        public override string ToString()
        {
            string s1 = GetOneStrokeText(
                shift,
                ctrl,
                alt,
                key);

            if (key2 == Keys.None)
                return s1;

            string s2 = GetOneStrokeText(
                shift2,
                ctrl2,
                alt2,
                key2);

            return $"{s1} , {s2}";
        }

        private static string GetOneStrokeText(
            bool shift,
            bool ctrl,
            bool alt,
            Keys key)
        {
            List<string> lst = [];

            if (ctrl) lst.Add("Ctrl");
            if (shift) lst.Add("Shift");
            if (alt) lst.Add("Alt");

            lst.Add(key.ToString());

            return string.Join(" + ", lst);
        }

    }

}
