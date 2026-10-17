using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{

    public class MidiKeySignature
    {
        // MIDIの (sf, mi) をキーとし、調の文字列表現を値とするテーブル
        private static readonly Dictionary<(sbyte sf, byte mi), string> KeyTable = new()
    {
        // --- メジャー（長調）: mi = 0 ---
        { (7, 0), "C# Major" },
        { (6, 0), "F# Major" },
        { (5, 0), "B Major" },
        { (4, 0), "E Major" },
        { (3, 0), "A Major" },
        { (2, 0), "D Major" },
        { (1, 0), "G Major" },
        { (0, 0), "C Major" },
        { (-1, 0), "F Major" },
        { (-2, 0), "Bb Major" },
        { (-3, 0), "Eb Major" },
        { (-4, 0), "Ab Major" },
        { (-5, 0), "Db Major" },
        { (-6, 0), "Gb Major" },
        { (-7, 0), "Cb Major" },

        // --- マイナー（短調）: mi = 1 ---
        { (7, 1), "A# Minor" },
        { (6, 1), "D# Minor" },
        { (5, 1), "G# Minor" },
        { (4, 1), "C# Minor" },
        { (3, 1), "F# Minor" },
        { (2, 1), "B Minor" },
        { (1, 1), "E Minor" },
        { (0, 1), "A Minor" },
        { (-1, 1), "D Minor" },
        { (-2, 1), "G Minor" },
        { (-3, 1), "C Minor" },
        { (-4, 1), "F Minor" },
        { (-5, 1), "Bb Minor" },
        { (-6, 1), "Eb Minor" },
        { (-7, 1), "Ab Minor" }
    };

        /// <summary>
        /// MIDIのデータから調（キー）の名前を取得します。
        /// </summary>
        /// <param name="sf">シャープ/フラットの数（マイナスはフラット）</param>
        /// <param name="mi">0: メジャー, 1: マイナー</param>
        public static string GetKeyName(sbyte sf, byte mi)
        {
            if (KeyTable.TryGetValue((sf, mi), out var keyName))
            {
                return keyName;
            }
            return "Unknown Key";
        }

        public static void Main()
        {
            // テスト例1: sf = -1 (フラット1つ), mi = 0 (メジャー) -> F Major
            Console.WriteLine(GetKeyName(-1, 0));

            // テスト例2: sf = 3 (シャープ3つ), mi = 1 (マイナー) -> F# Minor
            Console.WriteLine(GetKeyName(3, 1));
        }
    }
}
