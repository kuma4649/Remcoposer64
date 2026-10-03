using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    internal class UIControlChangeEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.LightGreen, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);

        public static readonly Dictionary<int, string> CCNames = new()
{
    // MSB (0–31)
    { 0,  "Bank msb" },
    { 1,  "Modulat" },
    { 2,  "Breath" },
    { 4,  "Foot C." },
    { 5,  "Porta.TM" },
    { 6,  "Data msb" },
    { 7,  "Volume" },
    { 8,  "Balance" },
    { 10, "Panpot" },
    { 11, "Express" },
    { 12, "Effect1" },
    { 13, "Effect2" },
    { 16, "Genpur1" },
    { 17, "Genpur2" },
    { 18, "Genpur3" },
    { 19, "Genpur4" },

    // LSB (32–63)
    { 32, "Bank lsb" },
    { 33, "Mod lsb" },
    { 34, "Breath lsb" },
    { 36, "Foot lsb" },
    { 37, "Porta lsb" },
    { 38, "Data lsb" },
    { 39, "Vol lsb" },
    { 40, "Bal lsb" },
    { 42, "Pan lsb" },
    { 43, "Exp lsb" },
    { 44, "Eff1 lsb" },
    { 45, "Eff2 lsb" },
    { 48, "Gp1 lsb" },
    { 49, "Gp2 lsb" },
    { 50, "Gp3 lsb" },
    { 51, "Gp4 lsb" },

    // Switches (64–90)
    { 64, "Hold1" },
    { 65, "Portamen" },
    { 66, "Sostenut" },
    { 67, "Soft" },
    { 68, "Legato" },
    { 69, "Hold2" },
    { 70, "Sound1" },
    { 71, "Resonanc" },
    { 72, "Rel.Time" },
    { 73, "Att.Time" },
    { 74, "Cutoff F" },
    { 75, "Sound6" },
    { 76, "Sound7" },
    { 77, "Sound8" },
    { 78, "Sound9" },
    { 79, "Sound10" },
    { 84, "Por.Cont" },
    { 89, "VariSend" },
    { 90, "DryLevel" },

    // Effects (91–95)
    { 91, "Reverb" },
    { 92, "Tremolo" },
    { 93, "Chorus" },
    { 94, "Delay" },
    { 95, "Phaser" },

    // RPN / NRPN (96–101)
    { 96, "Data inc" },
    { 97, "Data dec" },
    { 98, "NRPN lsb" },
    { 99, "NRPN msb" },
    { 100, "RPN lsb" },
    { 101, "RPN msb" },

    // Mode messages (120–127)
    { 120, "All soff" },
    { 121, "ResetAll" },
    { 122, "Local C." },
    { 123, "Note off" },
    { 124, "Omni off" },
    { 125, "Omni on" },
    { 126, "Mono mod" },
    { 127, "Poly mod" },
};

        public UIControlChangeEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            MIDIControlChangeEvent cc = (MIDIControlChangeEvent)ev;
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse]
                         : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse]);

            string label = CCNames.TryGetValue(cc.ControllerNumber, out var name)
                ? name
                : "Control";
            
            if (cc.editing)
            {
                FBColor fbC = aryFb[1];
                string val = cc.editVal;
                if (string.IsNullOrWhiteSpace(cc.editVal))
                {
                    val = cc.virVal;
                    fbC = aryFb[2];
                }

                // Control
                drawUtil.DrawCell(label, ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // keynumber
                x += 4;

                // ST
                if (cursor.Position < 3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(cc.ST.ToString(), ref pos, ref x, COL_ST, aryFb[0]);

                // Controller number(GT)
                if (cursor.Position == 3)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_GT, fbC);
                else
                    drawUtil.DrawCell(cc.ControllerNumber.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

                // Controller value(Vel)
                if (cursor.Position == 4)
                    drawUtil.DrawCell(val, ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(cc.ControllerValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];

                // Control
                drawUtil.DrawCell(label, ref pos, ref x, COL_NOTENAME, aryFb[0]);

                // keynumber
                x += 4;

                // ST
                if (state == RowState.Cursor && cursor.Position < 3)
                    drawUtil.DrawCell(cc.ST.ToString(), ref pos, ref x, COL_ST, fbC);
                else
                    drawUtil.DrawCell(cc.ST != 0 ? cc.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

                // Controller number(GT)
                if (state == RowState.Cursor && cursor.Position == 3)
                    drawUtil.DrawCell(cc.ControllerNumber.ToString(), ref pos, ref x, COL_GT, fbC);
                else
                    drawUtil.DrawCell(cc.ControllerNumber.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

                if(state == RowState.Cursor && cursor.Position == 4)
                    drawUtil.DrawCell(cc.ControllerValue.ToString(), ref pos, ref x, COL_VEL, fbC);
                else
                    drawUtil.DrawCell(cc.ControllerValue.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
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
