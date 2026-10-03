using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Model;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public abstract class UIEvent
    {
        //public delegate void DrawCellDelegate(
        //    string text,
        //    ref Point pos,
        //    ref int x,
        //    int col,
        //    FBColor fb,
        //    bool alignRight = true);

        public UIEvent(DrawUtil drawUtil)
        {
            this.drawUtil = drawUtil;
        }

        public abstract FBColor GetMeasStepColors(RowState state);

        public abstract void DrawEvent(MIDIEvent ev, MdlEvent me, Point pos, ref int x, CursorInfo cursor, RowState state);

        public static readonly int COL_MEAS = 0;
        public static readonly int COL_STEP = 1;
        public static readonly int COL_NOTENAME = 2;
        public static readonly int COL_KEYNUMBER = 3;
        public static readonly int COL_ST = 4;
        public static readonly int COL_GT = 5;
        public static readonly int COL_VEL = 6;
        protected DrawUtil drawUtil;
    }

    public class FBColor
    {
        public Color Fore;
        public Color Back;

        public FBColor(Color Fore,Color Back)
        {
            this.Fore = Fore;
            this.Back = Back;
        }
    }
}
