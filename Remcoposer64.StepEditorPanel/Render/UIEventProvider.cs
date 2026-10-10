using Remcoposer64.StepEditorPanelControl.Render;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.StepEditorPanelControl.Render
{
    public class UIEventProvider
    {
        private readonly UINoteEvent noteEvent;
        private readonly UISysExEvent sysExEvent;
        private readonly UISameMeasEvent sameMeasEvent;
        private readonly UIBarLineEvent barLineEvent;
        private readonly UIEndEvent endEvent;
        private readonly UIMemoEvent memoEvent;
        private readonly UIProgramChangeEvent programChangeEvent;
        private readonly UIControlChangeEvent controlChangeEvent;
        private readonly UIPitchBendEvent pitchBendEvent;
        private readonly UIMetaEvent metaEvent;

        private readonly UITempoEvent tempoEvent;
        private readonly UIChannelPressureEvent channelPressureEvent;
        private readonly UIPolyphonicKeyPressureEvent polyphonicKeyPressureEvent;

        private readonly UIRolandDeviceEvent rolandDeviceEvent;
        private readonly UIRolandBaseEvent rolandBaseEvent;
        private readonly UIRolandParaEvent rolandParaEvent;


        public UIEventProvider(DrawUtil drawUtil)
        {
            noteEvent = new(drawUtil);
            sysExEvent = new(drawUtil);
            sameMeasEvent = new(drawUtil);
            barLineEvent = new(drawUtil);
            endEvent = new(drawUtil);
            memoEvent = new(drawUtil);
            programChangeEvent = new(drawUtil);
            controlChangeEvent = new(drawUtil);
            pitchBendEvent = new(drawUtil);
            metaEvent = new(drawUtil);
            tempoEvent = new(drawUtil);
            channelPressureEvent = new(drawUtil);
            polyphonicKeyPressureEvent = new(drawUtil);
            rolandDeviceEvent = new(drawUtil);
            rolandBaseEvent = new(drawUtil);
            rolandParaEvent = new(drawUtil);
        }

        public UIEvent Get(MIDIEventType type)
        {
            return type switch
            {
                MIDIEventType.NoteON => noteEvent,
                MIDIEventType.SysExF0 => sysExEvent,
                MIDIEventType.SameMeas => sameMeasEvent,
                MIDIEventType.BarLine => barLineEvent,
                MIDIEventType.MetaEndOfTrack => endEvent,
                MIDIEventType.Memo => memoEvent,
                MIDIEventType.ProgramChange => programChangeEvent,
                MIDIEventType.ControlChange => controlChangeEvent,
                MIDIEventType.PitchBend => pitchBendEvent,
                MIDIEventType.Meta => metaEvent,
                MIDIEventType.MetaTempo => tempoEvent,
                MIDIEventType.ChannelAfterTouch => channelPressureEvent,
                MIDIEventType.KeyAfterTouch => polyphonicKeyPressureEvent,

                MIDIEventType.NoteOff => noteEvent,
                MIDIEventType.MetaTimeSignature=>metaEvent,
                MIDIEventType.MetaKeySignature => metaEvent,
                MIDIEventType.MetaSMPTEOffset => metaEvent,
                MIDIEventType.MetaSequencerSpecific => metaEvent,
                MIDIEventType.MetaTextEvent => metaEvent,
                MIDIEventType.MetaCopyrightNotice => memoEvent,
                MIDIEventType.MetaTrackName => memoEvent,
                MIDIEventType.MetaInstrumentName => metaEvent,
                MIDIEventType.MetaLyric => metaEvent,
                MIDIEventType.MetaMarker => metaEvent,
                MIDIEventType.MetaCuePoint => metaEvent,
                MIDIEventType.MetaProgramName => memoEvent,
                MIDIEventType.MetaDeviceName => metaEvent,
                MIDIEventType.MetaChannelPrefix => metaEvent,
                MIDIEventType.MetaPortPrefix => metaEvent,
                MIDIEventType.MetaSeqNumber => metaEvent,

                MIDIEventType.RolandDevice => rolandDeviceEvent,
                MIDIEventType.RolandBase => rolandBaseEvent,
                MIDIEventType.RolandPara => rolandParaEvent,

                _ => throw new NotSupportedException()
            };
        }
    }
}
