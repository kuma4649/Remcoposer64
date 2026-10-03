using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    public class MIDIRythm
    {
        private string _Name = "";
        private int _Key = 0;
        private int _Gt = 1;
        public string Name
        {
            set
            {
                _Name = value;
            }
            get
            {
                return _Name;
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
        public int Gt
        {
            set
            {
                _Gt = value;
            }
            get
            {
                return _Gt;
            }
        }
    }
}
