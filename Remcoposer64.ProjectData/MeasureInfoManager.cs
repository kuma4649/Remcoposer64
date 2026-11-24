using Remcoposer64.Common;
using Remcoposer64.ProjectData.Events;

namespace Remcoposer64.ProjectData
{
    public class MeasureInfoManager
    {
        long eventCount = 0;
        long nextFrame = long.MaxValue;

        private Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNode = new();
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
                //frameCounter++;
                frameCounter = nextFrame;
            }

            if (ClearPlayedFlag) Common.ClearPlayedFlag(project);
        }


        public bool ProcessFrame(long frameCounter)
        {
            int trkStatsIndex = 0;
            LinkedListNode<MIDITrack> nTrk = project.Track.First;
            bool playedAllTracks = true;

            nextFrame = long.MaxValue;

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

                    //nextFrame = Math.Min(nextFrame, ps.CurrentEventFrame + trk.St);

                    //if (frameCounter < ps.CurrentEventFrame + trk.St)
                    //    break;

                    while (ps.CurrentEvent != null)
                    {
                        long eventTick = ps.CurrentEventFrame + trk.St;

                        // まだ未来のイベント
                        if (frameCounter < eventTick)
                        {
                            nextFrame = Math.Min(nextFrame, eventTick);
                            break;
                        }

                        MIDIEvent evt = ps.CurrentEvent.Value;

                        //if (frameCounter < ps.CurrentEventFrame + trk.St)
                        //    break;

                        ps.CurrentEvent = AnalyzeEvent(frameCounter, trk, ts, nPrt, ps, ps.CurrentEvent);
                        ps.CurrentEventFrame += evt.ST;
                    }

                    // 次イベントの時刻を収集
                    if (ps.CurrentEvent != null)
                    {
                        nextFrame = Math.Min(
                            nextFrame,
                            ps.CurrentEventFrame + trk.St);
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

        private LinkedListNode<MIDIEvent> AnalyzeEvent(
    long frameCounter,
    MIDITrack trk,
    TrackStatus ts,
    LinkedListNode<MIDIPart> nPrt,
    PartStatus ps,
    LinkedListNode<MIDIEvent> nEvt)
        {
            MIDIPart prt = nPrt.Value;
            MIDIEvent evt = nEvt.Value;

            eventCount++;

            switch (evt.Type)
            {
                case MIDIEventType.TempoChange:
                    break;

                case MIDIEventType.MetaMarker:
                    {
                        var mevt = (MIDIMarkerEvent)evt;

                        project.Information.Markers.Add(new MarkerPoint()
                        {
                            Tick = frameCounter,
                            Text = mevt.Text
                        });
                        break;
                    }

                case MIDIEventType.MetaTimeSignature:
                    throw new NotSupportedException();

                case MIDIEventType.SameMeas:
                    {
                        if (ts.SameMeasStack.Count > 0)
                            return ts.SameMeasStack.Pop();

                        bool first = true;

                        while (evt.Type == MIDIEventType.SameMeas)
                        {
                            var sevt = (MIDISameMeasEvent)evt;
                            int target = sevt.RepeatMeas;

                            var key = (trk.Number, prt.Number, target);

                            if (!measureStartNode.TryGetValue(key, out var jumpNode))
                                break;

                            MIDIEvent jumpEvt = jumpNode.Value;

                            if (jumpEvt.Type == MIDIEventType.BarLine)
                                return nEvt.Next;

                            if (jumpEvt.Type == MIDIEventType.SameMeas)
                            {
                                if (nEvt == jumpNode)
                                {
                                    Log.Write(
                                        LogLevel.Error,
                                        $"MeasueInfoManager.cs: same measureEvent: Self-reference Case 4 : Part Name:{prt.Name} Event:{((MIDISameMeasEvent)nEvt.Value).RepeatMeas}");

                                    break;
                                }

                                if (first)
                                    ts.SameMeasStack.Push(nEvt.Next);

                                first = false;

                                nEvt = jumpNode;
                                evt = jumpEvt;
                                continue;
                            }

                            if (first)
                                ts.SameMeasStack.Push(nEvt.Next);

                            return jumpNode;
                        }

                        break;
                    }

                case MIDIEventType.BarLine:
                    if (ts.SameMeasStack.Count > 0)
                        return ts.SameMeasStack.Pop();
                    break;

                case MIDIEventType.MetaEndOfTrack:
                    if (ts.SameMeasStack.Count > 0)
                        return ts.SameMeasStack.Pop();
                    break;

                case MIDIEventType.LoopStart:
                    ts.LoopStack.Push((nEvt.Next, null));
                    break;

                case MIDIEventType.LoopEnd:
                    {
                        if (ts.LoopStack.Count > 0)
                        {
                            var (levt, lcnt) = ts.LoopStack.Pop();

                            var leEvt = (MIDILoopEndEvent)evt;

                            if (lcnt == null)
                                lcnt = leEvt.LoopCounter;

                            lcnt--;

                            if (lcnt > 0)
                            {
                                ts.LoopStack.Push((levt, lcnt));
                                return levt;
                            }
                        }
                        break;
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

        public void BuildPartInfo(MIDIPart prt)
        {
            Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNodePart = new();
            Common.BuildMeasureStartNode(measureStartNodePart, prt, 0, 0);
            TrackStatus ts = new TrackStatus();
            ts.ps.Add(new PartStatus());
            ProcessFrameFromPart(measureStartNodePart, prt, ts, ts.ps[0]);

            prt.LengthTick = ts.ps[0].CurrentEventFrame;
        }

        public void ProcessFrameFromPart(
            Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNodePart,
            MIDIPart prt,
            TrackStatus ts,
            PartStatus ps)
        {

            // 初期化（イベント開始前）
            if (ps.CurrentEventFrame == -1)
            {
                ps.CurrentEventFrame = 0;// prt.StartTick;
                ps.CurrentEvent = prt.Event.First;
            }

            if (ps.CurrentEvent == null)
            {
                return;
            }

            while (ps.CurrentEvent != null)
            {
                MIDIEvent evt = ps.CurrentEvent.Value;

                ps.CurrentEvent = AnalyzeEventFromPart(measureStartNodePart, ts, ps.CurrentEvent);
                ps.CurrentEventFrame += evt.ST;
            }

        }

        private LinkedListNode<MIDIEvent> AnalyzeEventFromPart(
            Dictionary<(int track, int part, int meas), LinkedListNode<MIDIEvent>> measureStartNodePart,
            TrackStatus ts,
            LinkedListNode<MIDIEvent> nEvt)
        {
            //MIDIPart prt = nPrt.Value;
            MIDIEvent evt = nEvt.Value;

            if (evt.Type == MIDIEventType.TempoChange)
            {

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

                    var key = (0, 0, target);

                    if (!measureStartNodePart.TryGetValue(key, out var jumpNode))
                        break;

                    if (jumpNode.Value.Type == MIDIEventType.BarLine)
                    {
                        return nEvt.Next;
                    }

                    if (jumpNode.Value.Type == MIDIEventType.SameMeas)
                    {
                        if (nEvt == jumpNode)
                        {
                            Log.Write(LogLevel.Error, $"MeasueInfoManager.cs: same measureEvent: Self-reference Case 4 : Event:{((MIDISameMeasEvent)nEvt.Value).RepeatMeas}");
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

        /// <summary>
        /// 指定したトラック中の指定したイベントが初めて演奏されるTick数を調べる
        /// </summary>
        public Dictionary<LinkedListNode<MIDIEvent>, int> GetTicksFormTrackEvent(
            MIDIProject project,
            LinkedListNode<MIDITrack> targetTrack,
            params LinkedListNode<MIDIEvent>[] events)
        {
            if (events == null || events.Length < 1) return null;

            Dictionary<LinkedListNode<MIDIEvent>, int> ret = new();

            // ダミーのステータス保持インスタンスを用意する(計測向け)
            trkStats.Clear();
            LinkedListNode<MIDITrack> nTrk = targetTrack;
            TrackStatus ts = new TrackStatus();
            for (int i = 0; i < nTrk.Value.Part.Count; i++) ts.ps.Add(new());
            trkStats.Add(ts);

            long frameCounter = 0;
            bool isFinished = false;

            this.project = project;

            Common.BuildMeasureStartNode(project, measureStartNode);

            while (!isFinished)
            {
                isFinished = ProcessFrame_GetTicks(frameCounter, nTrk, events,ref ret);
                //frameCounter++;
                frameCounter = nextFrame;
            }

            return ret;
        }

        private bool ProcessFrame_GetTicks(long frameCounter, LinkedListNode<MIDITrack> nTrk, LinkedListNode<MIDIEvent>[] events,ref Dictionary<LinkedListNode<MIDIEvent>, int> ret)
        {
            int eventsCount = 0;
            for(int i=0;i< events.Length; i++)
            {
                if (events[i] == null) continue;
                eventsCount++;
            }

            int trkStatsIndex = 0;

            nextFrame = long.MaxValue;

            TrackStatus ts = trkStats[trkStatsIndex];
            MIDITrack trk = nTrk.Value;
            if (ts.Played) return true;

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

                while (ps.CurrentEvent != null)
                {
                    long eventTick = ps.CurrentEventFrame + trk.St;

                    // ここでチェック
                    for (int i = 0; i < events.Length; i++)
                    {
                        if (ps.CurrentEvent == events[i] && !ret.ContainsKey(ps.CurrentEvent))
                        {
                            ret.Add(ps.CurrentEvent, (int)eventTick);
                            eventsCount--;
                            if (eventsCount == 0)
                            {
                                return true;
                            }
                        }
                    }

                    // まだ未来のイベント
                    if (frameCounter < eventTick)
                    {
                        nextFrame = Math.Min(nextFrame, eventTick);
                        break;
                    }

                    MIDIEvent evt = ps.CurrentEvent.Value;

                    ps.CurrentEvent = AnalyzeEvent(frameCounter, trk, ts, nPrt, ps, ps.CurrentEvent);
                    ps.CurrentEventFrame += evt.ST;

                }

                // 次イベントの時刻を収集
                if (ps.CurrentEvent != null)
                {
                    nextFrame = Math.Min(
                        nextFrame,
                        ps.CurrentEventFrame + trk.St);
                }

                nPrt = nPrt.Next;
                prtStatsIndex++;
            }

            if (playedAllParts) ts.Played = true;

            return false;
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
