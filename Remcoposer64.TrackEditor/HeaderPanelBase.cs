using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Remcoposer64.TrackEditor
{
    public abstract class HeaderPanelBase : Panel
    {
        protected IGraph graph;
        protected int pxFontHeight { get; private set; }

        public TrackEditorControl TrackEditorControl { get; private set; } = null;
        public MIDIProject Project { get; private set; } = null;
        public Setting Setting { get; private set; } = null;
        public RmPlayer Player { get; private set; } = null;

        protected bool IsReady =>
    graph != null
    && Project != null
    && Setting != null;

        protected HeaderPanelBase()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint,
                true);
        }


        public virtual void SetContext(
            TrackEditorControl trackEditorControl,
            MIDIProject project,
            Setting setting,
            RmPlayer player)
        {
            TrackEditorControl = trackEditorControl;
            Project = project;
            Setting = setting;
            Player = player;

            this.pxFontHeight = Setting.common.FontSize;

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

            graph.SetFontSize(pxFontHeight);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // 何もしない
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
            graph.BeginPaint(new System.Drawing.Rectangle(0, 0, this.Width, this.Height));
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