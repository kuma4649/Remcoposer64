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

        public LinkedListNode<MIDIEvent> insertEve(LinkedListNode<MIDIEvent> TargetEvent, int Step, MIDIEvent eve)
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

        public MIDIPart Clone()
        {
            MIDIPart part = new MIDIPart();

            part.Number = Number;
            part.Name = Name;
            part.StartTick = StartTick;
            part.LengthTick = LengthTick;

            foreach (var evt in Event)
            {
                part.Event.AddLast(
                    evt.Clone());
            }

            return part;
        }
    }

}
