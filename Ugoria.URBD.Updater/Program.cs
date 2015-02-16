using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.Net;
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Ugoria.URBD.Updater
{
    class Program
    {
        static FtpKit ftpKit;
        static readonly string SERVICE_NAME = "Ugoria.URBD.RemoteService";
        static FileInfo logFile;
        static void Main(string[] args)
        {
            string username = null;
            string password = null;
            string ftpAddress = null;

            foreach (string arg in args)
            {
                if (arg.StartsWith("/u", StringComparison.InvariantCultureIgnoreCase))
                    username = arg.Substring(3);
                else if (arg.StartsWith("/p", StringComparison.InvariantCultureIgnoreCase))
                    password = arg.Substring(3);
                else if (arg.StartsWith("/a", StringComparison.InvariantCultureIgnoreCase))
                    ftpAddress = arg.Substring(3);
            }
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(ftpAddress))
                return;

            ftpKit = new FtpKit(ftpAddress, new NetworkCredential(username, password), 2, 100);

            Regex regexFilepath = new Regex(@"([\p{L}\p{N}-_\\.:)(\s]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            try
            {
                string servicePath = ServiceHelper.GetServicePath(SERVICE_NAME);
                string filePath = regexFilepath.Match(servicePath).Groups[0].Value;
                FileInfo fi = new FileInfo(filePath);
                logFile = new FileInfo(string.Format(@"{0}\urbd-logs\update_log_{1:yyyy-MM-dd_HHmmss}.log", fi.Directory.FullName, DateTime.Now));
                if (!logFile.Directory.Exists)
                    logFile.Directory.Create();

                ServiceHelper.StopService(SERVICE_NAME);

                if (CopyFiles(null, fi.Directory))
                {
                    WriteLog("Файлы успешно скопированы");
                    ServiceHelper.StartService(SERVICE_NAME);
                }
            }
            catch (Exception ex)
            {
                WriteLog("Не удалось обновить сервис. Причина: " + ex.Message);
            }
        }

        private static bool CopyFiles(Uri ftpPath, DirectoryInfo dirSource)
        {
            if (!dirSource.Exists)
                dirSource.Create();

            foreach (FtpEntry ftpEntry in ftpKit.GetListEntrys(ftpPath))
            {
                string entryLocalPath = String.Format("{0}/{1}", dirSource.FullName, ftpEntry.Name);
                if (ftpEntry.Type == FtpEntryType.Directory)
                    CopyFiles(new Uri(String.Format("{0}/{1}", ftpPath, ftpEntry.Name)), new DirectoryInfo(entryLocalPath));
                else if (ftpKit.DownloadFile(ftpEntry, entryLocalPath))
                    WriteLog("Скопирован файл " + entryLocalPath);
            }
            return true;
        }

        private static void WriteLog(string message)
        {
            string msg = String.Format("[{0:yyyy.MM.dd HH:mm:ss}] {1}", DateTime.Now, message);
            FileStream stream = null;
            if (!logFile.Exists)
            {
                stream = logFile.Create();
                logFile.Refresh();
            }
            else
                stream = logFile.Open(FileMode.Append, FileAccess.Write);
            using (StreamWriter sw = new StreamWriter(stream))
            {
                Console.WriteLine(msg);
                sw.WriteLine(msg);
            }
        }
    }
}
