using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor
{
    internal abstract class TrackPanelBase : Panel
    {
        protected IGraph graph;

        public MIDIProject Project { get; private set; } = null;
        public Setting Setting { get; private set; } = null;
        protected bool IsReady =>
    graph != null
    && Project != null
    && Setting != null;

        public virtual void SetContext(
            MIDIProject project,
            Setting setting)
        {
            Project = project;
            Setting = setting;

            CreateGraph();
            Invalidate();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            CreateGraph();
        }

        protected void CreateGraph()
        {
            if (!IsHandleCreated)
                return;

            if (Setting == null)
                return;

            graph?.Dispose();

            graph = Setting.common.GraphType
                == Setting.GraphType.GDI
                ? new GraphGDI(Handle)
                : new GraphVortice(Handle);
        }

        protected sealed override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (!IsReady)
                return;

            BeforeRender();

            Render();

            AfterRender();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (graph == null)
                return;
            graph?.Resize(
                ClientRectangle.Width, ClientRectangle.Height);
        }

        protected virtual void BeforeRender()
        {
            graph.BeginPaint(new System.Drawing.Rectangle(0,0,this.Width,this.Height));
        }

        protected virtual void AfterRender()
        {
            graph.EndPaint();
        }

        protected abstract void Render();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                graph?.Dispose();
                graph = null;
            }

            base.Dispose(disposing);
        }
    }
}