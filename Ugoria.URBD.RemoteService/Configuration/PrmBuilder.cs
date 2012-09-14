using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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

        private PrmBuilder()
        {
        }        

        public FileInfo Build()
        {
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

            if (string.IsNullOrEmpty(fileName))
                fileName = Path.GetTempFileName();

            if (!File.Exists(logFile))
                File.Create(logFile);

            using (StreamWriter sw = new StreamWriter(new FileStream(fileName, FileMode.Create),
                Encoding.GetEncoding(1251)))
            {
                sw.WriteLine(prmData.ToString());
            }

            return new FileInfo(fileName);
        }

        public static PrmBuilder Create()
        {
            return new PrmBuilder();
        }
    }
}
