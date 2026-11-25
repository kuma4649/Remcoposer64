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
            // Common(0 - 999)
            FileOpen = 0,
            FileSave = 10,
            Play = 20,
            Stop = 30,
            // StepEditor(1000 - 1999)
            InsCC = 1000,
            JumpPlay = 1010,
            MeasPlay = 1020,
            SpecialEvent = 1030,
            Cut = 1040,
            Copy = 1050,
            Paste = 1060,
            Undo = 1070,
            Redo = 1080,
            Escape = 1090,
            BackSpace = 1100,
            ModeChange = 1110,
            DeleteEvent = 1120,
            Enter = 1130,
            PageUp = 1140,
            PageDown = 1150,
            MoveTopMeas = 1160,
            MoveNextMeas = 1170,
            MoveFirstEvent = 1180,
            MoveLastEvent = 1190,
            Left = 1200,
            Right = 1210,
            Up = 1220,
            Down = 1230,
            SelectUp = 1240,
            SelectDown = 1250,
            KeepUp = 1260,
            KeepDown = 1270,
        };

        public ShortCutKeyInfo[] Info { get; set; } = [];
        public enmContent[] RemovedFunctions { get; set; } = [];

        private List<ShortCutKeyInfo> waitingShortcut = null;




        public ShortCutKey Copy()
        {
            ShortCutKey ret = new ShortCutKey();

            if (this.Info != null)
            {
                ret.Info = new ShortCutKeyInfo[this.Info.Length];
                int i = 0;
                foreach (ShortCutKeyInfo inf in this.Info)
                {
                    ret.Info[i++] = inf.Copy();
                }
            }
            if (this.RemovedFunctions != null)
            {
                ret.RemovedFunctions = new enmContent[this.RemovedFunctions.Length];
                int i = 0;
                foreach (enmContent content in this.RemovedFunctions)
                {
                    ret.RemovedFunctions[i++] = content;
                }
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
                sb.Append(info.ToString());
                sb.AppendLine();
            }

            return sb.ToString();
        }


        public List<ShortCutKeyInfo> Find1stStroke(enmCategory category, Keys keyData)
        {
            List<ShortCutKeyInfo> ret = [];
            DecodeKeyData(keyData, out Keys key, out bool shift, out bool ctrl, out bool alt);

            foreach (var info in Info)
            {
                if (info.category != category) continue;
                if (info.shift != shift) continue;
                if (info.ctrl != ctrl) continue;
                if (info.alt != alt) continue;
                if (info.key != key) continue;

                ret.Add(info);
            }

            return ret;
        }

        public ShortCutKeyInfo Find2ndStroke(List<ShortCutKeyInfo> candidates, Keys keyData)
        {
            DecodeKeyData(keyData, out Keys key, out bool shift, out bool ctrl, out bool alt);

            foreach (var info in candidates)
            {
                if (info.shift2 != shift) continue;
                if (info.ctrl2 != ctrl) continue;
                if (info.alt2 != alt) continue;
                if (info.key2 != key) continue;

                return info;
            }

            return null;

        }

        private static void DecodeKeyData(Keys keyData, out Keys key, out bool shift, out bool ctrl, out bool alt)
        {
            key = keyData & Keys.KeyCode;

            ctrl = (keyData & Keys.Control) == Keys.Control;
            shift = (keyData & Keys.Shift) == Keys.Shift;
            alt = (keyData & Keys.Alt) == Keys.Alt;
        }

        private DateTime waitingShortcutTime;

        public ShortCutKeyInfo ProcessKey(enmCategory category, Keys keyData)
        {
            if ((DateTime.Now - waitingShortcutTime)
                .TotalSeconds > 3)
            {
                waitingShortcut = null;
            }

            if (waitingShortcut != null)
            {
                var info =
                    Find2ndStroke(waitingShortcut, keyData);

                waitingShortcut = null;

                if (info != null)
                    return info;
            }

            var lst = Find1stStroke(category, keyData);

            if (lst.Count == 1
                && lst[0].key2 == Keys.None)
            {
                return lst[0];
            }

            if (lst.Count > 0
                && lst.Any(x => x.key2 != Keys.None))
            {
                waitingShortcut = lst;
                waitingShortcutTime = DateTime.Now;
            }

            return null;
        }


        //public ShortCutKeyInfo[] MergeDefaultShortcut(ShortCutKeyInfo[] defaultInfo, ShortCutKeyInfo[] srcInfo)
        //{
        //    // 先ずはリストへコピー
        //    List<ShortCutKeyInfo> lsrcInfo = srcInfo.ToList<ShortCutKeyInfo>();

        //    // 実施済み項目
        //    List<ShortCutKeyInfo> lpInfo = new();

        //    // defaultにはあってsrcには無いもの(新規追加された機能)を追加する。
        //    foreach (var dInf in defaultInfo)
        //    {
        //        // 処理済みの項目は飛ばす
        //        bool fnd = false;
        //        foreach (var pInf in lpInfo)
        //        {
        //            if (dInf.func == pInf.func) { fnd = true; break; }
        //        }
        //        if (fnd) continue;


        //        // 個数を数える
        //        int dCount = 0;
        //        foreach (var dInf2 in defaultInfo)
        //            if (dInf.func == dInf2.func) dCount++;

        //        // 個数を数える
        //        int sCount = 0;
        //        foreach (var sInf in srcInfo)
        //            if (dInf.func == sInf.func) sCount++;

        //        // 個数が同じ場合又はdefaultより多い場合は次を調べる
        //        if (dCount <= sCount) continue;

        //        // 存在しない或いは数が足りない機能はその分だけ追加する
        //        int i = dCount - sCount;
        //        foreach (var dInf3 in defaultInfo)
        //        {
        //            if (dInf.func == dInf3.func) lsrcInfo.Add(dInf3);
        //            i--;
        //            if (i <= 0) break;
        //        }

        //        lpInfo.Add(dInf);
        //    }


        //    // srcにはあってdefaultには無いもの(廃止された機能)を削除する。
        //    foreach (var sInf in srcInfo)
        //    {
        //        // defaultに存在する機能か調べる
        //        bool fnd = false;
        //        foreach (var dInf in defaultInfo)
        //        {
        //            if (dInf.func == sInf.func) { fnd = true; break; }
        //        }

        //        // 見つからない機能であった場合は削除する。
        //        if (!fnd)
        //        {
        //            if (lsrcInfo.Contains(sInf)) lsrcInfo.Remove(sInf);
        //        }
        //    }


        //    // 結果をセットする
        //    return lsrcInfo.ToArray();
        //}

        public ShortCutKeyInfo[] MergeDefaultShortcut(
            ShortCutKeyInfo[] defaultInfo,
            ShortCutKeyInfo[] srcInfo,
            enmContent[] removedFunctions)
        {
            List<ShortCutKeyInfo> result = srcInfo.ToList();

            // 廃止された機能を削除
            result.RemoveAll(s =>
                !defaultInfo.Any(d => d.func == s.func));

            HashSet<enmContent> processed = [];

            foreach (var dInf in defaultInfo)
            {
                // 同じ機能は一度だけ処理
                if (!processed.Add(dInf.func))
                    continue;

                // ユーザーが削除した機能は復活させない
                if (removedFunctions.Contains(dInf.func))
                    continue;

                // 既に存在する機能は何もしない
                if (result.Any(s => s.func == dInf.func))
                    continue;

                // その機能の定義を全部追加
                foreach (var addInf in defaultInfo)
                {
                    if (addInf.func == dInf.func)
                    {
                        result.Add(addInf.Copy());
                    }
                }
            }

            return result.ToArray();
        }

        public static (
                ShortCutKeyInfo[] SrcInfo, 
                enmContent[] RemovedFunctions
            )
            RemoveShortcut(
                ShortCutKeyInfo[] srcInfo,
                enmContent[] removedFunctions,
                ShortCutKeyInfo target)
        {
            List<ShortCutKeyInfo> lst = srcInfo.ToList();

            lst.Remove(target);

            bool exists =
                lst.Any(x => x.func == target.func);

            List<enmContent> removed =
                removedFunctions.ToList();

            if (!exists && !removed.Contains(target.func))
            {
                removed.Add(target.func);
            }

            return (
                lst.ToArray(),
                removed.ToArray()
            );
        }

        public static (
            ShortCutKeyInfo[] SrcInfo,
            enmContent[] RemovedFunctions
            )
            AddShortcut(
                ShortCutKeyInfo[] srcInfo,
                enmContent[] removedFunctions,
                ShortCutKeyInfo addInfo)
        {
            List<ShortCutKeyInfo> lst = srcInfo.ToList();

            lst.Add(addInfo);

            enmContent[] removed =
                removedFunctions
                    .Where(x => x != addInfo.func)
                    .ToArray();

            return (
                lst.ToArray(),
                removed
            );
        }

        private static bool IsSameShortcut(ShortCutKeyInfo a, ShortCutKeyInfo b)
        {
            return
                a.func == b.func
                && a.shift == b.shift
                && a.ctrl == b.ctrl
                && a.alt == b.alt
                && a.key == b.key
                && a.shift2 == b.shift2
                && a.ctrl2 == b.ctrl2
                && a.alt2 == b.alt2
                && a.key2 == b.key2;
        }
    }
}
