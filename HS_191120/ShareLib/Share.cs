using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DefineLib;

namespace ShareLib
{
    public class Share                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
    {
        public bool shutdown = false;

        private static Share Instance = null;
        public static Share Initialize
        {
            get
            {
                if (Instance == null)
                {
                    Instance = new Share();
                }
                return Instance;
            }
        }

        public int SettingBoardColumnCount;
        public int SettingBoardRowCount;


        public Dictionary<int, BoardInfo> BoardInfoDic = new Dictionary<int, BoardInfo>();
        
        public List<string> SerialPortList = new List<string>();


        public void EventLog(int? boardnum, string action, string data, bool type = true)
        {
            int? BoardNum = boardnum;
            string EventTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
            string EventAction = action;
            string EventData = data;
            bool EventType = type;

            int FileCount = 0;

            try
            {
                if (!Directory.Exists(StaticDefine.PATH_LOG))
                    Directory.CreateDirectory(StaticDefine.PATH_LOG);

                // FileSize Over 10MByte ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                string FileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd"));//string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMdd"), StaticDefine.FILE_LOG);
                FileInfo FileSize = new FileInfo(FileName);

                if (FileSize.Exists)
                {
                    long bytes = FileSize.Length;

                    if (bytes <= 0)
                        return;
                    else if (bytes > 10 * 1024 * 1024) // 10 Mbytes
                    {
                        FileCount++;
                        FileName = string.Format("{0}_{1}.txt", DateTime.Now.ToString("yyyyMMdd"), FileCount);
                    }
                }

                ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                string FilePath = string.Format("{0}\\{1}", StaticDefine.PATH_LOG, FileName);

                if (!File.Exists(FilePath))
                    File.AppendAllText(FilePath, "Time\t\t\tExcept\tBoard\tEvent\t\tData\r\n", System.Text.Encoding.Default);

                string ExceptionCheck = string.Empty;

                if (!EventType)
                    ExceptionCheck = "e";

                string LogContents = string.Format("[{0}]\t{1}\t{2}\t{3}\t{4}\r\n", EventTime, ExceptionCheck, BoardNum, EventAction, EventData);

                File.AppendAllText(FilePath, LogContents, System.Text.Encoding.Default);
            }
            catch
            {
                //new ExceptionLog();
            }
        }
    }
}
