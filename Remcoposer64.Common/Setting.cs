using System.Text;
using System.Xml.Serialization;
using static Remcoposer64.Common.Setting;

namespace Remcoposer64.Common
{
    public class Setting
    {
        //public List<MIDIDeviceList> MIDIOutDeviceList { get; set; } = new();
        //public List<MIDIDeviceList> MIDIInDeviceList { get; set; } = new();

        public Log log = new Log();
        public MidiOut midiOut { get; set; } = new();
        public MidiIn midiIn { get; set; } = new();

        public Setting()
        {
            //MIDIOutDeviceList.Add(new MIDIDeviceList());
            //MIDIOutDeviceList.Add(new MIDIDeviceList());
            //MIDIOutDeviceList.Add(new MIDIDeviceList());
            //MIDIOutDeviceList.Add(new MIDIDeviceList());

            //MIDIInDeviceList.Add(new MIDIDeviceList());
            //MIDIInDeviceList.Add(new MIDIDeviceList());
            //MIDIInDeviceList.Add(new MIDIDeviceList());
            //MIDIInDeviceList.Add(new MIDIDeviceList());
        }

        public Setting Copy()
        {
            Setting setting = new Setting();
            setting.log = this.log.Copy();
            setting.midiOut = this.midiOut.Copy();
            setting.midiIn = this.midiIn.Copy();

            return setting;
        }

        public void Save()
        {
            try
            {
                string fullPath = Common.settingFilePath;
                fullPath = Path.Combine(fullPath, Common.cntSettingFileName);

                XmlSerializer serializer = new(typeof(Setting), typeof(Setting).GetNestedTypes());
                using StreamWriter sw = new(fullPath, false, new UTF8Encoding(false));
                serializer.Serialize(sw, this);
            }
            catch (Exception ex)
            {
                Remcoposer64.Common.Log.ForcedWrite(ex);
            }
        }

        public static Setting Load()
        {
            try
            {
                string fullPath = Common.settingFilePath;
                fullPath = Path.Combine(fullPath, Common.cntSettingFileName);

                if (!File.Exists(fullPath)) { return new Setting(); }
                XmlSerializer serializer = new(typeof(Setting), typeof(Setting).GetNestedTypes());
                using StreamReader sr = new(fullPath, new UTF8Encoding(false));

                Setting sett = (Setting)serializer.Deserialize(sr);

                return sett;
            }
            catch (Exception ex)
            {
                Remcoposer64.Common.Log.ForcedWrite(ex);
                return new Setting();
            }
        }



        [Serializable]
        public class Log
        {
            public int MAXLogLine { get; set; } = 5000;
            public int SkipCount { get; set; } = 1000;

            public Log Copy()
            {
                Log log = new Log();
                log.MAXLogLine = this.MAXLogLine;
                log.SkipCount = this.SkipCount;
                return log;
            }
        }

        [Serializable]
        public class MidiOut
        {
            public string GMReset { get; set; } = "30:F0,7E,7F,09,01,F7";
            public string XGReset { get; set; } = "30:F0,43,10,4C,00,00,7E,00,F7";
            public string GSReset { get; set; } = "30:F0,41,10,42,12,40,00,7F,00,41,F7";
            public string Custom { get; set; } = "";
            public int CurrentDev { get; set; } = 0;

            public List<midiOutInfo[]>? lstMidiOutInfo { get; set; } = null;

            public MidiOut Copy()
            {
                MidiOut MidiOut = new MidiOut();

                MidiOut.GMReset = this.GMReset;
                MidiOut.XGReset = this.XGReset;
                MidiOut.GSReset = this.GSReset;
                MidiOut.Custom = this.Custom;
                MidiOut.CurrentDev = this.CurrentDev;
                MidiOut.lstMidiOutInfo = null;
                if (this.lstMidiOutInfo != null)
                {
                    MidiOut.lstMidiOutInfo = new List<midiOutInfo[]>();
                    foreach (var item in this.lstMidiOutInfo)
                    {
                        midiOutInfo[] newItem = new midiOutInfo[item.Length];
                        for (int i = 0; i < item.Length; i++)
                        {
                            newItem[i] = item[i].Copy();
                        }
                        MidiOut.lstMidiOutInfo.Add(newItem);
                    }
                }

                return MidiOut;
            }

        }

        [Serializable]
        public class midiOutInfo
        {

            public int id = 0;
            public int manufacturer = -1;
            public string name = "";
            public int type = 0;//GM / XG / GS / LA / GS(SC - 55_1) / GS(SC - 55_2)

            public int beforeSendType = 0;//None / GM Reset / XG Reset / GS Reset / Custom
            public bool isVST = false;
            public string fileName = "";
            public string vendor = "";

            public midiOutInfo Copy()
            {
                midiOutInfo moi = new midiOutInfo();
                moi.id = this.id;
                moi.manufacturer = this.manufacturer;
                moi.name = this.name;
                moi.type = this.type;
                moi.beforeSendType = this.beforeSendType;
                moi.isVST = this.isVST;
                moi.fileName = this.fileName;
                moi.vendor = this.vendor;
                return moi;
            }
        }

        [Serializable]
        public class MidiIn
        {
            public List<midiInInfo[]>? lstMidiInInfo { get; set; } = null;
            public int CurrentDev { get; set; } = 0;

            public MidiIn Copy()
            {
                MidiIn MidiIn = new MidiIn();

                MidiIn.CurrentDev = this.CurrentDev;
                MidiIn.lstMidiInInfo = null;
                if (this.lstMidiInInfo != null)
                {
                    MidiIn.lstMidiInInfo = new List<midiInInfo[]>();
                    foreach (var item in this.lstMidiInInfo)
                    {
                        midiInInfo[] newItem = new midiInInfo[item.Length];
                        for (int i = 0; i < item.Length; i++)
                        {
                            newItem[i] = item[i].Copy();
                        }
                        MidiIn.lstMidiInInfo.Add(newItem);
                    }
                }

                return MidiIn;
            }

        }

        [Serializable]
        public class midiInInfo
        {

            public int id = 0;
            public int manufacturer = -1;
            public string name = "";
            public int type = 0;//GM / XG / GS / LA / GS(SC - 55_1) / GS(SC - 55_2)

            public int beforeSendType = 0;//None / GM Reset / XG Reset / GS Reset / Custom
            public bool isVST = false;
            public string fileName = "";
            public string vendor = "";

            public midiInInfo Copy()
            {
                midiInInfo mii = new midiInInfo();
                mii.id = this.id;
                mii.manufacturer = this.manufacturer;
                mii.name = this.name;
                mii.type = this.type;
                mii.beforeSendType = this.beforeSendType;
                mii.isVST = this.isVST;
                mii.fileName = this.fileName;
                mii.vendor = this.vendor;
                return mii;
            }
        }
    }
}
