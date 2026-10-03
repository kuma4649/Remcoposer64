using Remcoposer64.ProjectData;
using Remcoposer64.StepEditorPanelControl;
using Remcoposer64.StepEditorPanelControl.EventArgs;
using Remcoposer64.SysExEditorPanel;
using Remcoposer64.UndoRedoManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.App.form
{
    public partial class frmStepEditor : Form
    {
        private MIDITrack trk;
        private MIDIPart prt;

        public frmStepEditor()
        {
            InitializeComponent();
        }

        public frmStepEditor(MIDITrack trk, MIDIPart prt, UndoRedoManager.UndoRedoManager mng)
        {
            InitializeComponent();
            
            this.trk = trk;
            this.prt = prt;

            stepEditorPanel1.SetPartData(prt);
            stepEditorPanel1.Init(mng);
            stepEditorPanel1.InputModeChanged += stepEditorPanel1_InputModeChanged;
            stepEditorPanel1.ExitStepEditor += stepEditorPanel1_ExitStepEditor;
            stepEditorPanel1.SysExEditRequested = ExecuteSysExEditor;
            stepEditorPanel1.PlayNoteRequested += StepEditorPanel1_PlayNoteRequested;

            UpdateFormTitle();
        }

        private void stepEditorPanel1_InputModeChanged(object sender, ModeChangedEventArgs e)
        {
            tsslIsOverwrite.Text = e.IsOverwrite ? "OVR" : "INS";
        }

        private void stepEditorPanel1_ExitStepEditor(object sender, EventArgs e)
        {
            this.Close();
        }

        private SysExEditResult ExecuteSysExEditor(SysExEditRequest request)
        {
            Form sysExEditor = new SysExEditorForm();
            if (sysExEditor.ShowDialog() == DialogResult.OK)
            {
                SysExEditResult result = new SysExEditResult
                {
                    RawData = ((SysExEditorForm)sysExEditor).EditedData,
                    Name = ((SysExEditorForm)sysExEditor).EditedName
                };
                return result;
            }
            else
            {
                return null;
            }
        }


        [DllImport("kernel32.dll")]
        public static extern bool Beep(int frequency, int duration);

        private void StepEditorPanel1_PlayNoteRequested(object sender, PlayNoteEventArgs e)
        {
            double freq = 440.0 * Math.Pow(2.0, (e.KeyNumber - 69) / 12.0);
            Beep((int)freq, 200);
        }

        private void frmStepEditor_Shown(object sender, EventArgs e)
        {
            stepEditorPanel1.RefreshScrollBar();
        }

        private void UpdateFormTitle()
        {
            string trkNm = trk != null ? trk.Name.Trim() : "(Unknown)";
            string prtNm = prt != null ? prt.Name.Trim() : "(Unknown)";
            string upd = stepEditorPanel1.UpdateFlg ? "*" : "";
            this.Text = $"Step Editor - ( {trkNm} : {prtNm} ){upd}";

            tsslIsOverwrite.Text = stepEditorPanel1.OverwriteMode ? "OVR" : "INS";
        }
    }
}
