using Remcoposer64.ProjectData.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    public static class Common
    {
        public static void ClearPlayedFlag(MIDIProject _project)
        {
            if (_project == null || _project.Information == null) return;

            _project.Information.Played = false;
            LinkedListNode<MIDITrack> nTrk = _project.Track.First;
            while (nTrk != null)
            {
                nTrk.Value.Played = false;
                nTrk.Value.Memo = "";
                nTrk.Value.LoopStack = new Stack<(LinkedListNode<MIDIEvent>, int?)>();
                nTrk.Value.SameMeasStack = new Stack<LinkedListNode<MIDIEvent>>();
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                while (nPrt != null)
                {
                    //nPrt.Value.Played = false;
                    nPrt.Value.CurrentEvent = nPrt.Value.Event.First;
                    nPrt.Value.CurrentEventFrame = 0;
                    LinkedListNode<MIDIEvent> nEvt = nPrt.Value.Event.First;
                    while (nEvt != null)
                    {
                        nEvt = nEvt.Next;
                    }
                    nPrt = nPrt.Next;
                }
                nTrk = nTrk.Next;
            }
        }


        public static void BuildMeasureStartNode(MIDIProject _project, Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode)
        {
            measureStartNode.Clear();

            LinkedListNode<MIDITrack> nTrk = _project.Track.First;
            for (int t = 0; t < _project.Track.Count; t++)
            {
                var trk = nTrk;

                LinkedListNode<MIDIPart> nPrt = trk.Value.Part.First;
                for (int p = 0; p < trk.Value.Part.Count; p++)
                {
                    var prt = nPrt.Value;

                    ProjectData.Common.BuildMeasureStartNode(measureStartNode, prt, t, p);

                    nPrt = nPrt.Next;
                }

                nTrk = nTrk.Next;
            }
        }


        public static void BuildMeasureStartNode(
    Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode
    , MIDIPart prt, int t, int p)
        {

            var node = prt.Event.First;
            int meas = 0;

            measureStartNode[(t, p, meas)] = node;

            while (node != null)
            {
                var evt = node.Value;

                if (evt.Type == MIDIEventType.BarLine)
                {
                    if (node.Previous == null || node.Previous.Value.Type == MIDIEventType.SameMeas)
                    {
                        meas++;
                        measureStartNode[(t, p, meas)] = node;
                    }

                    meas++;
                    measureStartNode[(t, p, meas)] = node.Next;
                }
                else if (evt.Type == MIDIEventType.SameMeas)
                {
                    if (node.Previous == null || node.Previous.Value.Type != MIDIEventType.BarLine)
                    {
                        meas++;
                        measureStartNode[(t, p, meas)] = node;
                    }
                }
                else if (node.Previous != null && node.Previous.Value.Type == MIDIEventType.SameMeas)
                {
                    meas++;
                    measureStartNode[(t, p, meas)] = node;
                }

                node = node.Next;
            }


        }

    }
}
