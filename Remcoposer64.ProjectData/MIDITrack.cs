using Remcoposer64.ProjectData.Events;
using System.Drawing;
using System.Xml.Serialization;

namespace Remcoposer64.ProjectData
{
    [Serializable]
    public class MIDITrack
    {
        private Color _Color = Color.Black;
        private LinkedList<MIDIPart> _Part = new LinkedList<MIDIPart>();
        private LinkedList<MIDIRythm> _Rythm = new LinkedList<MIDIRythm>();
        private bool _RythmMode = false;
        private int? _Key = 0;
        private int _St = 0;
        private int? _StartPartIndex = null;
        private int? _EndPartIndex = null;
        private int _NumberPart = 0;
        private int _NowPartIndex = 0;
        private MIDIPart _NowPart = null;
        private int _NowTick = 0;
        private int _NextEventTick = 0;
        private bool _EndMark = false;
        private int _TrackNumber = 0;
        private int? _LoopOrSameTargetEventIndex = null;
        private int? _SameMeasure = null;
        private byte _RolandBase_gt = 0;
        private byte _RolandBase_vel = 0;
        private byte _RolandDev_gt = 0;
        private byte _RolandDev_vel = 0;
        private byte _RolandPara_gt = 0;
        private byte _RolandPara_vel = 0;
        private int[] _NoteGateTime = new int[128];


        public int? BeforeIndex { get; set; } = null;
        public int? AfterIndex {  get; set; } = null;
        public int Number { get; set; } = 0;
        public string Name { get; set; } = "";
        public string Memo { get; set; } = "";
        public string Comment { get; set; } = "";

        public int OutDevice { get; set; } = -1;
        public int? OutChannel { get; set; } = 0;
        public int InDevice { get; set; } = -1;
        public int? InChannel { get; set; } = 0;

        public bool Solo { get; set; } = false;
        public bool Mute { get; set; } = false;

        [XmlIgnore]
        public Color Color
        {
            set
            {
                _Color = value;
            }
            get
            {
                return _Color;
            }
        }
        public LinkedList<MIDIPart> Part
        {
            set
            {
                _Part = value;
            }
            get
            {
                return _Part;
            }
        }
        public LinkedList<MIDIRythm> Rythm
        {
            set
            {
                _Rythm = value;
            }
            get
            {
                return _Rythm;
            }
        }
        public bool RythmMode
        {
            set
            {
                _RythmMode = value;
            }
            get
            {
                return _RythmMode;
            }
        }
        public int? Key
        {
            set
            {
                _Key = value;
            }
            get
            {
                return _Key;
            }
        }
        public int St
        {
            set
            {
                _St = value;
            }
            get
            {
                return _St;
            }
        }
        public int? mStartIndex
        {
            set
            {
                _StartPartIndex = value;
            }
            get
            {
                return _StartPartIndex;
            }
        }
        public int? mEndIndex
        {
            set
            {
                _EndPartIndex = value;
            }
            get
            {
                return _EndPartIndex;
            }
        }
        public int mNumber
        {
            set
            {
                _NumberPart = value;
            }
            get
            {
                return _NumberPart;
            }
        }
        public int TrackNumber
        {
            set
            {
                _TrackNumber = value;
            }
            get
            {
                return _TrackNumber;
            }
        }
        //private int mCounter = 0;

        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public MIDIPart NowPart
        {
            set
            {
                _NowPart = value;
            }
            get
            {
                return _NowPart;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public int NowTick
        {
            set
            {
                _NowTick = value;
            }
            get
            {
                return _NowTick;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public int NextEventTick
        {
            set
            {
                _NextEventTick = value;
            }
            get
            {
                return _NextEventTick;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public bool EndMark
        {
            set
            {
                _EndMark = value;
            }
            get
            {
                return _EndMark;
            }
        }

        public Stack<(LinkedListNode<MIDIEvent>, int?)> LoopStack { get; set; } = new Stack<(LinkedListNode<MIDIEvent>, int?)>();
        public Stack<LinkedListNode<MIDIEvent>> SameMeasStack { get; set; } = new Stack<LinkedListNode<MIDIEvent>>();

        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public int? LoopOrSameTargetEventIndex
        {
            set
            {
                _LoopOrSameTargetEventIndex = value;
            }
            get
            {
                return _LoopOrSameTargetEventIndex;
            }
        }

        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public int? SameMeasure
        {
            set
            {
                _SameMeasure = value;
            }
            get
            {
                return _SameMeasure;
            }
        }

        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public byte RolandBase_gt
        {
            set
            {
                _RolandBase_gt = value;
            }
            get
            {
                return _RolandBase_gt;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public byte RolandBase_vel
        {
            set
            {
                _RolandBase_vel = value;
            }
            get
            {
                return _RolandBase_vel;
            }
        }

        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public byte RolandDev_gt
        {
            set
            {
                _RolandDev_gt = value;
            }
            get
            {
                return _RolandDev_gt;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public byte RolandDev_vel
        {
            set
            {
                _RolandDev_vel = value;
            }
            get
            {
                return _RolandDev_vel;
            }
        }

        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public byte RolandPara_gt
        {
            set
            {
                _RolandPara_gt = value;
            }
            get
            {
                return _RolandPara_gt;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public byte RolandPara_vel
        {
            set
            {
                _RolandPara_vel = value;
            }
            get
            {
                return _RolandPara_vel;
            }
        }
        /// <summary>
        /// 演奏時専用なのでそれ以外の用途で使用不可
        /// </summary>
        [XmlIgnore]
        public int[] NoteGateTime
        {
            set
            {
                _NoteGateTime = value;
            }
            get
            {
                return _NoteGateTime;
            }
        }

        public bool Played { get; set; } = false;
        public int MemoWait { get; set; }
        public LinkedListNode<MIDIPart> CurrentPart { get; set; }

        //初めのpartを得る
        public LinkedListNode<MIDIPart> getStartPartNode()
        {
            //if (mStartIndex == null) return null;

            return Part.First;//.Value;// [(int)mStartIndex];
        }

        //指定したpartの次のpartを得る
        public LinkedListNode<MIDIPart> getNextPartNode(LinkedListNode<MIDIPart> prt)
        {
            if (prt == null 
                //|| prt.AfterIndex == null
                ) return null;

            return prt.Next;//  Part[(int)prt.AfterIndex];
        }

        //全てのPartをメモリから消去する
        public void clearAllPartMemory()
        {
            this.Part.Clear();
            //this.mCounter = 0;
            //this.mStartIndex = null;
            //this.mEndIndex = null;
            //this.mNumber = 0;
        }

        /// <summary>
        /// partを挿入する
        /// (既存partが増えると挿入位置を特定するのに時間がかかるようになるので注意)
        /// </summary>
        /// <param name="StartTick">絶対値によるTick値</param>
        /// <param name="prt">part</param>
        public void insertPart(int StartTick, MIDIPart prt)
        {
            if (prt == null) return;
            prt.StartTick = StartTick;

            if (Part == null) Part = new LinkedList<MIDIPart>();

            if (Part.Count == 0 
                //|| this.mStartIndex == null
                )//初めのprt
            {
                //prt.AfterIndex = null;
                //prt.BeforeIndex = null;
                //prt.Number = this.mNumber;
                this.Part.AddLast(prt);
                //this.mStartIndex = 0;
                //this.mEndIndex = 0;
                //this.mCounter = 1;
                //this.mNumber++;
                return;
            }

            //遅くなる原因になっているループ
            LinkedListNode<MIDIPart> pPrt = Part.First;// getStartPart();
            while (true)
            {
                if (pPrt.Value.StartTick > prt.StartTick)
                {
                    break;
                }
                pPrt = pPrt.Next;
            }

            Part.AddAfter(pPrt, prt);

        }

        public MIDITrack Clone()
        {
            MIDITrack trk = new MIDITrack
            {
                BeforeIndex = BeforeIndex,
                AfterIndex = AfterIndex,

                Number = Number,
                Name = Name,
                Memo = Memo,
                Comment = Comment,

                OutDevice = OutDevice,
                OutChannel = OutChannel,

                InDevice = InDevice,
                InChannel = InChannel,

                Solo = Solo,
                Mute = Mute,

                Color = Color,

                RythmMode = RythmMode,

                Key = Key,
                St = St,

                mStartIndex = mStartIndex,
                mEndIndex = mEndIndex,
                mNumber = mNumber,

                TrackNumber = TrackNumber
            };

            foreach (var r in Rythm)
            {
                trk.Rythm.AddLast(r.Clone());
            }

            foreach (var p in Part)
            {
                trk.Part.AddLast(p.Clone());
            }

            return trk;
        }
    }

}
