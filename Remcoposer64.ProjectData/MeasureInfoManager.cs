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
        private List<TrackStatus> trkStats = new List<TrackStatus>();
        public class TrackStatus
        {
            public bool Played = false;
            public List<PartStatus> ps = new List<PartStatus>();
            public Stack<(LinkedListNode<MIDIEvent>, int?)> LoopStack { get; set; } = new Stack<(LinkedListNode<MIDIEvent>, int?)>();
            public Stack<LinkedListNode<MIDIEvent>> SameMeasStack { get; set; } = new Stack<LinkedListNode<MIDIEvent>>();

        }

        public class PartStatus
        {
            public long CurrentEventFrame { get; set; } = -1;
            public LinkedListNode<MIDIEvent> CurrentEvent { get; set; }
        }

        /// <summary>
        /// 注意:
        ///  演奏中にClearPlayedFlagをtrueにして本メソッドを実施すると演奏に影響が出る。
        ///  よって、trueを指定したいときは演奏を停止させておくこと
        /// </summary>
        /// <param name="project"></param>
        /// <param name="ClearPlayedFlag"></param>
        public void Build(MIDIProject project, bool ClearPlayedFlag = false)
        {
            this.project = project;
            if (ClearPlayedFlag) Common.ClearPlayedFlag(project);

            project.Information.Measures.Clear();
            project.Information.Markers.Clear();
            //TimeSignatures.Clear();

            // ダミーのステータス保持インスタンスを用意する(計測向け)
            trkStats.Clear();
            LinkedListNode<MIDITrack> nTrk = project.Track.First;
            while (nTrk != null)
            {
                TrackStatus ts = new TrackStatus();
                for (int i = 0; i < nTrk.Value.Part.Count; i++) ts.ps.Add(new());
                trkStats.Add(ts);
                nTrk = nTrk.Next;
            }

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

            if (ClearPlayedFlag) Common.ClearPlayedFlag(project);
        }

        public bool ProcessFrame(long frameCounter)
        {
            int trkStatsIndex = 0;
            LinkedListNode<MIDITrack> nTrk = project.Track.First;
            bool playedAllTracks = true;
            while (nTrk != null)
            {
                TrackStatus ts = trkStats[trkStatsIndex];
                MIDITrack trk = nTrk.Value;
                if (ts.Played)
                {
                    nTrk = nTrk.Next;
                    trkStatsIndex++;
                    continue;
                }

                playedAllTracks = false;
                int prtStatsIndex = 0;
                LinkedListNode<MIDIPart> nPrt = nTrk.Value.Part.First;
                bool playedAllParts = true;

                while (nPrt != null)
                {
                    MIDIPart prt = nPrt.Value;
                    PartStatus ps = ts.ps[prtStatsIndex];

                    // 初期化（イベント開始前）
                    if (ps.CurrentEventFrame == -1)
                    {
                        ps.CurrentEventFrame = prt.StartTick;
                        ps.CurrentEvent = prt.Event.First;
                    }

                    if (ps.CurrentEvent == null)
                    {
                        nPrt = nPrt.Next;
                        prtStatsIndex++;
                        continue;
                    }

                    playedAllParts = false;

                    if (frameCounter < ps.CurrentEventFrame + trk.St)
                        break;

                    while (ps.CurrentEvent != null)
                    {
                        MIDIEvent evt = ps.CurrentEvent.Value;

                        if (frameCounter < ps.CurrentEventFrame + trk.St)
                            break;

                        ps.CurrentEvent = AnalyzeEvent(frameCounter, trk, ts, nPrt, ps, ps.CurrentEvent);
                        ps.CurrentEventFrame += evt.ST;
                    }

                    nPrt = nPrt.Next;
                    prtStatsIndex++;
                }

                if (playedAllParts)
                    ts.Played = true;

                nTrk = nTrk.Next;
                trkStatsIndex++;
            }

            if (playedAllTracks)
            {
                //project.Information.Played = true;
                return true;
            }

            return false;
        }

        private LinkedListNode<MIDIEvent> AnalyzeEvent(long frameCounter, MIDITrack trk, TrackStatus ts, LinkedListNode<MIDIPart> nPrt, PartStatus ps, LinkedListNode<MIDIEvent> nEvt)
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
                if (ts.SameMeasStack.Count > 0)
                {
                    return ts.SameMeasStack.Pop();
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

                        if (first) ts.SameMeasStack.Push(nEvt.Next);
                        first = false;

                        nEvt = jumpNode;
                        evt = jumpNode.Value;
                        continue;
                    }

                    // ★ 戻り位置をスタックに積む（元の位置の次イベント）
                    if (first) ts.SameMeasStack.Push(nEvt.Next);

                    return jumpNode;

                }
            }
            else if (evt.Type == MIDIEventType.BarLine)
            {
                if (ts.SameMeasStack.Count > 0)
                {
                    return ts.SameMeasStack.Pop();
                }
            }
            else if (evt.Type == MIDIEventType.MetaEndOfTrack)
            {
                if (ts.SameMeasStack.Count > 0)
                {
                    return ts.SameMeasStack.Pop();
                }
            }
            else if (evt.Type == MIDIEventType.LoopStart)
            {
                ts.LoopStack.Push((nEvt.Next, null));
            }
            else if (evt.Type == MIDIEventType.LoopEnd)
            {
                if (ts.LoopStack.Count > 0)
                {
                    var (levt, lcnt) = ts.LoopStack.Pop();
                    MIDILoopEndEvent leEvt = (MIDILoopEndEvent)evt;
                    if (lcnt == null)
                    {
                        lcnt = leEvt.LoopCounter;
                    }

                    lcnt--;

                    if (lcnt > 0)
                    {
                        ts.LoopStack.Push((levt, lcnt));
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
