using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.RemoteService
{
    class PrmBuilder
    {
        private string logFile = "";

        public string LogFile
        {
            get { return logFile; }
            set { logFile = value; }
        }
        private string fileName = "";

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        private PrmBuilder() { }

        public FileInfo Build()
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = Path.GetTempFileName();

            FileInfo logFileInfo = new FileInfo(logFile);
            if (!logFileInfo.Directory.Exists)
            {
                logFileInfo.Directory.Create();
                if (!logFileInfo.Exists)
                    using (logFileInfo.Create()) { }
            }

            FileInfo prmFileInfo = new FileInfo(fileName);
            if (prmFileInfo.Exists)
                prmFileInfo.Delete();
            StringBuilder prmData = new StringBuilder();
            prmData.AppendLine("[General]");

            if (!string.IsNullOrEmpty(logFile))
            {
                prmData.Append("Output = ");
                prmData.Append("\"");
                prmData.Append(logFile);
                prmData.AppendLine("\"");
            }
            prmData.AppendLine("Quit = 1");
            prmData.AppendLine("AutoExchange = 1");
            prmData.AppendLine("[AutoExchange]");
            prmData.AppendLine("SharedMode = 1");
            prmData.AppendLine("WriteTo = *");
            prmData.AppendLine("ReadFrom = *");

            using (StreamWriter sw = new StreamWriter(prmFileInfo.Create(), Encoding.GetEncoding(1251)))
            {
                sw.WriteLine(prmData.ToString());
            }
            return prmFileInfo;
        }

        public static PrmBuilder Create()
        {
            return new PrmBuilder();
        }
    }
}
