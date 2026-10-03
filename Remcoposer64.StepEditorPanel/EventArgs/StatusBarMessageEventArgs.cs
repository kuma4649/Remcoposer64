namespace Remcoposer64.StepEditorPanelControl.EventArgs
{
    public class StatusBarMessageEventArgs: System.EventArgs
    {
        private string v;

        public StatusBarMessageEventArgs(string v)
        {
            this.v = v;
        }
    }
}