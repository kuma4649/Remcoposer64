using Remcoposer64.Common;
using Remcoposer64.Core;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.Test
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            Log.Write(LogLevel.Information, "Test Track Editor");
            Setting setting = new Setting();
            ImportRcp iRcp = new ImportRcp("E:\\FM音源\\data\\MIDI\\人様のＭＩＤＩ\\ANIME\\NADOP88G.RCP", setting);
            MIDIProject project = iRcp.Load();

            trackEditorControl1.SetContext(project, setting);
        }
    }
}
