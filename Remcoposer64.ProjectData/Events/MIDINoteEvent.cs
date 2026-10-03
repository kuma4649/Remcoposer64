using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData.Events
{
    public class MIDINoteEvent : MIDIEvent
    {
        private int _keyNumber;
        private string _noteName = "C 4";

        public int KeyNumber
        {
            get => _keyNumber;
            set
            {
                _keyNumber = Math.Clamp(value, 0, 127);
                _noteName = KeyNumberToNoteName(_keyNumber);
            }
        }

        public string NoteName
        {
            get => _noteName;
            set
            {
                _noteName = value;
                _keyNumber = NoteNameToKeyNumber(ref _noteName);
            }
        }
        public int backupKeyNumber;
        public static int oKeyNumber;
        public static string oNoteName;

        public int GT { get; set; }
        public int backupGT;
        public static int oGT;

        public int Vel { get; set; }
        public int backupVel;
        public static int oVel;

        public bool Tie { get; set; } = false;
        public int backupTie;
        public static int oTie;

        public override MIDIEvent Clone()
        {
            return new MIDINoteEvent()
            {
                Type = MIDIEventType.NoteON,

                Meas = this.Meas,
                Step = this.Step,
                ST = this.ST,

                KeyNumber = this.KeyNumber,
                NoteName = this.NoteName,
                GT = this.GT,
                Vel = this.Vel,
                Tie = this.Tie,
            };
        }

        private static readonly string[] NoteNames =
        {
            "C ", "C#", "D ", "D#", "E ", "F ",
            "F#", "G ", "G#", "A ", "A#", "B "
        };

        public static string KeyNumberToNoteName(int key)
        {
            int note = key % 12;
            int octave = (key / 12) - 1;
            return $"{NoteNames[note]}{octave}";
        }

        public static int NoteNameToKeyNumber(ref string name)
        {
            try
            {
                string s = name.ToUpper();
                if (s[1] == '+') s = s.Substring(0, 1) + '#' + s.Substring(2);
                if (s[1] == '-') s = s.Substring(0, 1) + 'b' + s.Substring(2);

                if (int.TryParse(s.Substring(2), out int octave) == false)
                {
                    octave = 4;// デフォルトは 4 オクターブ
                    s = s.Substring(0, 2) + octave.ToString();
                    name = s;
                }
                string notePart = s.Substring(0, 2);

                int noteIndex = ParseNoteName(notePart);
                if (noteIndex < -1)
                    throw new ArgumentException($"Invalid note name: {name}");

                return (octave + 1) * 12 + noteIndex;
            }
            catch
            {
                return -1;
            }
        }

        private static int ParseNoteName(string note)
        {
            // 特殊ケース：B# → 12
            if (note == "B#") return 12;
            // 特殊ケース：Cb → -1
            if (note == "Cb") return -1;
            // 特殊ケース：E# → F
            if (note == "E#") return Array.IndexOf(NoteNames, "F ");
            // 特殊ケース：Fb → E
            if (note == "Fb") return Array.IndexOf(NoteNames, "E ");

            // 通常の NoteNames から検索
            int idx = Array.IndexOf(NoteNames, note);
            if (idx >= 0) return idx;

            // フラット（b）対応：Db, Eb, Gb, Ab, Bb
            // 大文字化して "b" を "B" に統一
            string upper = note.ToUpper();

            if (upper.EndsWith("B"))
            {
                string natural = upper[..^1] + " ";   // "D" → "D "
                int naturalIndex = Array.IndexOf(NoteNames, natural);

                if (naturalIndex >= 0)
                    return (naturalIndex + 11) % 12;  // 半音下げる
            }

            // ここまで来たら不正
            return -2;
        }

        public override void CopyFrom(MIDIEvent other)
        {
            base.CopyFrom(other);

            var n = (MIDINoteEvent)other;

            // NoteEvent 固有フィールド
            this.KeyNumber = n.KeyNumber;
            this.NoteName = n.NoteName;
            this.GT = n.GT;
            this.Vel = n.Vel;
            this.Tie = n.Tie;

        }

    }
}
