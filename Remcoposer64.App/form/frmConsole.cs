using Remcoposer64.Core;
using Remcoposer64.Common;

namespace Remcoposer64.App
{
    public partial class frmConsole : Form
    {
        private Setting setting;

        public void LogWrite(string message)
        {
            if (!this.IsHandleCreated || this.IsDisposed) return;
            this.BeginInvoke((Action<string>)lw, message);
        }

        private void lw(string message)
        {
            try
            {
                tbLog.SelectionStart = tbLog.Text.Length;
                tbLog.SelectionLength = 0;
                tbLog.SelectedText = string.Format("{0}\r\n", message);
                if (tbLog.Lines.Length > setting.log.MAXLogLine)
                {
                    tbLog.Text = string.Join("\r\n", tbLog.Lines.Skip(setting.log.SkipCount));
                }
                tbLog.SelectionStart = tbLog.Text.Length;
                tbLog.ScrollToCaret();
            }
            catch { }
        }

        public frmConsole(Setting setting)
        {
            this.setting = setting;
            InitializeComponent();
            Log.SetLogger(LogWrite);
        }

        private void tsmiTrace_Click(object sender, EventArgs e)
        {
            Log.logLevel = LogLevel.Trace;
            updateLoglevel();
        }

        private void tsmiDebug_Click(object sender, EventArgs e)
        {
            Log.logLevel = LogLevel.Debug;
            updateLoglevel();
        }

        private void tsmiError_Click(object sender, EventArgs e)
        {
            Log.logLevel = LogLevel.Error;
            updateLoglevel();
        }

        private void tsmiWarning_Click(object sender, EventArgs e)
        {
            Log.logLevel = LogLevel.Warning;
            updateLoglevel();
        }

        private void tsmiInformation_Click(object sender, EventArgs e)
        {
            Log.logLevel = LogLevel.Information;
            updateLoglevel();
        }

        private void tsmiClear_Click(object sender, EventArgs e)
        {
            tbLog.Clear();
        }

        private void frmConsole_Load(object sender, EventArgs e)
        {

        }

        private void frmConsole_Shown(object sender, EventArgs e)
        {
            updateLoglevel();
        }

        private void updateLoglevel()
        {
            tsmiTrace.Checked = Log.logLevel == LogLevel.Trace;
            tsmiDebug.Checked = Log.logLevel == LogLevel.Debug;
            tsmiError.Checked = Log.logLevel == LogLevel.Error;
            tsmiWarning.Checked = Log.logLevel == LogLevel.Warning;
            tsmiInformation.Checked = Log.logLevel == LogLevel.Information;
        }

        private void frmConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void frmConsole_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
    }
}
