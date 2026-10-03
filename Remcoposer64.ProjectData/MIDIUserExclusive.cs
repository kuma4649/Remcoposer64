using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.ProjectData
{
    public class MIDIUserExclusive
    {
        private string _Name = "";
        private string _Memo = "";
        private byte[] _Exclusive = null;
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
        public byte[] Exclusive
        {
            set
            {
                _Exclusive = value;
            }
            get
            {
                return _Exclusive;
            }
        }
    }
}
