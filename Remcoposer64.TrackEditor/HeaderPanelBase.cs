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

            graph?.LoadImage(0, "Resource\\rRuler.png", new Dictionary<int, Rectangle>()
                {
                    { 0, new Rectangle( 0, 0, 6, 4) }, // ルーラーの目印1
                    { 1, new Rectangle( 6, 0, 6, 4) }, // ルーラーの目印2
                    { 2, new Rectangle( 0, 4, 1, 4) }, // ルーラーの目盛り1
                    { 3, new Rectangle( 1, 4, 1, 4) }, // ルーラーの目盛り2
                    { 4, new Rectangle( 2, 4, 1, 4) }, // ルーラーの目盛り3
                    { 5, new Rectangle( 3, 4, 1, 4) }, // ルーラーの目盛り4
                    { 6, new Rectangle( 4, 4, 1, 4) }, // ルーラーの目盛り5
                    { 7, new Rectangle( 5, 4, 1, 4) }, // ルーラーの目盛り6
                    { 8, new Rectangle(12, 0, 4, 4) }, // ルーラーの数字0
                    { 9, new Rectangle(16, 0, 4, 4) }, // ルーラーの数字1
                    { 10, new Rectangle(20, 0, 4, 4) }, // ルーラーの数字2
                    { 11, new Rectangle(24, 0, 4, 4) }, // ルーラーの数字3
                    { 12, new Rectangle(28, 0, 4, 4) }, // ルーラーの数字4
                    { 13, new Rectangle(12, 4, 4, 4) }, // ルーラーの数字5
                    { 14, new Rectangle(16, 4, 4, 4) }, // ルーラーの数字6
                    { 15, new Rectangle(20, 4, 4, 4) }, // ルーラーの数字7
                    { 16, new Rectangle(24, 4, 4, 4) }, // ルーラーの数字8
                    { 17, new Rectangle(28, 4, 4, 4) }, // ルーラーの数字9
                });

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