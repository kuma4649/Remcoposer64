using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Remcoposer64.UndoRedoManager;

namespace Remcoposer64.App.form
{
    public partial class frmStepEditor : Form
    {
        private MIDIPart prt;

        public frmStepEditor()
        {
            InitializeComponent();
        }

        public frmStepEditor(MIDIPart prt, UndoRedoManager.UndoRedoManager mng)
        {
            InitializeComponent();
            this.prt = prt;
            stepEditorPanel1.SetPartData(prt);
            stepEditorPanel1.Init(mng);
        }

        private void frmStepEditor_Shown(object sender, EventArgs e)
        {
            stepEditorPanel1.RefreshScrollBar();
        }
    }
}
