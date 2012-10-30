using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data;
using System.IO;
using Ugoria.URBD.Contracts.Services;
using System.Text.RegularExpressions;

namespace Ugoria.URBD.RemoteService
{
    public class Verifier
    {
        private string logFilename = "";
        private string message = "";
        private Dictionary<string, PacketInfo> mlgReport = new Dictionary<string, PacketInfo>();

        public Dictionary<string, PacketInfo> MlgReport
        {
            get { return mlgReport; }
        }

        private Regex regexFilepath = new Regex(@"'([a-z-_\\0-9.:\s]+)'", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);

        public virtual void Verification()
        {
            string currentFile = null;

            using (StreamReader read = new StreamReader(logFilename, Encoding.GetEncoding(1251)))
            {
                string line = "";
                while ((line = read.ReadLine()) != null)
                {
                    MLGMessage mlgMessage = BuildMlgMessage(line);
                    if (mlgMessage == null)
                        continue;
                    switch (mlgMessage.eventType)
                    {
                        case "DistUplBeg":
                            currentFile = new FileInfo(mlgMessage.information).Name;
                            if (!mlgReport.Any(r => r.Key.Equals(currentFile, StringComparison.InvariantCultureIgnoreCase)))
                                mlgReport.Add(currentFile, new PacketInfo { isSuccess = false, type = PacketType.Load });
                            break;
                        case "DistDnldBeg":
                            currentFile = new FileInfo(regexFilepath.Match(mlgMessage.information).Groups[1].Value).Name;
                            if (!mlgReport.Any(r => r.Key.Equals(currentFile, StringComparison.InvariantCultureIgnoreCase)))
                                mlgReport.Add(currentFile, new PacketInfo { isSuccess = false, type = PacketType.Unload });
                            break;
                        case "DistUplSuc":
                            mlgReport[currentFile].isSuccess = true;
                            break;
                        case "DistDnldSuc":
                            mlgReport[currentFile].isSuccess = true;
                            break;
                        case "DistUplErr":
                            if (mlgReport[currentFile].isSuccess)
                                break;
                            mlgReport[currentFile].status = String.Format("Ошибка загрузки пакета {0}: {1}", currentFile, mlgMessage.information);
                            break;
                        case "DistDnlErr":
                            mlgReport[currentFile].status = String.Format("Ошибка выгрузки пакета: {0}: {1} ", currentFile, mlgMessage.information);
                            break;
                    }
                }
            }
            return;
        }

        internal Verifier(string logFilename)
        {
            this.logFilename = logFilename;
            try
            {
                FileInfo fi = new FileInfo(logFilename);
                if (fi.Exists)
                {
                    fi.Delete();
                    fi.Create().Close();
                }
            }
            catch (Exception) { }
        }

        private static MLGMessage BuildMlgMessage(string line)
        {
            MLGMessage mlgMessage = new MLGMessage();
            //20070728;13:59:48;Администратор;C;Distr;DistDnldBeg;1;Код ИБ: 'СП ', Файл: 'F:\Base_urbd_sql\SPB\CP\SPB_C.zip';;
            string[] messArr = line.Split(new char[] { ';' });
            if (!"Distr".Equals(messArr[4])) // пропуск записей, не связанных с обменом
                return null;
            mlgMessage.eventDate = DateTime.ParseExact(messArr[0] + messArr[1], "yyyyMMddHH:mm:ss", null);
            mlgMessage.eventType = messArr[5];
            mlgMessage.information = messArr[7];
            return mlgMessage;
        }
    }

    public class PacketInfo
    {
        public string status = "";
        public bool isSuccess = false;
        public PacketType type;
    }
}
