using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using Remcoposer64.StepEditorPanelControl.Render;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class UINoteEvent : UIEvent
    {
        private static readonly FBColor MeasStepCursor = new(Color.Black, Color.White);
        private static readonly FBColor MeasStepNormal = new(Color.White, Color.Black);
        private static readonly FBColor MeasStepSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursor = new(Color.Black, Color.White);
        private static readonly FBColor EventNormal = new(Color.Cyan, Color.Black);
        private static readonly FBColor EventSelect = new(Color.Black, Color.Gray);
        private static readonly FBColor EventCursorEdit = new(Color.Yellow, Color.Black);
        private static readonly FBColor EventCursorDefault = new(Color.Gray, Color.Black);
        private static readonly FBColor EventCursorReverse = new(Color.White, Color.Black);
        private static readonly FBColor EventNormalTie = new(Color.LightYellow, Color.Black);

        public UINoteEvent(DrawUtil drawUtil) : base(drawUtil)
        {
        }

        public override FBColor GetMeasStepColors(RowState state)
        {
            return state == RowState.Cursor ? MeasStepCursor
                : (state == RowState.Normal ? MeasStepNormal
                : MeasStepSelect);
        }

        public override void DrawEvent(MIDIEvent ev1, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state)
        {
            var note = (MIDINoteEvent)ev1;
            FBColor[] aryFb = state == RowState.Cursor ? [EventCursor, EventCursorEdit, EventCursorDefault, EventCursorReverse, EventCursor]
                         : (state == RowState.Normal ? [EventNormal, EventCursorEdit, EventCursorDefault, EventCursorReverse, EventNormalTie]
                         : [EventSelect, EventCursorEdit, EventCursorDefault, EventCursorReverse, EventSelect]);


            if (note.editing)
            {
                FBColor fbC = aryFb[1];
                string val = note.editVal;
                if (string.IsNullOrWhiteSpace(note.editVal))
                {
                    val = note.virVal;
                    fbC = aryFb[2];
                }

                if (cursor.Position == 0) drawUtil.DrawCell(val, ref pos, ref x, COL_NOTENAME, fbC);
                else
                {
                    if (cursor.Position == 1)
                    {
                        string nn = note.editVal;
                        if (int.TryParse(nn, out int keyNumber)) nn = NoteEvent.KeyNumberToNoteName(keyNumber);
                        drawUtil.DrawCell(nn, ref pos, ref x, COL_NOTENAME, aryFb[0]);
                    }
                    else
                    {
                        drawUtil.DrawCell(note.NoteName, ref pos, ref x, COL_NOTENAME, aryFb[0]);
                    }
                }

                if (cursor.Position == 1) drawUtil.DrawCell(val, ref pos, ref x, COL_KEYNUMBER, fbC);
                else drawUtil.DrawCell(note.KeyNumber.ToString(), ref pos, ref x, COL_KEYNUMBER, aryFb[0]);

                if (cursor.Position == 2) drawUtil.DrawCell(val, ref pos, ref x, COL_ST, fbC);
                else drawUtil.DrawCell(note.ST.ToString(), ref pos, ref x, COL_ST, aryFb[0]);

                if (cursor.Position == 3) drawUtil.DrawCell(val, ref pos, ref x, COL_GT, fbC);
                else drawUtil.DrawCell(note.GT.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

                if (cursor.Position == 4) drawUtil.DrawCell(val, ref pos, ref x, COL_VEL, fbC);
                else drawUtil.DrawCell(note.Vel.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);
            }
            else
            {
                FBColor fbC = aryFb[3];
                Color fgTie = Color.LightYellow;

                if (state == RowState.Cursor && cursor.Position == 0) drawUtil.DrawCell(note.NoteName, ref pos, ref x, COL_NOTENAME, fbC);
                else drawUtil.DrawCell(note.NoteName, ref pos, ref x, COL_NOTENAME, aryFb[0]);

                if (state == RowState.Cursor && cursor.Position == 1) drawUtil.DrawCell(note.KeyNumber.ToString(), ref pos, ref x, COL_KEYNUMBER, fbC);
                else drawUtil.DrawCell(note.KeyNumber.ToString(), ref pos, ref x, COL_KEYNUMBER, aryFb[0]);

                if (state == RowState.Cursor && cursor.Position == 2) drawUtil.DrawCell(note.ST.ToString(), ref pos, ref x, COL_ST, fbC);
                else drawUtil.DrawCell(note.ST != 0 ? note.ST.ToString() : "    ", ref pos, ref x, COL_ST, aryFb[0]);

                if (state == RowState.Cursor && cursor.Position == 3) drawUtil.DrawCell(note.GT.ToString(), ref pos, ref x, COL_GT, fbC);
                else drawUtil.DrawCell(note.GT.ToString(), ref pos, ref x, COL_GT, aryFb[0]);

                if (state == RowState.Cursor && cursor.Position == 4) drawUtil.DrawCell(note.Vel.ToString(), ref pos, ref x, COL_VEL, fbC);
                else drawUtil.DrawCell(note.Vel.ToString(), ref pos, ref x, COL_VEL, aryFb[0]);

                // Tie の表示（VEL の右に＊）
                if (note.Tie)
                {
                    // ＊を描く位置は、VEL のセルのすぐ右
                    Point tiePos = new(pos.X + x * drawUtil.charWidth + drawUtil.charWidth, pos.Y);
                    if (state == RowState.Cursor) drawUtil.DrawString("*", ref tiePos, aryFb[0].Fore, aryFb[0].Back);
                    else drawUtil.DrawString("*", ref tiePos, aryFb[4].Fore, aryFb[4].Back);
                }
            }
        }
    }
}
