using Remcoposer64.ProjectData.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    [Serializable]
    public class MIDIPart
    {
        public int Number { get; set; } = 0;
        public string Name { get; set; } = "";
        public int StartTick { get; set; } = 0;
        /// <summary>
        /// イベントリスト
        /// </summary>
        public LinkedList<MIDIEvent> Event { get; set; } = new LinkedList<MIDIEvent>();
        public bool Played { get; set; } = false;

        //初めのイベントを得る
        public LinkedListNode<MIDIEvent> getStartEventNode()
        {
            return Event.First;
        }

        //最後のイベントを得る
        public LinkedListNode<MIDIEvent> getEndEventNode()
        {
            return Event.Last;
        }

        //指定したイベントの次のイベントを得る
        public LinkedListNode<MIDIEvent> getNextEventNode(LinkedListNode<MIDIEvent> eve)
        {
            return eve.Next;
        }

        ////指定したイベントの前のイベントを得る
        //public MIDIEvent getPrevEvent(MIDIEvent eve)
        //{
        //    if (eve == null || eve.BeforeIndex == null) return null;

        //    return Event[(int)eve.BeforeIndex];
        //}

        ////指定したイベントを除外する（メモリには残る）
        //public bool removeEvent(MIDIEvent eve)
        //{
        //    if (eve == null) return false;
        //    MIDIEvent pEvent = getPrevEvent(eve);
        //    MIDIEvent nEvent = getNextEvent(eve);
        //    if (pEvent != null) pEvent.AfterIndex = (nEvent == null) ? null : (int?)nEvent.Number;
        //    if (nEvent != null) nEvent.BeforeIndex = (pEvent == null) ? null : (int?)pEvent.Number;
        //    pEvent.ST += eve.ST;
        //    this.eCounter--;

        //    return true;
        //}

        //指定したイベントをメモリから消去する(removeEventに比べ低速)
        public bool clearEventNode(LinkedListNode<MIDIEvent> eve)
        {
            if (eve == null) return false;
            Event.Remove(eve);
            return true;
        }

        ////全てのイベントをメモリから消去する
        //public void clearAllEventMemory()
        //{
        //    this.Event.Clear();
        //    this.eCounter = 0;
        //    this.eStartIndex = null;
        //    this.eEndIndex = null;
        //    this.eNumber = 0;
        //}

        //全てのイベントを消去する
        public void clearEventNode()
        {
            //this.eCounter = 0;
            //this.eStartIndex = null;
            //this.eEndIndex = null;
        }

        /// <summary>
        /// 指定されたイベントの後ろにイベントを挿入する
        /// </summary>
        /// <param name="TargetEvent">このイベントの後ろに新たに入る</param>
        /// <param name="Step">Step値</param>
        /// <param name="EventType">イベントタイプ</param>
        /// <param name="MIDImessage">MIDIメッセージ(Chは0固定であること)</param>
        /// <returns>新たに挿入したイベント</returns>
        public LinkedListNode<MIDIEvent> insertEventNode(LinkedListNode<MIDIEvent> TargetEvent, int Step, MIDIEventType EventType, byte[] MIDImessage)
        {
            //if (MIDImessage == null) return null;
            MIDIEvent eve = Events.MIDIEventsFactory.CreateMIDIEvent(EventType);
            eve.Type = EventType;
            eve.MIDIMessage = MIDImessage;
            eve.MIDIMessageLst = null;
            eve.ST = Step;

            if (eve is MIDIMemoEvent)
            {
                Encoding encoding = Common.Common.GetCode(MIDImessage, 0, MIDImessage.Length);
                if (encoding == null) encoding = Encoding.UTF8;
                string strFromByte = encoding.GetString(MIDImessage).Replace("\0", "");
                ((MIDIMemoEvent)eve).Text = strFromByte;
            }

            return insertEve(TargetEvent, Step, eve);
        }

        /// <summary>
        /// 指定されたイベントの後ろにイベントを挿入する
        /// </summary>
        /// <param name="TargetEvent">このイベントの後ろに新たに入る</param>
        /// <param name="Step">Step値</param>
        /// <param name="EventType">イベントタイプ</param>
        /// <param name="MIDImessage">MIDIメッセージ(Chは0固定であること)</param>
        /// <param name="gt">ゲートタイム</param>
        /// <returns>新たに挿入したイベント</returns>
        public LinkedListNode<MIDIEvent> insertEventNode(LinkedListNode<MIDIEvent> TargetEvent, int Step, MIDIEventType EventType, byte[] MIDImessage, int gt)
        {
            if (MIDImessage == null) return null;
            MIDIEvent eve = Events.MIDIEventsFactory.CreateMIDIEvent(EventType);
            eve.Type = EventType;
            eve.MIDIMessage = MIDImessage;
            eve.MIDIMessageLst = null;
            eve.ST = Step;
            if (eve is MIDINoteEvent)
            {
                ((MIDINoteEvent)eve).KeyNumber = MIDImessage[1];
                ((MIDINoteEvent)eve).GT = gt;
                ((MIDINoteEvent)eve).Vel = MIDImessage[2];
            }
            else if (eve is MIDITempoEvent)
            {
                ((MIDITempoEvent)eve).TempoValue =  60000000 / (eve.MIDIMessage[2] * 0x10000 + eve.MIDIMessage[3] * 0x100 + eve.MIDIMessage[4]);
            }
            else if (eve is MIDIControlChangeEvent)
            {
                ((MIDIControlChangeEvent)eve).ControllerNumber = MIDImessage[1];
                ((MIDIControlChangeEvent)eve).ControllerValue = MIDImessage[2];
            }
            else if (eve is MIDIProgramChangeEvent)
            {
                ((MIDIProgramChangeEvent)eve).ProgramNumber = MIDImessage[1];
            }
            else if(eve is MIDIPitchBendEvent)
            {
                ((MIDIPitchBendEvent)eve).PitchValue = (short)((MIDImessage[2] << 7) | MIDImessage[1]);
            }

            return insertEve(TargetEvent, Step, eve);
        }

        /// <summary>
        /// 指定されたイベントの後ろにイベントを挿入する
        /// </summary>
        /// <param name="TargetEvent">このイベントの後ろに新たに入る</param>
        /// <param name="Step">Step値</param>
        /// <param name="EventType">イベントタイプ</param>
        /// <param name="MIDImessageLst">MIDIメッセージ</param>
        /// <returns>新たに挿入したイベント</returns>
        public LinkedListNode<MIDIEvent> insertSpEventNode(LinkedListNode<MIDIEvent> TargetEvent, int Step, MIDISpEventType EventType, byte[][] MIDImessageLst)
        {
            if (MIDImessageLst == null) return null;
            int enm = (byte)EventType + 0x100;
            MIDIEvent eve = Events.MIDIEventsFactory.CreateMIDIEvent((MIDIEventType)enm);
            eve.MIDIMessage = new byte[1] { (byte)EventType };
            eve.MIDIMessageLst = MIDImessageLst;
            eve.ST = Step;
            if(eve.Type== MIDIEventType.RolandDevice)
            {
                ((MIDIRolandDeviceEvent)eve).RolandDev_gt = eve.MIDIMessageLst[0][0];
                ((MIDIRolandDeviceEvent)eve).RolandDev_vel = eve.MIDIMessageLst[0][1];
            }
            else if (eve.Type == MIDIEventType.RolandBase)
            {
                ((MIDIRolandBaseEvent)eve).RolandBase_gt = eve.MIDIMessageLst[0][0];
                ((MIDIRolandBaseEvent)eve).RolandBase_vel = eve.MIDIMessageLst[0][1];
            }
            else if (eve.Type == MIDIEventType.RolandPara)
            {
                ((MIDIRolandParaEvent)eve).RolandPara_gt = eve.MIDIMessageLst[0][0];
                ((MIDIRolandParaEvent)eve).RolandPara_vel = eve.MIDIMessageLst[0][1];
            }

            return insertEve(TargetEvent, Step, eve);
        }

        private LinkedListNode<MIDIEvent> insertEve(LinkedListNode<MIDIEvent> TargetEvent, int Step, MIDIEvent eve)
        {
            //イベントリストを生成
            if (this.Event == null)
            {
                this.Event = new LinkedList<MIDIEvent>();
            }
            if (TargetEvent == null || this.Event.Count == 0 
                //|| this.eStartIndex == null
                )//初めのイベント
            {
                return this.Event.AddLast(eve);
            }

            return this.Event.AddAfter(TargetEvent, eve);
        }

        ///// <summary>
        ///// Tickを考慮せずに最後のイベントの後ろに追加する。
        ///// </summary>
        ///// <param name="Tick"></param>
        ///// <param name="Type"></param>
        ///// <param name="MIDImessage"></param>
        //public void addEvent(int ST, MIDIEventType Type, byte[] MIDImessage)
        //{
        //    if (MIDImessage == null) return;
        //    MIDIEvent eve = new MIDIEvent();
        //    eve.Type = Type;
        //    eve.MIDIMessage = MIDImessage;
        //    eve.MIDIMessageLst = null;
        //    eve.ST = ST;

        //    MIDIEvent lastEvent = this.getEndEvent();
        //    if (lastEvent == null)//初めのイベント
        //    {
        //        eve.AfterIndex = null;
        //        eve.BeforeIndex = null;
        //        eve.Number = this.eNumber;
        //        this.Event.Add(eve);
        //        this.eStartIndex = 0;
        //        this.eEndIndex = 0;
        //        this.eCounter = 1;
        //        this.eNumber++;
        //        return;
        //    }

        //    eve.BeforeIndex = lastEvent.Number;
        //    eve.AfterIndex = null;
        //    eve.Number = this.eNumber;
        //    this.eEndIndex = this.eNumber;
        //    this.Event.Add(eve);
        //    this.eCounter++;
        //    this.eNumber++;
        //    lastEvent.AfterIndex = eve.Number;
        //}

    }

}
