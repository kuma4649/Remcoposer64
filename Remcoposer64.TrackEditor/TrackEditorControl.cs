using Remcoposer64.Common;
using Remcoposer64.ProjectData;
using System.Drawing;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor
{
    public partial class TrackEditorControl : UserControl
    {
        public MIDIProject Project { get; private set; } = null;
        public Setting Setting { get; private set; } = null;

        public TrackEditorControl()
        {
            InitializeComponent();

            BackColor = Color.FromArgb(40, 40, 40);
        }
        
        public void SetContext(MIDIProject project,Setting setting)
        {
            this.Project = project;
            this.Setting = setting;

            trackHeaderPanel.SetContext(project, setting);
            trackCanvasPanel.SetContext(project, setting);

            Invalidate();
        }
    }
}
