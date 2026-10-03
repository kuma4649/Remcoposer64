using System;
using System.Collections.Generic;
using System.Text;

namespace Remcoposer64.Core
{
    public class MIDIDeviceList
    {
        public bool DevAlive = false;
        public int DevType = 0;
        public string DevToneMapFile1 = "";
        public string DevToneMapFile2 = "";
        public int? DevNumber = null;
        public string DevName = "";
        public int UsrNumber = -1;
        public string UsrName = "";
    }
}
