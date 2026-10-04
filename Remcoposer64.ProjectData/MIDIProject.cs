using Remcoposer64.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Remcoposer64.ProjectData
{
    [Serializable]
    public class MIDIProject
    {
        private MIDIInformation _Information = new MIDIInformation();
        //private List<MIDITrack> _Track = new List<MIDITrack>();
        private List<MIDIUserExclusive> _UserExclusive = new List<MIDIUserExclusive>();
        private List<MIDIRythm> _Rythm = new List<MIDIRythm>();
        private bool _RelativeTempoChangeSW = false;
        private double _RelativeTempoChangeTargetTempo = 0.0;
        private double _RelativeTempoChangeTickSlice = 0.0;
        private double _RelativeTempoChangeNowTempo = 0.0;
        private Setting setting;

        public MIDIInformation Information
        {
            set
            {
                _Information = value;
            }
            get
            {
                return _Information;
            }
        }
        public LinkedList<MIDITrack> Track { get; set; } = new LinkedList<MIDITrack>();
        //{
        //    set
        //    {
        //        _Track = value;
        //    }
        //    get
        //    {
        //        return _Track;
        //    }
        //}
        public List<MIDIUserExclusive> UserExclusive
        {
            set
            {
                _UserExclusive = value;
            }
            get
            {
                return _UserExclusive;
            }
        }
        public List<MIDIRythm> Rythm
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

        public bool RelativeTempoChangeSW
        {
            set
            {
                _RelativeTempoChangeSW = value;
            }
            get
            {
                return _RelativeTempoChangeSW;
            }
        }
        public double RelativeTempoChangeTargetTempo
        {
            set
            {
                _RelativeTempoChangeTargetTempo = value;
            }
            get
            {
                return _RelativeTempoChangeTargetTempo;
            }
        }
        public double RelativeTempoChangeTickSlice
        {
            set
            {
                _RelativeTempoChangeTickSlice = value;
            }
            get
            {
                return _RelativeTempoChangeTickSlice;
            }
        }
        public double RelativeTempoChangeNowTempo
        {
            set
            {
                _RelativeTempoChangeNowTempo = value;
            }
            get
            {
                return _RelativeTempoChangeNowTempo;
            }
        }

        public MIDIProject(Setting setting)
        {
            this.setting = setting;
            this.Information = new MIDIInformation();
            this.Track = new LinkedList<MIDITrack>();
            this.UserExclusive = new List<MIDIUserExclusive>();
            this.Rythm = new List<MIDIRythm>();
        }


        public void Load(string fn)
        {
            try
            {
                MIDIProject Project = new MIDIProject(setting);
                string filename = fn;
                if (!File.Exists(filename))
                {
                    throw new Exception(string.Format("{0}が見つかりませんでした。", filename));
                }

                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MIDIProject));
                    Project = (MIDIProject)serializer.Deserialize(fs);
                    fs.Close();
                    this.Information = Project.Information;
                    this.Track = Project.Track;
                }
            }
            catch
            {
                throw ;
            }
        }
        public void Save(string fn)
        {
            try
            {
                string filename = fn;

                using (FileStream fs = new FileStream(filename, FileMode.Create))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MIDIProject));
                    serializer.Serialize(fs, this);
                    fs.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        //初めのトラックを得る
        public LinkedListNode<MIDITrack> getStartTrackNode()
        {
            //if (Information.tStartIndex == null) return null;

            return Track.First;// (int)Information.tStartIndex];
        }

        //最後の小節を得る
        public LinkedListNode<MIDITrack> getEndTrackNode()
        {
            //if (Information.tEndIndex == null) return null;

            return Track.Last;// [(int)Information.tEndIndex];
        }

        //指定した小節の次の小節を得る
        public LinkedListNode<MIDITrack> getNextTrackNode(LinkedListNode<MIDITrack>  trk)
        {
            if (trk == null
                //|| trk.AfterIndex == null
                ) return null;

            return trk.Next;// Track[(int)trk.AfterIndex];
        }

        //指定した小節の前の小節を得る
        public LinkedListNode<MIDITrack> getPrevTrackNode(LinkedListNode< MIDITrack> trk)
        {
            if (trk == null 
                //|| trk.BeforeIndex == null
                ) return null;

            return trk.Previous;// Track[(int)trk.BeforeIndex];
        }

        //全ての小節をメモリから消去する
        public void clearAllTrackMemory()
        {
            this.Track.Clear();
            this.Information.tCounter = 0;
            this.Information.tStartIndex = null;
            this.Information.tEndIndex = null;
            this.Information.tNumber = 0;
        }

        //全ての小節を消去する
        public void clearTrack()
        {
            this.Information.tCounter = 0;
            this.Information.tStartIndex = null;
            this.Information.tEndIndex = null;
        }

        /// <summary>
        /// 小節を挿入する
        /// (既存小節が増えると挿入位置を特定するのに時間がかかるようになるので注意)
        /// </summary>
        /// <param name="StartTick">絶対値によるTick値</param>
        /// <param name="EndTick">絶対値によるTick値</param>
        /// <param name="mea">小節</param>
        public void insertTrack(int TrackNumber, MIDITrack trk)
        {
            if (trk == null) return;
            trk.TrackNumber = TrackNumber;

            if (this.Track == null)
            {
                this.Track = new LinkedList<MIDITrack>();
            }
            if (this.Track.Count == 0 || this.Information.tStartIndex == null)//初めの小節
            {
                //trk.AfterIndex = null;
                //trk.BeforeIndex = null;
                //trk.Number = this.Information.tNumber;
                this.Track.AddLast(trk);
                //this.Information.tStartIndex = 0;
                //this.Information.tEndIndex = 0;
                //this.Information.tCounter = 1;
                //this.Information.tNumber++;
                return;
            }

            //遅くなる原因になっているループ
            LinkedListNode<MIDITrack> pTrk = getStartTrackNode();
            while (true)
            {
                if (pTrk.Value.TrackNumber > trk.TrackNumber)
                {
                    pTrk = getPrevTrackNode(pTrk);
                    break;
                }
                LinkedListNode<MIDITrack> ppTrk = getNextTrackNode(pTrk);
                if (ppTrk == null) break;
                pTrk = ppTrk;
            }

            //trk.BeforeIndex = pTrk.Number;
            //trk.AfterIndex = pTrk.AfterIndex;
            //trk.Number = this.Information.tNumber;
            //pTrk.AfterIndex = trk.Number;
            LinkedListNode<MIDITrack> ntrk = this.Track.AddLast(trk);
            if (trk.AfterIndex == null)
            {
                this.Information.tEndIndex = this.Information.tNumber;
            }
            else
            {
                pTrk = getNextTrackNode(ntrk);
                //pTrk.BeforeIndex = trk.Number;
            }
            //this.Information.tCounter++;
            //this.Information.tNumber++;

        }

        //public void setTrackDevice4TrackNumber(int trkNumber, string devUserName, int devUserNumber, string devName, int? devNumber)
        //{
        //    LinkedListNode<MIDITrack> trk = getStartTrackNode();
        //    while (trk != null)
        //    {
        //        if (trk.Value.Number == trkNumber)
        //        {
        //            trk.Value.OutUserDeviceName = devUserName;
        //            trk.Value.OutUserDeviceNumber = devUserNumber;
        //            trk.Value.OutDeviceName = devName;
        //            trk.Value.OutDeviceNumber = devNumber;
        //            break;
        //        }
        //        trk = trk.Next;
        //    }
        //}

        public MIDITrack getTrack4TrackNumber(int trkNumber)
        {
            LinkedListNode<MIDITrack> trk = getStartTrackNode();
            while (trk != null)
            {
                if (trk.Value.TrackNumber == trkNumber)
                {
                    break;
                }
                trk = trk.Next;
            }

            return trk == null ? null : trk.Value;
        }

        public void SetSetting(Setting setting)
        {
            this.setting = setting;
        }

        public void Reinit()
        {
        }
    }

}
