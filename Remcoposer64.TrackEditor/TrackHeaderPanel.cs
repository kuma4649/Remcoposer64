using Remcoposer64.Common;
using Remcoposer64.Common.Render;
using Remcoposer64.Core.Player;
using Remcoposer64.ProjectData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
            Setting setting,
            RmPlayer player) => base.SetContext(trackEditorControl, project, setting, player);

        private const int COL_TRACKNO = 0;
        private const int COL_MUTE = 3;
        private const int COL_SOLO = 4;
        private const int COL_ST = 6;
        private const int COL_KEY = 11;
        private const int COL_TRACKNAME = 16;
        private const int COL_MIDIOUTCH = 6;
        private const int COL_MIDIOUT = 10;
        private const int COL_MIDIINCH = 6;
        private const int COL_MIDIIN = 10;

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

            Setting.midiOutInfo[] midiOutDevices = null;
            if (Setting?.midiOut?.lstMidiOutInfo != null
                && Setting.midiOut.lstMidiOutInfo.Count > Setting.midiOut.CurrentDev)
            {
                midiOutDevices =
                    Setting.midiOut.lstMidiOutInfo[
                        Setting.midiOut.CurrentDev];
            }

            Setting.midiInInfo[] midiInDevices = null;
            if (Setting?.midiIn?.lstMidiInInfo != null
                && Setting.midiIn.lstMidiInInfo.Count > Setting.midiIn.CurrentDev)
            {
                midiInDevices =
                    Setting.midiIn.lstMidiInInfo[
                        Setting.midiIn.CurrentDev];
            }

            Point pos = Point.Empty;

            graph.BackColor = backColor;
            graph.FillRectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height);
            Color mojiColor = Color.FromArgb(0, 150, 140, 250);

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
                pos.X = ColX(COL_TRACKNO);
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    $"{i + 1:D2} ",
                    ref pos,
                    mojiColor,
                    backColor);

                // Mute Button
                pos.X = ColX(COL_MUTE);
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    "M",
                    ref pos,
                    Color.DarkMagenta,
                    backColor);

                // Solo Button
                pos.X = ColX(COL_SOLO);
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    "S",
                    ref pos,
                    Color.Blue,
                    backColor);

                // ST+
                pos.X = ColX(COL_ST);
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    $"{trk.Value.St:+000;-000; 000}",
                    ref pos,
                    mojiColor,
                    backColor);

                // KEY+
                pos.X = ColX(COL_KEY);
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawString(
                    $"{trk.Value.Key:+000;-000; 000}",
                    ref pos,
                    mojiColor,
                    backColor);

                // Track Name
                pos.X = ColX(COL_TRACKNAME);
                pos.Y = pxTrackMarginTop + trackPanelPosY;
                graph.DrawStringOSFont(
                    trk.Value.Name,
                    ref pos,
                    mojiColor,
                    Color.Black
                    );



                if (TrackEditorControl.HeaderLine > 1)
                {
                    // MIDI Out Ch
                    pos.X = ColX(COL_MIDIOUTCH);
                    pos.Y = pxTrackMarginTop*2 + trackPanelPosY + pxFontHeight;
                    string num = trk.Value.OutChannel == null ? "Off" : $"{trk.Value.OutChannel + 1:D2}";
                    graph.DrawString(
                        num,
                        ref pos,
                        mojiColor,
                        backColor);

                    // MIDI Out
                    string name = "";
                    if (midiOutDevices != null && midiOutDevices.Length > 0)
                    {
                        int dev = Math.Clamp(trk.Value.OutDevice, 0, midiOutDevices.Length - 1);
                        name = midiOutDevices[dev]?.name ?? "";
                    }
                    pos.X = ColX(COL_MIDIOUT);
                    pos.Y = pxTrackMarginTop*2 + trackPanelPosY + pxFontHeight;
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        mojiColor,
                        Color.Black
                        );

                }

                if (TrackEditorControl.HeaderLine > 2)
                {
                    // MIDI In Ch
                    pos.X = ColX(COL_MIDIINCH);
                    pos.Y = pxTrackMarginTop * 3 + trackPanelPosY + pxFontHeight * 2;
                    string num = trk.Value.InChannel == null ? "Off" : $"{trk.Value.InChannel + 1:D2}";
                    graph.DrawString(
                        num,
                        ref pos,
                        mojiColor,
                        backColor);

                    // MIDI In
                    string name = "";
                    if (midiInDevices != null
                        && midiInDevices.Length > 0)
                    {
                        int dev = Math.Clamp(trk.Value.InDevice, 0, midiInDevices.Length - 1);
                        name = midiInDevices[dev]?.name ?? "";
                    }
                    pos.X = ColX(COL_MIDIIN);
                    pos.Y = pxTrackMarginTop * 3 + trackPanelPosY + pxFontHeight * 2;
                    graph.DrawStringOSFont(
                        name,
                        ref pos,
                        mojiColor,
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
                graph.BackColor = Color.FromArgb(0, 90, 80, 190);
                pos.X = 0;
                pos.Y = 0 + trackPanelPosY + pxTrackHeight - pxTrackBorderLineThin;
                graph.FillRectangle(pos.X, pos.Y, this.ClientSize.Width, pxTrackBorderLineThin);
                //pos.X = this.ClientSize.Width - 1 - pxTrackBorderLineThin;
                //pos.Y = 0 + trackPanelPosY;
                //graph.FillRectangle(pos.X, pos.Y, pxTrackBorderLineThin, pxFontHeight + pxTrackMarginTop + pxTrackMarginBottom - 1);
                //Debug.WriteLine($"Pos.Y:{pos.Y}");

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

        private int ColX(int col)
        {
            return pxTrackMarginLeft
                + pxFixedFontWidth * col
                - TrackEditorControl.pxHeaderScrollX;
        }

        public int GetMaxWidth()
        {
            int maxCol = COL_TRACKNAME;

            if (TrackEditorControl.HeaderLine > 1)
                maxCol = Math.Max(maxCol, COL_MIDIOUT);

            if (TrackEditorControl.HeaderLine > 2)
                maxCol = Math.Max(maxCol, COL_MIDIIN);

            return pxTrackMarginLeft
                 + pxFixedFontWidth * (maxCol + 40);
        }

    }
}
