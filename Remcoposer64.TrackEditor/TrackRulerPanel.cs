using Remcoposer64.ProjectData;
using Remcoposer64.TrackEditor.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.TrackEditor
{
    public class TrackRulerPanel : HeaderPanelBase
    {
        public enum RulerSprite
        {
            Marker1 = 0,
            Marker2 = 1,

            Tick1 = 2,
            Tick2 = 3,
            Tick3 = 4,
            Tick4 = 5,
            Tick5 = 6,
            Tick6 = 7,

            Digit0 = 8,
            Digit1 = 9,
            Digit2 = 10,
            Digit3 = 11,
            Digit4 = 12,
            Digit5 = 13,
            Digit6 = 14,
            Digit7 = 15,
            Digit8 = 16,
            Digit9 = 17,
        }

        public TrackEditorControl trackEditorControl = null;
        public MIDIProject currentProject = null;

        protected override void Render()
        {
            graph.BackColor = Color.DarkSlateBlue;
            graph.FillRectangle(0, 0, Width, Height);
            if (trackEditorControl == null || trackEditorControl.trackCanvasPanel == null) return;
            if (currentProject == null) return;

            int startTick = (int)(trackEditorControl.pxScrollX / trackEditorControl.trackCanvasPanel.tickZoom);
            if (startTick < 0) startTick = 0;
            int endTick = startTick + (int)(Width / trackEditorControl.trackCanvasPanel.tickZoom) + 1;
            int lastX = int.MinValue;
            int ppq = currentProject.Information.TimeBase;
            if (ppq == 0) ppq = 480;
            int tick16 = ppq / 4;
            int tick8 = ppq / 2;
            int beatDen = currentProject.Information.BeatDen;
            int beatMol = currentProject.Information.BeatNum;
            int tickMeas = ppq * beatMol * 4 / beatDen;
            int tickBeat = ppq * 4 / beatDen;

            for (int tick = startTick - (startTick % tick16); tick <= endTick; tick += tick16)
            {
                int x = trackEditorControl.trackCanvasPanel.TickToX(tick);

                bool drawTick = x != lastX;
                if (drawTick)
                {
                    lastX = x;
                }

                if (tick % tickMeas == 0)
                {
                    if (drawTick)
                    {
                        // 小節
                        graph.DrawImage(0, (int)RulerSprite.Tick4, x, Height - 9);
                    }

                    string n = (tick / tickMeas + 1).ToString();
                    int width = n.Length * 4;
                    int drawX = x - width / 2 + 2;
                    for (int i = 0; i < n.Length; i++)
                    {
                        graph.DrawImage(0, (int)RulerSprite.Digit0 + (n[i] - '0'), drawX - 1 + i * 4, Height - 13);
                    }
                }

                if (!drawTick)
                {
                    continue;
                }

                if (tick % tickBeat == 0)
                {
                    // 拍
                    graph.DrawImage(0, (int)RulerSprite.Tick3, x, Height - 9);
                }
                else if (tick % tick8 == 0)
                {
                    graph.DrawImage(0, (int)RulerSprite.Tick2, x, Height - 9);
                }
                else
                {
                    graph.DrawImage(0, (int)RulerSprite.Tick1, x, Height - 9);
                }

                foreach (var marker in currentProject.Information.Markers)
                {
                    x = trackEditorControl.trackCanvasPanel.TickToX((int)marker.Tick);

                    if (x < -32 || x > Width + 32)
                        continue;

                    graph.DrawImage(0, (int)RulerSprite.Marker2, x - 2, 0);
                    Point pos = new Point(x + 4, 0);
                    graph.DrawStringOSFont(marker.Text, ref pos, Color.LightGreen, Color.Transparent);
                }

            }

            if (Project == Player.GetPlayingProject())
            {
                int markerPos = trackEditorControl.trackCanvasPanel.TickToX((int)Player.GetFrameCounter());

                graph.DrawImage(
                    0,
                    (int)RulerSprite.Marker1,
                    markerPos - 2,
                    Height - 13);
            }

            graph.DrawLine(0, Height - 1, Width, Height - 1, Color.White);
        }

        public void RefreshRuler()
        {
            this.Invalidate();
        }
    }
}