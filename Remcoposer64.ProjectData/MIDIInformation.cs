using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    [Serializable]
    public class MIDIInformation
    {
        private string _FileName = "";
        private string _Title = "";
        private string _Writer = "";
        private string _Composer = "";
        private string _Arranger = "";
        private string _Copyright = "";
        private string _Memo = "";
        private int _Key = 0;
        private int _PlayBIAS = 0;
        private string _ControlFileCM6 = "";
        private string _ControlFileGSD = "";
        private string _ControlFileGSD2 = "";
        private DateTime _CreationDate = DateTime.MinValue;
        private DateTime _UpdatedDate = DateTime.MinValue;
        private int _TimeBase = 480;
        private int _Tempo = 120;
        private int _BeatDen = 4; //拍子（分母）
        private int _BeatMol = 4; //拍子（分子）
        private List<int> _Marker = new List<int>();
        private int? _StartTrackIndex = null;
        private int? _EndTrackIndex = null;
        private int _NumberTrack = 0;
        private int _tCounter = 0;

        public string FileFullName { get; set; } = "";
        public string FileName
        {
            set
            {
                _FileName = value;
            }
            get
            {
                return _FileName;
            }
        }
        public string Title
        {
            set
            {
                _Title = value;
            }
            get
            {
                return _Title;
            }
        }
        public string Writer
        {
            set
            {
                _Writer = value;
            }
            get
            {
                return _Writer;
            }
        }
        public string Composer
        {
            set
            {
                _Composer = value;
            }
            get
            {
                return _Composer;
            }
        }
        public string Arranger
        {
            set
            {
                _Arranger = value;
            }
            get
            {
                return _Arranger;
            }
        }
        public string Copyright
        {
            set
            {
                _Copyright = value;
            }
            get
            {
                return _Copyright;
            }
        }
        public string Memo
        {
            set
            {
                _Memo = value;
            }
            get
            {
                return _Memo;
            }
        }
        public int Key
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
        public int PlayBIAS
        {
            set
            {
                _PlayBIAS = value;
            }
            get
            {
                return _PlayBIAS;
            }
        }
        public string ControlFileCM6
        {
            set
            {
                _ControlFileCM6 = value;
            }
            get
            {
                return _ControlFileCM6;
            }
        }
        public string ControlFileGSD
        {
            set
            {
                _ControlFileGSD = value;
            }
            get
            {
                return _ControlFileGSD;
            }
        }
        public string ControlFileGSD2
        {
            set
            {
                _ControlFileGSD2 = value;
            }
            get
            {
                return _ControlFileGSD2;
            }
        }
        public DateTime CreationDate
        {
            set
            {
                _CreationDate = value;
            }
            get
            {
                return _CreationDate;
            }
        }
        public DateTime UpdatedDate
        {
            set
            {
                _UpdatedDate = value;
            }
            get
            {
                return _UpdatedDate;
            }
        }
        public int TimeBase
        {
            set
            {
                _TimeBase = value;
            }
            get
            {
                return _TimeBase;
            }
        }
        public int Tempo
        {
            set
            {
                _Tempo = value;
            }
            get
            {
                return _Tempo;
            }
        }
        public int BeatDen//拍子（分母）
        {
            set
            {
                _BeatDen = value;
            }
            get
            {
                return _BeatDen;
            }
        }
        public int BeatMol
        {
            set
            {
                _BeatMol = value;
            }
            get
            {
                return _BeatMol;
            }
        }
        public List<int> Marker
        {
            set
            {
                _Marker = value;
            }
            get
            {
                return _Marker;
            }
        }
        public int? tStartIndex
        {
            set
            {
                _StartTrackIndex = value;
            }
            get
            {
                return _StartTrackIndex;
            }
        }
        public int? tEndIndex
        {
            set
            {
                _EndTrackIndex = value;
            }
            get
            {
                return _EndTrackIndex;
            }
        }
        public int tNumber
        {
            set
            {
                _NumberTrack = value;
            }
            get
            {
                return _NumberTrack;
            }
        }
        public int tCounter
        {
            set
            {
                _tCounter = value;
            }
            get
            {
                return _tCounter;
            }
        }

        public bool Played { get; set; } = false;
    }
}
