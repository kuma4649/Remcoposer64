using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIProgramChangeEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.LightSalmon, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);

        public static readonly Dictionary<int, string> GMNames = new()
{
    // Piano
    { 0,  "Piano" },
    { 1,  "BrightPn" },
    { 2,  "E.Piano1" },
    { 3,  "E.Piano2" },
    { 4,  "Honky" },
    { 5,  "E.Org1" },
    { 6,  "E.Org2" },
    { 7,  "Harpsi" },

    // Chromatic Perc
    { 8,  "Clav" },
    { 9,  "Celesta" },
    { 10, "Glocken" },
    { 11, "MusicBox" },
    { 12, "Vibes" },
    { 13, "Marimba" },
    { 14, "Xylophon" },
    { 15, "Tubebell" },

    // Organ
    { 16, "Drawbar" },
    { 17, "PercOrg" },
    { 18, "RockOrg" },
    { 19, "Church" },
    { 20, "ReedOrg" },
    { 21, "Accordion" },
    { 22, "Harmonic" },
    { 23, "Bandone" },

    // Guitar
    { 24, "NylonGt" },
    { 25, "SteelGt" },
    { 26, "JazzGt" },
    { 27, "CleanGt" },
    { 28, "MutedGt" },
    { 29, "OverDrv" },
    { 30, "DistGt" },
    { 31, "GtHarm" },

    // Bass
    { 32, "AcBass" },
    { 33, "FngBass" },
    { 34, "PickBass" },
    { 35, "Fretless" },
    { 36, "Slap1" },
    { 37, "Slap2" },
    { 38, "SynBass1" },
    { 39, "SynBass2" },

    // Strings
    { 40, "Violin" },
    { 41, "Viola" },
    { 42, "Cello" },
    { 43, "Contra" },
    { 44, "TremStr" },
    { 45, "PizzStr" },
    { 46, "Harp" },
    { 47, "Timpani" },

    // Ensemble
    { 48, "StrEns1" },
    { 49, "StrEns2" },
    { 50, "SynStr1" },
    { 51, "SynStr2" },
    { 52, "ChoAahs" },
    { 53, "VoiceOo" },
    { 54, "SynVoice" },
    { 55, "OrchHit" },

    // Brass
    { 56, "Trumpet" },
    { 57, "Trombone" },
    { 58, "Tuba" },
    { 59, "MutedTp" },
    { 60, "FrHorn" },
    { 61, "BrsSect" },
    { 62, "SynBrs1" },
    { 63, "SynBrs2" },

    // Reed
    { 64, "Soprano" },
    { 65, "AltoSax" },
    { 66, "TenorSax" },
    { 67, "BariSax" },
    { 68, "Oboe" },
    { 69, "EngHorn" },
    { 70, "Bassoon" },
    { 71, "Clarinet" },

    // Pipe
    { 72, "Piccolo" },
    { 73, "Flute" },
    { 74, "Recorder" },
    { 75, "PanFlute" },
    { 76, "Bottle" },
    { 77, "Shakuh" },
    { 78, "Whistle" },
    { 79, "Ocarina" },

    // Synth Lead
    { 80, "Square" },
    { 81, "Saw" },
    { 82, "Calliope" },
    { 83, "Chiffer" },
    { 84, "Charang" },
    { 85, "SoloVox" },
    { 86, "5thLead" },
    { 87, "BassLead" },

    // Synth Pad
    { 88, "NewAge" },
    { 89, "WarmPad" },
    { 90, "PolySyn" },
    { 91, "ChoPad" },
    { 92, "BowedPad" },
    { 93, "MetalPad" },
    { 94, "HaloPad" },
    { 95, "SweepPad" },

    // FX
    { 96, "Rain" },
    { 97, "SoundTrk" },
    { 98, "Crystal" },
    { 99, "Atmosph" },
    { 100, "Bright" },
    { 101, "Goblins" },
    { 102, "Echoes" },
    { 103, "SciFi" },

    // World
    { 104, "Sitar" },
    { 105, "Banjo" },
    { 106, "Shamisen" },
    { 107, "Koto" },
    { 108, "Kalimba" },
    { 109, "Bagpipe" },
    { 110, "Fiddle" },
    { 111, "Shanai" },

    // Percussive
    { 112, "Tinkle" },
    { 113, "Agogo" },
    { 114, "SteelDrm" },
    { 115, "Woodblk" },
    { 116, "Taiko" },
    { 117, "Tom" },
    { 118, "SynthDrm" },
    { 119, "RevCym" },

    // SFX
    { 120, "GtrFret" },
    { 121, "Breath" },
    { 122, "Seashore" },
    { 123, "Bird" },
    { 124, "TelRing" },
    { 125, "Helicopt" },
    { 126, "Applause" },
    { 127, "Gunshot" },
};

        public UIProgramChangeEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDIProgramChangeEvent pc = (MIDIProgramChangeEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor 
                ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : (state == RowState.Normal 
                ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            if (pc.editing)
            {
                FBColor fbC = aryFb[1];
                string val = pc.editVal;
                if (string.IsNullOrWhiteSpace(pc.editVal))
                {
                    val = pc.virVal;
                    fbC = aryFb[2];
                }

                // Program
                int n = pc.ProgramNumber;
                if(cursor.Position > 2)
                    if (!int.TryParse(val, out n)) n = 0;
                string label = GMNames.TryGetValue(n, out var name)
                    ? name
                    : "Program";
                drawUtil.DrawCell(label, ref pos, ref x, 2, aryFb[0]);

                // ST
                x += 4;
                if (cursor.Position < 3)
                    drawUtil.DrawCell(val, ref pos, ref x, 4, fbC);
                else
                    drawUtil.DrawCell(pc.ST.ToString(), ref pos, ref x, 4, aryFb[0]);

                // Program number(GT)
                if (cursor.Position > 2)
                    drawUtil.DrawCell(val, ref pos, ref x, 5, fbC);
                else
                    drawUtil.DrawCell(pc.ProgramNumber.ToString(), ref pos, ref x, 5, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // Program
                string label = GMNames.TryGetValue(pc.ProgramNumber, out var name)
                    ? name
                    : "Program";
                drawUtil.DrawCell(label, ref pos, ref x, 2, aryFb[0]);

                // ST
                x += 4;
                if (state == RowState.Cursor && cursor.Position < 3)
                    drawUtil.DrawCell(pc.ST.ToString(), ref pos, ref x, 4, fbC);
                else
                    drawUtil.DrawCell(pc.ST != 0 ? pc.ST.ToString() : "    ", ref pos, ref x, 4, aryFb[0]);

                // Program number(GT)
                if (state == RowState.Cursor && cursor.Position > 2)
                    drawUtil.DrawCell(pc.ProgramNumber.ToString(), ref pos, ref x, 5, fbC);
                else
                    drawUtil.DrawCell(pc.ProgramNumber.ToString(), ref pos, ref x, 5, aryFb[0]);
            }
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                         : (state == RowState.Normal ? MeasStepNormal
                         : MeasStepSelect);
        }
    }
}
