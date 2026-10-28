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
        public int LengthTick { get; set; } = 0;

        /// <summary>
        /// イベントリスト
        /// </summary>
        public LinkedList<MIDIEvent> Event { get; set; } = new LinkedList<MIDIEvent>();
        public LinkedListNode<MIDIEvent> CurrentEvent { get; set; }
        public int CurrentEventFrame { get; set; }
        //public Stack<LinkedListNode<MIDIEvent>> JumpReturnStack { get; set; }

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

        //指定したイベントをメモリから消去する
        public bool clearEventNode(LinkedListNode<MIDIEvent> eve)
        {
            if (eve == null) return false;
            Event.Remove(eve);
            return true;
        }

        //全てのイベントを消去する
        public void clearEventNode()
        {
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
            //if (MIDImessage == null || MIDImessage.Length<1) return null;
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
            else if(eve is MIDIBarLineEvent)
            {
                ;
                if (eve.Type == MIDIEventType.MetaTimeSignature)
                {
                    //
                    eve.Type = MIDIEventType.BarLine;
                }
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
            else if (eve.Type == MIDIEventType.LoopEnd)
            {
                ((MIDILoopEndEvent)eve).LoopCounter = eve.MIDIMessageLst[0][0] + (eve.MIDIMessageLst[0][1] << 8);
            }
            else if (eve.Type == MIDIEventType.SysExF0)
            {
                ((MIDISysExEvent)eve).Data = new byte[eve.MIDIMessageLst[0].Length - 2];
                Array.Copy(eve.MIDIMessageLst[0], 0, ((MIDISysExEvent)eve).Data, 0, eve.MIDIMessageLst[0].Length - 2);
                ((MIDISysExEvent)eve).GT = eve.MIDIMessageLst[0][eve.MIDIMessageLst[0].Length - 2];
                ((MIDISysExEvent)eve).VEL = eve.MIDIMessageLst[0][eve.MIDIMessageLst[0].Length - 1];
            }
            else if (eve.Type == MIDIEventType.BankProgram)
            {
                ((MIDIBankProgramEvent)eve).ProgramNumber = eve.MIDIMessageLst[0][1];
                ((MIDIBankProgramEvent)eve).ControlChange = eve.MIDIMessageLst[1][2];
            }
            else if (eve.Type == MIDIEventType.MetaTempo)
            {
                ((MIDITempoEvent)eve).TempoValue = -1;
            }
            else if (eve.Type == MIDIEventType.UserExclusive1)
            {
                ((MIDIUsrExEvent)eve).GT = eve.MIDIMessageLst[0][eve.MIDIMessageLst[0].Length - 2];
                ((MIDIUsrExEvent)eve).VEL = eve.MIDIMessageLst[0][eve.MIDIMessageLst[0].Length - 1];
            }

            return insertEve(TargetEvent, Step, eve);
        }

        private LinkedListNode<MIDIEvent> insertEve(LinkedListNode<MIDIEvent> TargetEvent, int Step, MIDIEvent eve)
        {
            //イベントリストを生成
            if (this.Event == null)
            {
                this.Event = new LinkedList<MIDIEvent>();
                this.LengthTick = 0;
            }

            if (TargetEvent == null || this.Event.Count == 0)//初めのイベント
            {
                this.LengthTick = eve.ST;
                return this.Event.AddLast(eve);
            }

            this.LengthTick += eve.ST;
            return this.Event.AddAfter(TargetEvent, eve);
        }

    }

}
