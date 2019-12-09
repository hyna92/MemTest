using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DefineLib
{
    //public class EventLog
    //{
    //    public int? BoardNum;
    //    public string EventTime;
    //    public string EventAction;
    //    public string EventData;
    //    public bool EventType;
    //
    //    public EventLog(int? boardnum, string action, string data, bool type = true)
    //    {
    //        BoardNum = boardnum;
    //        EventTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
    //        EventAction = action;
    //        EventData = data;
    //        EventType = type;
    //    
    //        try
    //        {
    //            string FileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd"));//string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMdd"), StaticDefine.FILE_LOG);
    //            string FilePath = string.Format("{0}\\{1}", StaticDefine.PATH_LOG, FileName);
    //
    //            if (!Directory.Exists(StaticDefine.PATH_LOG))
    //                Directory.CreateDirectory(StaticDefine.PATH_LOG);
    //
    //            if (!File.Exists(FilePath))
    //                File.AppendAllText(FilePath, "Time\t\t\tExcept\tBoard\tEvent\t\tData\r\n", System.Text.Encoding.Default);
    //
    //            string ExceptionCheck = string.Empty;
    //    
    //            if(!EventType)
    //                ExceptionCheck = "e";
    //    
    //            string LogContents = string.Format("[{0}]\t{1}\t{2}\t{3}\t{4}\r\n", EventTime, ExceptionCheck, BoardNum, EventAction, EventData);
    //        
    //            File.AppendAllText(FilePath, LogContents, System.Text.Encoding.Default);
    //        }
    //        catch
    //        {
    //            //new ExceptionLog();
    //        }
    //    }
    //}


    //public class ExceptionLog
    //{
    //    public string ExceptionTime;
    //    public int ExceptionType;
    //    public string ExceptionFunc;
    //    public string ExceptionData;
    //
    //    public ExceptionLog(int exceptype, string excepfunc, string excepdata)
    //    {
    //        ExceptionTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
    //        ExceptionType = exceptype;
    //        ExceptionFunc = excepfunc;
    //        ExceptionData = excepdata;
    //
    //        try
    //        {
    //            string FileName = string.Format("{0}_{1}", DateTime.Now.ToString("yyyyMMdd"), StaticDefine.FILE_LOG_EXCEPTION);
    //            string FilePath = string.Format("{0}\\{1}", StaticDefine.PATH_LOG, FileName);
    //
    //            if (!Directory.Exists(StaticDefine.PATH_LOG))
    //                Directory.CreateDirectory(StaticDefine.PATH_LOG);
    //
    //            string LogContents = string.Format("{0}\t{1}\t{2} - {3}\r\n", ExceptionTime, ExceptionType, ExceptionFunc, ExceptionData);
    //
    //            File.AppendAllText(FilePath, LogContents, System.Text.Encoding.Default);
    //        }
    //        catch
    //        {
    //
    //        }
    //    }
    //}
}
