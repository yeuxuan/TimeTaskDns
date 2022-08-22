using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TimeTaskDns
{
    public enum LogLevel
    {
        Error,
        Debug,
        Info
    }

    internal static class Log
    {

        private static void WriteLog(LogLevel logLevel, params string[] args)
        {
            string logleve = "";
            switch (logLevel)
            {
                case LogLevel.Error:
                    logleve = "Error";
                    break;
                case LogLevel.Debug:
                    logleve = "Debug";
                    break;
                case LogLevel.Info:
                    logleve = "Info";
                    break;
                default:
                    break;
            }
            using (FileStream fileStream = new FileStream($@"D:\TimeTaskDns\{logleve}.txt", FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter sw = new StreamWriter(fileStream))
                {
                    sw.WriteLine($"[{DateTime.Now}]:{JsonConvert.SerializeObject(args)}");
                }
            }

        }
        public static void FullOutput(this LogLevel lvl, string title, params string[] logList)
        {
            var logContent = $"[{title}] " + string.Join(Environment.NewLine, logList);
            switch (lvl)
            {
                case LogLevel.Error:
                    WriteLog(lvl, logContent);
                    break;
                case LogLevel.Debug:
                    WriteLog(lvl, logContent);
                    break;
                case LogLevel.Info:
                    WriteLog(lvl, logContent);
                    break;
            }
        }
    }
}
