using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.StepEditorPanel.EventArgs
{
    public class PlayMeasureOrPointEventArgs : System.EventArgs
    {
        public MIDITrack TargetTrack { get; }
        public LinkedListNode<MIDIEvent> StartEvent { get; }
        public LinkedListNode<MIDIEvent> EndEvent { get; }

        public PlayMeasureOrPointEventArgs(MIDITrack targetTrack, LinkedListNode<MIDIEvent> startEvent, LinkedListNode<MIDIEvent> endEvent)
        {
            TargetTrack = targetTrack;
            StartEvent = startEvent;
            EndEvent = endEvent;
        }
    }
}
