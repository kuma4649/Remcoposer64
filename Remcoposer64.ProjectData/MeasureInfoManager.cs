using Remcoposer64.Common;
using Remcoposer64.ProjectData.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    public class MeasureInfoManager
    {
        //public SortedDictionary<long, TimeSignaturePoint> TimeSignatures { get; } = new();
        private Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode = new();
        //private int currentDen = 4;
        //private int currentNum = 4;
        private MIDIProject project = null;

        public void Build(MIDIProject project)
        {
            this.project = project;
            Common.ClearPlayedFlag(project);

            project.Information.Measures.Clear();
            project.Information.Markers.Clear();
            //TimeSignatures.Clear();
            long frameCounter = 0;
            bool isFinished = false;
            //currentNum = project.Information.BeatNum;
            //currentDen = project.Information.BeatDen;

            Common.BuildMeasureStartNode(project, measureStartNode);

            while (!isFinished)
            {
                isFinished = ProcessFrame(frameCounter);
                frameCounter++;
            }

            Common.ClearPlayedFlag(project);
        }

        public bool ProcessFrame(long frameCounter)
        {
            LinkedListNode<MIDITrack> nTrk = project.Track.First;
            bool playedAllTracks = true;
            while (nTrk != null)
            {
                MIDITrack trk = nTrk.Value;
                if (trk.Played)
                {
                    nTrk = nTrk.Next;
                    continue;
                }

                playedAllTracks = false;
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                bool playedAllParts = true;

                while (nPrt != null)
                {
                    MIDIPart prt = nPrt.Value;

                    // 初期化（イベント開始前）
                    if (prt.CurrentEventFrame == -1)
                    {
                        prt.CurrentEventFrame = prt.StartTick;
                        prt.CurrentEvent = prt.Event.First;
                    }

                    if (prt.CurrentEvent == null)
                    {
                        nPrt = nPrt.Next;
                        continue;
                    }

                    playedAllParts = false;

                    if (frameCounter < prt.CurrentEventFrame + trk.St)
                        break;

                    while (prt.CurrentEvent != null)
                    {
                        MIDIEvent evt = prt.CurrentEvent.Value;

                        if (frameCounter < prt.CurrentEventFrame + trk.St)
                            break;

                        prt.CurrentEvent = AnalyzeEvent(frameCounter, trk, nPrt, prt.CurrentEvent);
                        prt.CurrentEventFrame += evt.ST;
                    }

                    nPrt = nPrt.Next;
                }

                if (playedAllParts)
                    trk.Played = true;

                nTrk = nTrk.Next;
            }

            if (playedAllTracks)
            {
                project.Information.Played = true;
                return true;
            }

            return false;
        }

        private LinkedListNode<MIDIEvent> AnalyzeEvent(long frameCounter, MIDITrack trk, LinkedListNode<MIDIPart> nPrt, LinkedListNode<MIDIEvent> nEvt)
        {
            MIDIPart prt = nPrt.Value;
            MIDIEvent evt = nEvt.Value;

            if (evt.Type == MIDIEventType.TempoChange)
            {

            }
            else if (evt.Type == MIDIEventType.MetaMarker)
            {
                var mevt = (MIDIMarkerEvent)evt;

                project.Information.Markers.Add(new MarkerPoint()
                {
                    Tick = frameCounter,
                    Text = mevt.Text
                });
            }
            else if (evt.Type == MIDIEventType.MetaTimeSignature)
            {
                // rcp形式はBarLineに拍子情報が存在する筈なのでMetaTimeSignatureはない
                throw new NotSupportedException();
            }
            else if (evt.Type == MIDIEventType.SameMeas)
            {
                if (trk.SameMeasStack.Count > 0)
                {
                    return trk.SameMeasStack.Pop();
                }

                bool first = true;
                while (evt.Type == MIDIEventType.SameMeas)
                {
                    var sevt = (MIDISameMeasEvent)evt;
                    int target = sevt.RepeatMeas;

                    var key = (trk.Number, prt.Number, target);

                    if (!measureStartNode.TryGetValue(key, out var jumpNode))
                        break;

                    if (jumpNode.Value.Type == MIDIEventType.BarLine)
                    {
                        return nEvt.Next;

                    }

                    if (jumpNode.Value.Type == MIDIEventType.SameMeas)
                    {
                        if (nEvt == jumpNode)
                        {
                            Log.Write(LogLevel.Error, $"MeasueInfoManager.cs: same measureEvent: Self-reference Case 4 : Part Name:{nPrt.Value.Name} Event:{((MIDISameMeasEvent)nEvt.Value).RepeatMeas}");
                            break;
                        }

                        if (first) trk.SameMeasStack.Push(nEvt.Next);
                        first = false;

                        nEvt = jumpNode;
                        evt = jumpNode.Value;
                        continue;
                    }

                    // ★ 戻り位置をスタックに積む（元の位置の次イベント）
                    if (first) trk.SameMeasStack.Push(nEvt.Next);

                    return jumpNode;

                }
            }
            else if (evt.Type == MIDIEventType.BarLine)
            {
                if (trk.SameMeasStack.Count > 0)
                {
                    return trk.SameMeasStack.Pop();
                }
            }
            else if (evt.Type == MIDIEventType.MetaEndOfTrack)
            {
                if (trk.SameMeasStack.Count > 0)
                {
                    return trk.SameMeasStack.Pop();
                }
            }
            else if (evt.Type == MIDIEventType.LoopStart)
            {
                trk.LoopStack.Push((nEvt.Next, null));
            }
            else if (evt.Type == MIDIEventType.LoopEnd)
            {
                if (trk.LoopStack.Count > 0)
                {
                    var (levt, lcnt) = trk.LoopStack.Pop();
                    MIDILoopEndEvent leEvt = (MIDILoopEndEvent)evt;
                    if (lcnt == null)
                    {
                        lcnt = leEvt.LoopCounter;
                    }

                    lcnt--;

                    if (lcnt > 0)
                    {
                        trk.LoopStack.Push((levt, lcnt));
                        return levt;
                    }
                }
            }

            return nEvt.Next;
        }

        public MeasureInfo GetMeasure(int measureNo)
        {
            return null;
        }

        public MeasureInfo GetMeasureFromTick(long tick)
        {
            return null;
        }
    }

    public class MeasureInfo
    {
        public int MeasureNo;
        
        public long StartTick;
        public long EndTick;

        public int Numerator;
        public int Denominator;

        // この小節で発生
        public List<TempoPoint> TempoEvents=new();
        public List<ProgramPoint> ProgramEvents=new();

        // この小節開始時点の状態
        public Dictionary<ChannelKey, ChannelStatus> Snapshot = new();
    }

    public class TempoPoint
    {
        public long Tick;
        public int Tempo;
    }

    public class ProgramPoint
    {
        public long Tick;

        public int DeviceNo;
        public int Channel;

        public int ProgramNo;
    }

    public readonly struct ChannelKey
    {
        public readonly int DeviceNo;
        public readonly int Channel;

        public ChannelKey(int deviceNo, int channel)
        {
            DeviceNo = deviceNo;
            Channel = channel;
        }
    }

    public class ChannelStatus
    {
        public int BankMSB = 0;
        public int BankLSB = 0;
        public int Program = 0;

        public int Volume = 100;
        public int Pan = 64;
        public int Expression = 127;
    }

    public class TimeSignaturePoint
    {
        public long Tick;

        public int Numerator;
        public int Denominator;

        // どのトラックから見つかったか
        public MIDITrack Track;
    }
    
    public class MarkerPoint
    {
        public long Tick;
        public string Text;
    }

}
