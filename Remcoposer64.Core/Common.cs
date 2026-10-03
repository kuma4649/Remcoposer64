using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Remcoposer64.Core
{
    public static class Common
    {
        public static readonly string AssemblyTitle = "Remcoposer64";
        public static readonly string cntSettingFileName = "Setting.xml";
        public static readonly string cntLogFilename = "log.txt";
        public static readonly string cntTimeFormat="yyMMddHHmmssfff";
        internal static readonly string cntExceptionFormat= "例外発生:\r\n- Type ------\r\n{0}\r\n- Message ------\r\n{1}\r\n- Source ------\r\n{2}\r\n- StackTrace ------\r\n{3}\r\n";
        internal static readonly string cntInnerExceptionFormat= "内部例外:\r\n- Type ------\r\n{0}\r\n- Message ------\r\n{1}\r\n- Source ------\r\n{2}\r\n- StackTrace ------\r\n{3}\r\n";
        internal static readonly string cntNoTitle="No title";
        internal static readonly string cntSMF0DefaultTrackName= "Channel {0}";
        internal static readonly string cntNullDevice= "Null Device";
        internal static readonly string cntSMF0DefaultPartName= "Channel {0} Part";
        internal static readonly string cntSMF1ConductorTrackName= "コンダクタートラック";
        internal static readonly string cntSMF1DefaultTrackName = "Track {0}";
        internal static readonly string cntSMF1DefaultPartName = "Track {0} Part";

        // 設定ファイルを置くディレクトリパス
        public static string settingFilePath = "";

        public static string GetApplicationDataFolder(bool make = false)
        {
            try
            {
                string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string fullPath;
                fullPath = System.IO.Path.Combine(appPath, "KumaApp", AssemblyTitle);
                if (!System.IO.Directory.Exists(fullPath)) System.IO.Directory.CreateDirectory(fullPath);

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        public static void SetExecutablePath(string executablePath)
        {
            try
            {
                string fn = Common.cntSettingFileName;
                if (File.Exists(Path.Combine(Path.GetDirectoryName(executablePath), fn)))
                {
                    //アプリケーションと同じフォルダに設定ファイルがあるならそちらを使用する
                    Common.settingFilePath = Path.GetDirectoryName(executablePath);
                }
                else
                {
                    //上記以外は、アプリケーション向けデータフォルダを使用する
                    Common.settingFilePath = Common.GetApplicationDataFolder(true);
                }
            }
            catch (Exception ex)
            {
                Log.ForcedWrite(ex);
                Common.settingFilePath = Common.GetApplicationDataFolder(true);
            }
        }
    }
}
