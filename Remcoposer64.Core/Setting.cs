using System.Text;
using System.Xml.Serialization;

namespace Remcoposer64.Core
{
    public class Setting
    {
        public List<MIDIDeviceList> MIDIOutDeviceList { get; set; } = new();
        public List<MIDIDeviceList> MIDIInDeviceList { get; set; } = new();

        public class Log
        {
            public int MAXLogLine { get; set; } = 5000;
            public int SkipCount { get; set; } = 1000;

        }
        public Log log = new Log();

        public Setting()
        {
            MIDIOutDeviceList.Add(new MIDIDeviceList());
            MIDIOutDeviceList.Add(new MIDIDeviceList());
            MIDIOutDeviceList.Add(new MIDIDeviceList());
            MIDIOutDeviceList.Add(new MIDIDeviceList());

            MIDIInDeviceList.Add(new MIDIDeviceList());
            MIDIInDeviceList.Add(new MIDIDeviceList());
            MIDIInDeviceList.Add(new MIDIDeviceList());
            MIDIInDeviceList.Add(new MIDIDeviceList());
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
            catch(Exception ex)
            {
                Core.Log.ForcedWrite(ex);
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
                Core.Log.ForcedWrite(ex);
                return new Setting();
            }
        }


    }
}
