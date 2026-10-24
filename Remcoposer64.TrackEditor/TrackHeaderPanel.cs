using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Remcoposer64.TrackEditor.Internal
{
    [ToolboxItem(false)]
    internal class TrackHeaderPanel : TrackPanelBase
    {

        //public int pxTrackHeight => TrackEditorControl.HeaderLine * (pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom);

        public Color backColor = Color.FromArgb(0, 20, 20, 60);

        public TrackHeaderPanel()
        {
        }

        public override void SetContext(
            TrackEditorControl trackEditorControl,
            MIDIProject project,
            Setting setting) => base.SetContext(trackEditorControl, project, setting);

        protected override void Render()
        {

            //int firstTrack = Math.Max(0, TrackEditorControl.pxScrollY / pxTrackHeight);
            //int visibleTracks = ClientSize.Height / pxTrackHeight + 2;
            //int lastTrack = Math.Min(Project.Track.Count, firstTrack + visibleTracks);

            LinkedListNode<MIDITrack> trk = Project.Track.First;
            for (int i = 0; i < TrackEditorControl.FirstVisibleTrack; i++)
            {
                if (trk == null) return;
                trk = trk.Next;
            }

            Point pos = Point.Empty;

            graph.BackColor = backColor;
            graph.FillRectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);

            for (int i = TrackEditorControl.FirstVisibleTrack; i < TrackEditorControl.LastVisibleTrack; i++)
            {
                if (trk == null)
                    break;

                int trackPanelPosY = i * pxTrackHeight - TrackEditorControl.pxScrollY;

                // Back 
                pos.X = 0;
                pos.Y = 0 + trackPanelPosY;
                graph.BackColor = backColor;
                graph.FillRectangle(
                    pos.X,
                    pos.Y,
                    this.ClientSize.Width,
                    pxTrackHeight
                    );


                // Track Number
                pos.X = pxTrackMarginLeft;
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    $"{i + 1:D2} ",
                    ref pos,
                    Color.WhiteSmoke,
                    backColor);

                // Mute Button
                pos.X = pxTrackMarginLeft + pxFixedFontWidth * 3;
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    "M",
                    ref pos,
                    Color.DarkMagenta,
                    backColor);

                // Solo Button
                pos.X = pxTrackMarginLeft + pxFixedFontWidth * 4;
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    "S",
                    ref pos,
                    Color.Blue,
                    backColor);

                // ST+
                pos.X = pxTrackMarginLeft + pxFixedFontWidth * 6;
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    $"{trk.Value.St:D3}",
                    ref pos,
                    Color.WhiteSmoke,
                    backColor);

                // KEY+
                pos.X = pxTrackMarginLeft + pxFixedFontWidth * 10;
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    $"{trk.Value.Key:D3}",
                    ref pos,
                    Color.WhiteSmoke,
                    backColor);

                // Track Name
                pos.X = pxTrackMarginLeft + pxFixedFontWidth * 14;
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawStringOSFont(
                    $"{trk.Value.Name}",
                    ref pos,
                    Color.WhiteSmoke,
                    Color.Black
                    );



                if (TrackEditorControl.HeaderLine > 1)
                {
                    // MIDI Out Ch
                    pos.X = pxTrackMarginLeft + pxFixedFontWidth * 6;
                    pos.Y = pxTrackMarginTop*2 + trackPanelPosY + pxFontHeight;
                    graph.DrawString(
                        $"{trk.Value.OutChannel + 1:D2}",
                        ref pos,
                        Color.WhiteSmoke,
                        backColor);

                    // MIDI Out
                    Setting.MidiOut mo = Setting.midiOut;
                    string name = "";
                    if (mo != null
                        && mo.lstMidiOutInfo != null
                        && mo.lstMidiOutInfo.Count > 0
                        && mo.lstMidiOutInfo.Count > mo.CurrentDev
                        && mo.lstMidiOutInfo[mo.CurrentDev] != null
                    )
                        name = mo.lstMidiOutInfo[mo.CurrentDev][Math.Max(Math.Min(trk.Value.OutDevice, mo.lstMidiOutInfo[mo.CurrentDev].Length - 1), 0)].name;

                    pos.X = pxTrackMarginLeft + pxFixedFontWidth * 9;
                    pos.Y = pxTrackMarginTop*2 + trackPanelPosY + pxFontHeight;
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        Color.WhiteSmoke,
                        Color.Black
                        );

                }

                if (TrackEditorControl.HeaderLine > 2)
                {
                    // MIDI In Ch
                    pos.X = pxTrackMarginLeft + pxFixedFontWidth * 6;
                    pos.Y = pxTrackMarginTop*3 + trackPanelPosY + pxFontHeight * 2;
                    graph.DrawString(
                        $"{trk.Value.InChannel + 1:D2}",
                        ref pos,
                        Color.WhiteSmoke,
                        backColor);

                    // MIDI In
                    Setting.MidiIn mi = Setting.midiIn;
                    string name = "";
                    if (mi != null
                        && mi.lstMidiInInfo != null
                        && mi.lstMidiInInfo.Count > 0
                        && mi.lstMidiInInfo.Count > mi.CurrentDev
                        && mi.lstMidiInInfo[mi.CurrentDev] != null
                    )
                        name = mi.lstMidiInInfo[mi.CurrentDev][Math.Max(Math.Min(trk.Value.InDevice, mi.lstMidiInInfo[mi.CurrentDev].Length - 1), 0)].name;
                    pos.X = pxTrackMarginLeft + pxFixedFontWidth * 9;
                    pos.Y = pxTrackMarginTop*3 + trackPanelPosY + pxFontHeight * 2;
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        Color.WhiteSmoke,
                        Color.Black
                        );
                }

                // ベゼル
                //graph.BackColor = Color.White;
                //pos.X = 0;
                //pos.Y = 0 + trackPanelPosY;
                //graph.FillRectangle(pos.X, pos.Y, this.ClientSize.Width, pxTrackBorderLineThin);
                //pos.X = 0;
                //pos.Y = 0 + trackPanelPosY;
                //graph.FillRectangle(pos.X, pos.Y, pxTrackBorderLineThin, pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom - 1);
                graph.BackColor = Color.Gray;
                pos.X = 0;
                pos.Y = 0 + trackPanelPosY + pxTrackHeight - pxTrackBorderLineThin;
                graph.FillRectangle(pos.X, pos.Y, this.ClientSize.Width, pxTrackBorderLineThin);
                //pos.X = this.ClientSize.Width - 1 - pxTrackBorderLineThin;
                //pos.Y = 0 + trackPanelPosY;
                //graph.FillRectangle(pos.X, pos.Y, pxTrackBorderLineThin, pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom - 1);


                trk = trk.Next;

            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            int trackNo =
                (e.Y + TrackEditorControl.pxScrollY)
                / pxTrackHeight;

            if (trackNo < 0
                || trackNo >= Project.Track.Count)
                return;

            LinkedListNode<MIDITrack> trk =
                Project.Track.First;

            for (int i = 0; i < trackNo; i++)
            {
                if (trk == null)
                    return;

                trk = trk.Next;
            }

            TrackEditorControl.RaiseTrackDoubleClicked(trk);
        }

    }
}
