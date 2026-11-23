using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Remcoposer64.Common
{
    [Serializable]
    public class ShortCutKey
    {
        private const string ESC_PIPE = "\u0001";
        public enum enmCategory
        {
            Common,
            StepEditor,
            TrackEditor,
            SysExEditor,
        }

        public enum enmContent : int
        {
            None = int.MinValue,

            FileOpen = 0,
            FileSave = 100,
            Play = 400,
            Stop = 800,

            InsCC = 1000,
            MeasPlay = 1010,
        };

        public ShortCutKeyInfo[] Info { get; set; } = [];



        public ShortCutKey Copy()
        {
            ShortCutKey ret = new ShortCutKey();

            if (this.Info == null)
                return ret;

            ret.Info = new ShortCutKeyInfo[this.Info.Length];
            int i = 0;
            foreach (ShortCutKeyInfo inf in this.Info)
            {
                ret.Info[i++] = inf.Copy();
            }

            return ret;
        }
        public bool Load(string fileName)
        {
            try
            {
                using FileStream fs =
                    File.OpenRead(fileName);

                Load(fs);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void LoadDefault()
        {
            Assembly asm = typeof(ShortCutKey).Assembly;

            using Stream? st =
                asm.GetManifestResourceStream(
                    "Remcoposer64.Common.ShortcutKeyDefault.txt");

            if (st == null)
                throw new Exception("Resource not found.");

            Load(st);
        }

        private void Load(Stream stream)
        {
            List<ShortCutKeyInfo> lst = [];
            using StreamReader sr = new StreamReader(stream);
            string txt = sr.ReadToEnd();
            string[] lines = txt.Replace("\r", "").Split('\n');

            enmCategory category = enmCategory.Common;
            int number = 0;

            foreach (var line in lines)
            {
                string ln = line.Trim();
                if (string.IsNullOrEmpty(ln)) continue;
                if (ln[0] == '#') continue;

                if (ln[0] == '!')
                {
                    if (ln.Length < 2) continue;
                    string lnc = ln.Substring(1).Trim();
                    if (!Enum.TryParse(lnc, out category))
                    {
                        Debug.WriteLine($"Unknown category : {lnc}");
                    }
                    continue;
                }

                ln = ln.Replace("||", ESC_PIPE);
                string[] parts = ln.Split('|');
                for (int i = 0; i < parts.Length; i++)
                {
                    parts[i] = parts[i].Replace(ESC_PIPE, "|");
                }
                if (parts.Length != 3)
                {
                    Debug.WriteLine($"Format Error : {ln}");
                    continue;
                }

                string func = parts[0].Trim();
                string displayName = parts[1].Trim();
                string shortcut = parts[2].Trim();

                ShortCutKeyInfo info = new()
                {
                    number = number++,
                    category = category,
                    displayName = displayName
                };
                if (Enum.TryParse(func, out enmContent content)) info.func = content;
                else
                {
                    Debug.WriteLine($"Unknown Function : {func}");
                    continue;
                }

                if(!ParseKeyStroke(shortcut,
                    out info.shift, out info.ctrl, out info.alt, out info.key,
                    out info.shift2, out info.ctrl2, out info.alt2, out info.key2
                    ))
                {
                    Debug.WriteLine($"Invalid Shortcut : {shortcut}");
                    continue;
                }

#if DEBUG
                Debug.WriteLine($"{info.category}:{info.func}:{info.displayName}");
                Debug.WriteLine($"  Shift={info.shift} Ctrl={info.ctrl} " + $"Alt={info.alt} " + $"Key={info.key}");
                Debug.WriteLine($"  Shift2={info.shift2} Ctrl2={info.ctrl2} " + $"Alt2={info.alt2} " + $"Key2={info.key2}");
#endif
                lst.Add(info);
            }

            Info = lst.ToArray();
        }

        private static bool ParseKeyStroke(string src
            , out bool shift, out bool ctrl, out bool alt, out Keys key
            , out bool shift2, out bool ctrl2, out bool alt2, out Keys key2
            )
        {
            shift = false;
            ctrl = false;
            alt = false;
            key = Keys.None;

            shift2 = false;
            ctrl2 = false;
            alt2 = false;
            key2 = Keys.None;

            src = src.Replace(" ", "");
            src = src.Replace("\t", "");
            string[] strokes = src.Split(',');

            if (strokes.Length < 1 || strokes.Length > 2)
            {
                Debug.WriteLine($"Invalid shortcut : {src}");
                return false;
            }

            if (!ParseOneStroke(strokes[0],
                out shift, out ctrl, out alt, out key))
                return false;

            if (strokes.Length > 1)
            {
                if (!ParseOneStroke(strokes[1],
                    out shift2, out ctrl2, out alt2, out key2))
                    return false;
            }

            return true;
        }

        private static bool ParseOneStroke(string src, out bool shift, out bool ctrl, out bool alt, out Keys key)
        {
            shift = false;
            ctrl = false;
            alt = false;
            key = Keys.None;

            string[] ele = src.Split('+');
            bool keyFound = false;

            foreach (string e in ele)
            {
                string s = e.Trim();

                if (s.Equals("Shift", StringComparison.OrdinalIgnoreCase))
                {
                    shift = true;
                    continue;
                }

                if (s.Equals("Ctrl", StringComparison.OrdinalIgnoreCase))
                {
                    ctrl = true;
                    continue;
                }

                if (s.Equals("Alt", StringComparison.OrdinalIgnoreCase))
                {
                    alt = true;
                    continue;
                }

                if (!Enum.TryParse(s, true, out Keys parsedKey))
                {
                    return false;
                }

                if (keyFound)
                {
                    return false;
                }

                key = parsedKey;
                keyFound = true;
            }

            return key != Keys.None;
        }

        public string GetText(ShortCutKeyInfo[] shortCutKeyInfos)
        {
            StringBuilder sb = new();

            enmCategory currentCategory = (enmCategory)(-1);

            foreach (ShortCutKeyInfo info in shortCutKeyInfos)
            {
                if (currentCategory != info.category)
                {
                    currentCategory = info.category;

                    if (sb.Length > 0)
                        sb.AppendLine();

                    sb.AppendLine($"!{currentCategory}");
                }

                string displayName = info.displayName.Replace("|", "||");

                sb.Append(info.func);
                sb.Append(" | ");
                sb.Append(displayName);
                sb.Append(" | ");
                sb.Append(GetShortcutText(info));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetShortcutText(ShortCutKeyInfo info)
        {
            string s1 = GetOneStrokeText(
                info.shift,
                info.ctrl,
                info.alt,
                info.key);

            if (info.key2 == Keys.None)
                return s1;

            string s2 = GetOneStrokeText(
                info.shift2,
                info.ctrl2,
                info.alt2,
                info.key2);

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
