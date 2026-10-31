using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Remcoposer64.App.form
{
    public partial class frmProject : DockContent
    {
        private frmMain parent;
        private MIDIProject project;

        public frmProject()
        {
            InitializeComponent();
        }
        public frmProject(frmMain parent,MIDIProject project)
        {
            InitializeComponent();
            this.parent = parent;
            this.project = project;
        }

        private void frmProject_FormClosed(object sender, FormClosedEventArgs e)
        {
            parent.CloseProject();
        }
    }
}
