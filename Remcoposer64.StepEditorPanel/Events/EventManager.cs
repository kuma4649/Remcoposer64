using Remcoposer64.ProjectData;
using Remcoposer64.ProjectData.Events;
using Remcoposer64.StepEditorPanelControl.Events;
using Remcoposer64.StepEditorPanelControl.Model;
using System.Text.Json;

namespace Remcoposer64.StepEditorPanelControl
{
    public class EventManager
    {
        public MdlControlChangeEvent mdlControlChangeEvent;
        public MdlProgramChangeEvent mdlProgramChangeEvent;
        public MdlMetaEvent mdlMetaEvent;
        public MdlTempoEvent mdlTempoEvent;
        public MdlMemoEvent mdlMemoEvent;
        public MdlPitchBendEvent mdlPitchBendEvent;
        public MdlChannelPressureEvent mdlChannelPressureEvent;
        public MdlPolyphonicKeyPressureEvent mdlPolyphonicKeyPressureEvent;
        public MdlNoteEvent mdlNoteEvent;
        public MdlSysExEvent mdlSysExEvent;
        public MdlSameMeasEvent mdlSameMeasEvent;
        public MdlBarLineEvent mdlBarLineEvent;
        public MdlEndEvent mdlEndEvent;

        private LinkedList<MIDIEvent> events = new();

        // 高速インデックスアクセス用キャッシュ
        private List<LinkedListNode<MIDIEvent>> indexCache = new();

        // SameMeasEvent管理用
        private class SameMeasInfo
        {
            public int Index;                       // indexCache 内の位置
            public LinkedListNode<MIDIEvent> Node;      // LinkedList 内の位置
            public MIDISameMeasEvent Event => (MIDISameMeasEvent)Node.Value;
        }
        private List<SameMeasInfo> sameMeasList = new();

        // BarLineEvent管理用
        private class BarLineInfo
        {
            public int Index;                       // indexCache 内の位置
            public LinkedListNode<MIDIEvent> Node;      // LinkedList 内の位置
            public MIDIEvent Event => Node.Value;
        }

        /// <summary>
        /// barLineListにはSameMeasイベントも含みます
        /// </summary>
        private List<BarLineInfo> barLineList = new();

        public LinkedListNode<MIDIEvent> FirstEvent { get { return events?.First; } }
        public int EventCount => indexCache.Count;
        public StepEditorPanel parent;

        // タイ判別向けワーク
        private MIDITieCache[] tieWork = new MIDITieCache[128];
        private MIDIPart prt;

        public EventManager(StepEditorPanel parent)
        {
            mdlControlChangeEvent = new MdlControlChangeEvent(this);
            mdlProgramChangeEvent = new MdlProgramChangeEvent(this);
            mdlMetaEvent = new MdlMetaEvent(this);
            mdlTempoEvent = new MdlTempoEvent(this);
            mdlMemoEvent = new MdlMemoEvent(this);
            mdlPitchBendEvent = new MdlPitchBendEvent(this);
            mdlChannelPressureEvent = new MdlChannelPressureEvent(this);
            mdlPolyphonicKeyPressureEvent = new MdlPolyphonicKeyPressureEvent(this);
            mdlNoteEvent = new MdlNoteEvent(this);
            mdlSysExEvent = new MdlSysExEvent(this);
            mdlSameMeasEvent = new MdlSameMeasEvent(this);
            mdlBarLineEvent = new MdlBarLineEvent(this);
            mdlEndEvent = new MdlEndEvent(this);

            this.parent = parent;
            for (int i = 0; i < tieWork.Length; i++) tieWork[i] = new MIDITieCache();
        }

        // ================================
        // 保存処理
        // ================================
        public void Save(string path)
        {
            var list = new List<EventDto>();

            foreach (var ev in events)
            {
                var dto = new EventDto
                {
                    Type = ev.Type,
                    Meas = ev.Meas,
                    Step = ev.Step
                };

                switch (ev)
                {
                    case MIDINoteEvent seq:
                        dto.NoteName = seq.NoteName;
                        dto.KeyNumber = seq.KeyNumber;
                        dto.ST = seq.ST;
                        dto.GT = seq.GT;
                        dto.Vel = seq.Vel;
                        break;

                    case MIDISysExEvent sx:
                        dto.ST = sx.ST;
                        dto.Data = sx.Data;
                        break;

                    case MIDISameMeasEvent sm:
                        dto.RepeatMeas = sm.RepeatMeas;
                        break;

                    case MIDIBarLineEvent bar:
                        dto.MeasureTotalST = bar.MeasureTotalST;
                        break;

                    case MIDIMemoEvent memo:
                        dto.Text = memo.Text;
                        break;

                    case MIDIProgramChangeEvent pc:
                        dto.ST = pc.ST;
                        dto.ProgramNumber = pc.ProgramNumber;
                        break;

                    case MIDIControlChangeEvent cc:
                        dto.ST = cc.ST;
                        dto.ControllerNumber = cc.ControllerNumber;
                        dto.ControlChangeValue = cc.ControllerValue;
                        break;

                    case MIDIPitchBendEvent pb:
                        dto.ST = pb.ST;
                        dto.PitchBendValue = pb.PitchValue;
                        break;

                    case MIDIChannelPressureEvent cp:
                        dto.ST = cp.ST;
                        dto.CPressureValue = cp.PressureValue;
                        break;

                    case MIDIPolyphonicKeyPressureEvent pkp:
                        dto.ST = pkp.ST;
                        dto.KeyNumber = pkp.KeyNumber;
                        dto.PPressureValue = pkp.PressureValue;
                        break;

                    case MIDITempoEvent te:
                        dto.ST = te.ST;
                        dto.TempoValue = te.TempoValue;
                        break;
                }

                list.Add(dto);
            }

            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        // ================================
        // 読み込み処理
        // ================================
        public void Load(string path)
        {
            var json = File.ReadAllText(path);
            var list = JsonSerializer.Deserialize<List<EventDto>>(json);

            var newEvents = new LinkedList<MIDIEvent>();
            var newCache = new List<LinkedListNode<MIDIEvent>>();

            foreach (var dto in list)
            {
                MIDIEvent ev = dto.Type switch
                {
                    MIDIEventType.NoteON => new MIDINoteEvent()
                    {
                        NoteName = dto.NoteName,
                        ST = dto.ST,
                        GT = dto.GT,
                        Vel = dto.Vel
                    },

                    MIDIEventType.SysExF0 => new MIDISysExEvent()
                    {
                        ST = dto.ST,
                        Data = dto.Data
                    },

                    MIDIEventType.SameMeas => new MIDISameMeasEvent()
                    {
                        RepeatMeas = dto.RepeatMeas
                    },

                    MIDIEventType.BarLine => new MIDIBarLineEvent()
                    {
                        ST = 0,
                    },

                    MIDIEventType.MetaEndOfTrack => new MIDIEndEvent(),
                    MIDIEventType.Memo => new MIDIMemoEvent()
                    {
                        Text = dto.Text
                    },
                    MIDIEventType.ProgramChange=> new MIDIProgramChangeEvent()
                    {
                        ST = dto.ST,
                        ProgramNumber = dto.ProgramNumber
                    },
                    MIDIEventType.ControlChange => new MIDIControlChangeEvent()
                    {
                        ST = dto.ST,
                        ControllerNumber = dto.ControllerNumber,
                        ControllerValue = dto.ControlChangeValue
                    },
                    MIDIEventType.PitchBend => new MIDIPitchBendEvent()
                    {
                        ST = dto.ST,
                        PitchValue = dto.PitchBendValue
                    },
                    MIDIEventType.MetaTempo => new MIDITempoEvent()
                    {
                        ST = dto.ST,
                        TempoValue = dto.TempoValue
                    },
                    MIDIEventType.ChannelAfterTouch => new MIDIChannelPressureEvent()
                    {
                        ST = dto.ST,
                        PressureValue = dto.CPressureValue
                    },
                    MIDIEventType.KeyAfterTouch => new MIDIPolyphonicKeyPressureEvent()
                    {
                        ST = dto.ST,
                        KeyNumber = dto.KeyNumber,
                        PressureValue = dto.PPressureValue
                    },
                    _ => throw new Exception("Unknown event type")
                };

                var node = newEvents.AddLast(ev);
                newCache.Add(node);
                if (ev.Type == MIDIEventType.SameMeas)
                    sameMeasList.Add(new SameMeasInfo { Index = newCache.Count - 1, Node = node });
                if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                    barLineList.Add(new BarLineInfo { Index = newCache.Count - 1, Node = node });
            }

            this.events = newEvents;
            this.indexCache = newCache;

            AdjustAllEvents();
        }


        // ================================
        // イベント取得（UI用）
        // ================================

        // インデックス指定でイベントを取得
        public MIDIEvent GetEventAt(int index)
        {
            if (index < 0 || index >= indexCache.Count)
                return null;

            return indexCache[index].Value;
        }

        // STsum に基づく挿入位置検索
        public int FindInsertIndexBySTsum(int stsum)
        {
            //// ★ 線形探索（イベント数が数千なら十分高速）
            //for (int i = 0; i < indexCache.Count; i++)
            //{
            //    if (indexCache[i].Value.STsum > stsum)
            //        return i;
            //}
            return indexCache.Count; // 最後に挿入
        }

        // ================================
        // 追加処理
        // ================================

        // 末尾に追加（内部処理）
        // データ読み込み時のみ使用
        public void AddLast(MIDIEvent ev)
        {
            var node = events.AddLast(ev);
            indexCache.Add(node);
            if (ev.Type == MIDIEventType.SameMeas)
                sameMeasList.Add(new SameMeasInfo { Index = indexCache.Count - 1, Node = node });
            if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                barLineList.Add(new BarLineInfo { Index = indexCache.Count - 1, Node = node });
        }

        // 指定ノードの前に追加（内部処理）
        public void AddBefore(LinkedListNode<MIDIEvent> eventNode, MIDIEvent ev)
        {
            var node = events.AddBefore(eventNode, ev);

            // ★ indexCache に挿入位置を反映
            int idx = indexCache.IndexOf(eventNode);
            if (idx >= 0)
            {
                indexCache.Insert(idx, node);

                // ★ SameMeas の index 補正
                foreach (var info in sameMeasList)
                {
                    if (info.Index >= idx)
                        info.Index++;
                }
                if (ev.Type == MIDIEventType.SameMeas)
                    sameMeasList.Add(new SameMeasInfo { Index = idx, Node = node });
                foreach (var info in barLineList)
                {
                    if (info.Index >= idx)
                        info.Index++;
                }
                if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                    barLineList.Add(new BarLineInfo { Index = idx, Node = node });
            }
            else
            {
                indexCache.Add(node); // fallback
                if (ev.Type == MIDIEventType.SameMeas)
                    sameMeasList.Add(new SameMeasInfo { Index = indexCache.Count - 1, Node = node });
                if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                    barLineList.Add(new BarLineInfo { Index = indexCache.Count - 1, Node = node });
            }

            var startNode = FindStartNodeForAdjust(node);
            AdjustMeasureAndStepInCurrentEvent(startNode, ev.Type);

        }

        // イベント挿入（UI用）
        public void InsertEvent(MIDIEvent ev)
        {
            // STsum に基づいて挿入位置を決める
            int index = FindInsertIndexBySTsum(ev.STsum);
            InsertEventAt(index, ev);
        }

        // インデックス指定挿入（内部処理）
        public void InsertEventAt(int index, MIDIEvent ev)
        {
            LinkedListNode<MIDIEvent> node;

            if (index >= indexCache.Count)
            {
                node = events.AddLast(ev);
                indexCache.Add(node);
                // ★ SameMeas の index 補正
                foreach (var info in sameMeasList)
                {
                    if (info.Index > index)
                        info.Index++;
                }
                if (ev.Type == MIDIEventType.SameMeas)
                    sameMeasList.Add(new SameMeasInfo { Index = indexCache.Count - 1, Node = node });
                if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                    barLineList.Add(new BarLineInfo { Index = indexCache.Count - 1, Node = node });
            }
            else
            {
                var nextNode = indexCache[index];
                node = events.AddBefore(nextNode, ev);
                indexCache.Insert(index, node);
                // ★ SameMeas の index 補正
                foreach (var info in sameMeasList)
                {
                    if (info.Index > index)
                        info.Index++;
                }
                if (ev.Type == MIDIEventType.SameMeas) sameMeasList.Add(new SameMeasInfo { Index = index, Node = node });
                foreach (var info in barLineList)
                {
                    if (info.Index > index)
                        info.Index++;
                }
                if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas) barLineList.Add(new BarLineInfo { Index = index, Node = node });
            }

            var startNode = FindStartNodeForAdjust(node);
            AdjustMeasureAndStepInCurrentEvent(startNode, ev.Type);
        }


        // ================================
        // 削除処理
        // ================================

        // イベント削除（内部処理）
        public void Remove(MIDIEvent ev)
        {
            var node = events.Find(ev);
            if (node == null)
                return;
            int index = indexCache.IndexOf(node);


            var startNode = FindStartNodeForAdjust(node, true);
            indexCache.Remove(node);
            events.Remove(node);

            // ★ SameMeas の index 補正
            foreach (var info in sameMeasList)
            {
                if (info.Index > index)
                    info.Index--;
            }
            if (ev.Type == MIDIEventType.SameMeas)
                sameMeasList.RemoveAll(info => info.Node == node);
            foreach (var info in barLineList)
            {
                if (info.Index > index)
                    info.Index--;
            }
            if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                barLineList.RemoveAll(info => info.Node == node);

            AdjustMeasureAndStepInCurrentEvent(startNode, ev.Type);
        }

        // イベント削除（UI用）
        public void RemoveEventAt(int index)
        {
            if (index < 0 || index >= indexCache.Count)
                return;

            var node = indexCache[index];
            var ev = node.Value;


            var startNode = FindStartNodeForAdjust(node, true);

            indexCache.RemoveAt(index);
            events.Remove(node);

            // ★ SameMeas の index 補正
            foreach (var info in sameMeasList)
            {
                if (info.Index > index)
                    info.Index--;
            }
            if (ev.Type == MIDIEventType.SameMeas)
                sameMeasList.RemoveAll(info => info.Node == node);
            foreach (var info in barLineList)
            {
                if (info.Index > index)
                    info.Index--;
            }
            if (ev.Type == MIDIEventType.BarLine || ev.Type == MIDIEventType.SameMeas)
                barLineList.RemoveAll(info => info.Node == node);

            AdjustMeasureAndStepInCurrentEvent(startNode, ev.Type);
        }


        // ================================
        // 高速インデックスアクセス
        // ================================
        public LinkedListNode<MIDIEvent> GetEventNodeAtIndex(int index)
        {
            if (index < 0) index = 0;
            if (index >= indexCache.Count) index = indexCache.Count - 1;

            return indexCache[index];
        }

        // ================================
        // Step/Meas と　SameMeasのRepeatMeas 調整処理
        // ================================
        public void AdjustAllEvents()
        {
            int meas = 0;
            int step = 0;
            int sum = 0;

            LinkedListNode<MIDIEvent> node = events.First;
            while (node != null)
            {
                var ev = node.Value;

                ev.Meas = meas;
                ev.Step = step;
                ev.STsum = sum + ev.ST;
                step++;
                sum += ev.ST;
                if (ev.Type == MIDIEventType.BarLine)
                {
                    ((MIDIBarLineEvent)ev).MeasureTotalST = sum;
                    ev.STsum = 0;
                    meas++;
                    step = 0;
                    sum = 0;
                }
                else if (ev.Type == MIDIEventType.SameMeas)
                {
                    if (node.Previous.Value.Type != MIDIEventType.BarLine && node.Previous.Value.Type != MIDIEventType.SameMeas)
                        ev.Meas++;
                    ev.Step = 0;
                    meas++;
                    if (node.Previous.Value.Type != MIDIEventType.BarLine && node.Previous.Value.Type != MIDIEventType.SameMeas)
                        meas++;
                    step = 0;
                }

                node = node.Next;
            }

            // sameMeasのRepeatMeas/BeforeNodeの調整
            foreach (var si in sameMeasList)
            {
                MIDISameMeasEvent se = (MIDISameMeasEvent)si.Node.Value;
                SetRepeatMeasNode(se);
            }

            UpdateTieFlags();
        }

        public void SetRepeatMeasNode(MIDISameMeasEvent se)
        {
            int repeatMeas = se.RepeatMeas;

            if (repeatMeas == 0)
            {
                se.RepeatMeasBeforeNode = null;
                return;
            }

            foreach (var bl in barLineList)
            {
                if (bl.Node.Value.Meas != repeatMeas - 1) continue;
                se.RepeatMeasBeforeNode = bl.Node;
                break;
            }

            if (se.RepeatMeasBeforeNode == null) se.RepeatMeas = -1;
        }

        internal void UpdateTieFlags()
        {
            // 1. 初期化
            foreach (var tie in tieWork)
            {
                tie.Node = null;
                tie.GT = 0;
            }

            // 2. イベントを順に処理
            var node = events.First;
            while (node != null)
            {
                ProgressExistingTies(node);   // 既存のタイを進める（減算＋Tie判定）
                RegisterTieSourceIfNeeded(node); // 今のノートを新しいタイ元として登録

                if (node.Value.Type == MIDIEventType.BarLine || node.Value.Type == MIDIEventType.SameMeas)
                    node.Value.TieCache = CloneTieCache(tieWork); // キャッシュ生成

                node = node.Next;
            }
        }

        internal void UpdateTieFlagsDiff(LinkedListNode<MIDIEvent> changedNode)
        {
            // 1. 差分更新の起点を探す
            var start = changedNode?.Previous;
            while (start != null && start.Value.TieCache == null)
                start = start.Previous;

            // キャッシュが無い → フルスキャン
            if (start == null)
            {
                UpdateTieFlags();
                return;
            }

            // 2. キャッシュを復元
            LoadTieCache(start.Value.TieCache);

            // 3. 起点から最後まで再計算
            var node = start.Next;

            while (node != null)
            {
                if (node.Value.Type == MIDIEventType.NoteON) ((MIDINoteEvent)node.Value).Tie = false;
                ProgressExistingTies(node);   // 既存のタイを進める（減算＋Tie判定）
                RegisterTieSourceIfNeeded(node); // 今のノートを新しいタイ元として登録

                if (node.Value.Type == MIDIEventType.BarLine || node.Value.Type == MIDIEventType.SameMeas)
                    node.Value.TieCache = CloneTieCache(tieWork);

                node = node.Next;
            }
        }

        private void ProgressExistingTies(LinkedListNode<MIDIEvent> node)
        {
            int sT = node.Value.ST;

            foreach (var tie in tieWork)
            {
                if (tie.Node == null) continue;

                var prevNote = (MIDINoteEvent)tie.Node.Value;
                // 同じキーのノートが来たら Tie
                if (node.Value.Type == MIDIEventType.NoteON)
                {
                    var curNote = (MIDINoteEvent)node.Value;
                    if (prevNote.KeyNumber == curNote.KeyNumber)
                    {
                        prevNote.Tie = true;
                    }
                }

                // 残り GT を減算
                tie.GT -= sT;
                if (tie.GT > 0) continue;

                tie.Node = null;
                tie.GT = 0;
            }
        }

        private void RegisterTieSourceIfNeeded(LinkedListNode<MIDIEvent> node)
        {
            if (node.Value.Type != MIDIEventType.NoteON) return;

            var note = (MIDINoteEvent)node.Value;
            // 今のノートが伸びるなら新しいタイ元として登録
            if (note.GT <= note.ST)
            {
                note.Tie = false;
                return;
            }

            int key = note.KeyNumber;
            tieWork[key].Node = node;
            tieWork[key].GT = note.GT - note.ST;
        }

        private MIDITieCache[] CloneTieCache(MIDITieCache[] src)
        {
            var dst = new MIDITieCache[128];
            for (int i = 0; i < 128; i++)
            {
                dst[i] = new MIDITieCache
                {
                    Node = src[i].Node,
                    GT = src[i].GT
                };
            }
            return dst;
        }

        private void LoadTieCache(MIDITieCache[] cache)
        {
            for (int i = 0; i < 128; i++)
            {
                tieWork[i].Node = cache[i].Node;
                tieWork[i].GT = cache[i].GT;
            }
        }

        public LinkedListNode<MIDIEvent> FindStartNodeForAdjust(LinkedListNode<MIDIEvent> node, bool isRemove = false)
        {
            var ev = node.Value;
            var cur = node;
            LinkedListNode<MIDIEvent> returnNode;

            while (cur.Previous != null)
            {
                if (cur.Previous.Value.Type == MIDIEventType.BarLine ||
                    cur.Previous.Value.Type == MIDIEventType.SameMeas)
                {
                    returnNode = cur;
                    if (isRemove && returnNode == node) returnNode = node.Next;
                    return returnNode;
                }
                cur = cur.Previous;
            }

            returnNode = events.First;
            if (isRemove && returnNode == node) returnNode = node.Next;
            return returnNode;
        }

        public void AdjustMeasureAndStepInCurrentEvent(LinkedListNode<MIDIEvent> workEvent, MIDIEventType acticonEventType)
        {
            if (workEvent == null)
                return;

            LinkedListNode<MIDIEvent> workEvent2 = workEvent;

            int meas;
            if (workEvent.Previous != null)
                meas = workEvent.Previous.Value.Meas + 1;
            else
            {
                meas = workEvent.Value.Meas;
                if (acticonEventType == MIDIEventType.BarLine
                    || acticonEventType == MIDIEventType.SameMeas
                    || workEvent.Value.Type == MIDIEventType.BarLine
                    || workEvent.Value.Type == MIDIEventType.SameMeas
                        ) meas--;
                meas = Math.Max(0, meas);
            }

            int step = 0;
            int sum = 0;
            bool isFirst = true;

            while (workEvent != null)
            {
                workEvent.Value.Meas = meas;
                workEvent.Value.Step = step;
                if (workEvent.Value.ST != 0) step++;
                sum += workEvent.Value.ST;

                if (workEvent.Value.Type == MIDIEventType.BarLine)
                {
                    ((MIDIBarLineEvent)workEvent.Value).MeasureTotalST = sum;
                    meas++;
                    step = 0;
                    sum = 0;
                    isFirst = true;
                }
                else if (workEvent.Value.Type == MIDIEventType.SameMeas
                    || workEvent.Value.Type == MIDIEventType.MetaEndOfTrack)
                {
                    if (!isFirst)
                    {
                        meas++;
                        isFirst = true;
                    }
                    workEvent.Value.Meas = meas;
                    workEvent.Value.Step = 0;
                    meas++;
                    step = 0;
                    sum = 0;
                }
                else
                {
                    isFirst = false;
                }

                workEvent = workEvent.Next;
            }

            // BarLine または SameMeas イベントの追加・削除後に、同じ小節を参照している SameMeas イベントの RepeatMeas を調整する
            if (acticonEventType == MIDIEventType.BarLine || acticonEventType == MIDIEventType.SameMeas)
            {
                foreach (var si in sameMeasList)
                {
                    //参照先の BarLine または SameMeas が削除された場合、RepeatMeasBeforeNode を null、RepeatMeasを Ref!(-1)状態 にする
                    LinkedListNode<MIDIEvent> ev = ((MIDISameMeasEvent)si.Node.Value).RepeatMeasBeforeNode;
                    if ((ev == null && ((MIDISameMeasEvent)si.Node.Value).RepeatMeas != 0)
                        || (ev != null && ev.Previous == null && ev.Next == null))
                    {
                        ((MIDISameMeasEvent)si.Node.Value).RepeatMeasBeforeNode = null;
                        ((MIDISameMeasEvent)si.Node.Value).RepeatMeas = -1;
                        continue;
                    }

                }
            }

            // RepeatMeasが1以上のSameMeasEventの調整を行う
            //  RepeatMeasBeforeNodeが繰り返し対象の開始イベントの一つ手前にいるので、そのイベントのMeasに1を足した値をRepeatMeasに設定する
            // 又、参照先が同じ小節値(自分自身)の場合は循環参照を避ける為にRef!(-1)状態にする
            foreach (var si in sameMeasList)
            {
                MIDISameMeasEvent sme = (MIDISameMeasEvent)si.Node.Value;
                int repeatMeas = sme.RepeatMeas;

                if (repeatMeas > 0) sme.RepeatMeas = sme.RepeatMeasBeforeNode.Value.Meas + 1;

                // 参照先が同じ小節値(自分自身)の場合はRef!(-1)状態にする
                if (repeatMeas != si.Node.Value.Meas) continue;

                sme.RepeatMeasBeforeNode = null;
                sme.RepeatMeas = -1;
            }

            UpdateTieFlagsDiff(workEvent2);

        }

        public MIDIEvent CreateEvent(MIDIEventType type)
        {
            return type switch
            {
                MIDIEventType.NoteON => new MIDINoteEvent(),
                MIDIEventType.SysExF0 => new MIDISysExEvent(),
                MIDIEventType.SameMeas => new MIDISameMeasEvent(),
                MIDIEventType.BarLine => new MIDIBarLineEvent(),
                MIDIEventType.MetaEndOfTrack => new MIDIEndEvent(),
                MIDIEventType.Memo => new MIDIMemoEvent(),
                MIDIEventType.ProgramChange => new MIDIProgramChangeEvent(),
                MIDIEventType.ControlChange => new MIDIControlChangeEvent(),
                MIDIEventType.PitchBend => new MIDIPitchBendEvent(),
                MIDIEventType.Meta => new MIDIMetaEvent(),
                MIDIEventType.MetaTempo => new MIDITempoEvent(),
                MIDIEventType.ChannelAfterTouch => new MIDIChannelPressureEvent(),
                MIDIEventType.KeyAfterTouch => new MIDIPolyphonicKeyPressureEvent(),
                _ => throw new NotSupportedException()
            };
        }

        public void Init()
        {
            sameMeasList.Clear();
            barLineList.Clear();
        }

        public void SetPartData(MIDIPart prt)
        {
            this.prt = prt;
            this.events = prt.Event;
            this.indexCache.Clear();
            var node = events.First;
            while (node != null)
            {
                indexCache.Add(node);
                if (node.Value.Type == MIDIEventType.SameMeas)
                    sameMeasList.Add(new SameMeasInfo { Index = indexCache.Count - 1, Node = node });
                if (node.Value.Type == MIDIEventType.BarLine || node.Value.Type == MIDIEventType.SameMeas)
                    barLineList.Add(new BarLineInfo { Index = indexCache.Count - 1, Node = node });
                node = node.Next;
            }
            AdjustAllEvents();
        }

        public MdlEvent GetModel(MIDIEventType evType)
        {
            switch (evType)
            {
                case MIDIEventType.ControlChange:
                    return mdlControlChangeEvent;
                case MIDIEventType.ProgramChange:
                    return mdlProgramChangeEvent;
                case MIDIEventType.Meta:
                    return mdlMetaEvent;
                case MIDIEventType.MetaTempo:
                    return mdlTempoEvent;
                case MIDIEventType.Memo:
                    return mdlMemoEvent;
                case MIDIEventType.PitchBend:
                    return mdlPitchBendEvent;
                case MIDIEventType.ChannelAfterTouch:
                    return mdlChannelPressureEvent;
                case MIDIEventType.KeyAfterTouch:
                    return mdlPolyphonicKeyPressureEvent;
                case MIDIEventType.NoteON:
                    return mdlNoteEvent;
                case MIDIEventType.SysExF0:
                    return mdlSysExEvent;
                case MIDIEventType.SameMeas:
                    return mdlSameMeasEvent;
                case MIDIEventType.BarLine:
                    return mdlBarLineEvent;
                case MIDIEventType.MetaEndOfTrack:
                    return mdlEndEvent;

                case MIDIEventType.NoteOff:
                    return mdlNoteEvent;
                case MIDIEventType.MetaChannelPrefix:
                    return mdlMetaEvent;
                case MIDIEventType.MetaCopyrightNotice:
                    return mdlMemoEvent;
                case MIDIEventType.MetaCuePoint:
                    return mdlMetaEvent;
                case MIDIEventType.MetaDeviceName:
                    return mdlMetaEvent;
                case MIDIEventType.MetaInstrumentName:
                    return mdlMetaEvent;
                case MIDIEventType.MetaKeySignature:
                    return mdlMetaEvent;
                case MIDIEventType.MetaLyric:
                    return mdlMetaEvent;
                case MIDIEventType.MetaMarker:
                    return mdlMetaEvent;
                case MIDIEventType.MetaPortPrefix:
                    return mdlMetaEvent;
                case MIDIEventType.MetaProgramName:
                    return mdlMemoEvent;
                case MIDIEventType.MetaSeqNumber:
                    return mdlMetaEvent;
                case MIDIEventType.MetaSequencerSpecific:
                    return mdlMetaEvent;
                case MIDIEventType.MetaSMPTEOffset:
                    return mdlMetaEvent;
                case MIDIEventType.MetaTextEvent:
                    return mdlMetaEvent;
                case MIDIEventType.MetaTimeSignature:
                    return mdlMetaEvent;
                case MIDIEventType.MetaTrackName:
                    return mdlMemoEvent;
            }

            return null;
        }
    }
}