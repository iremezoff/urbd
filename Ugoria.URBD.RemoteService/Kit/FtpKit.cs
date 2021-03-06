﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Ugoria.URBD.Shared;
using System.Globalization;

namespace Ugoria.URBD.RemoteService.Kit
{
    class FtpKit
    {
        private WebClient ftpClient;

        private int waitTime = 1;

        public int WaitTime
        {
            get { return waitTime; }
            set { waitTime = value; }
        }

        private int attempts = 3;

        public int Attempts
        {
            get { return attempts; }
            set { attempts = value; }
        }

        public FtpKit(string address, NetworkCredential credential)
        {
            this.ftpClient = new WebClient();
            ftpClient.BaseAddress = address;
            ftpClient.Credentials = credential;
            ftpClient.Proxy = null;
        }

        public FtpKit(string address, NetworkCredential credential, int waitTime)
            : this(address, credential)
        {
            this.waitTime = waitTime;
        }

        public FtpKit(string address, NetworkCredential credential, int waitTime, int attempts)
            : this(address, credential, waitTime)
        {
            this.attempts = attempts;
        }

        private bool ExchangeFile(Uri source, Uri destination)
        {
            int unitAttempts = attempts;
            while (true)
            {
                try
                {
                    if (source.IsFile)
                    {
                        LogHelper.Write2Log(String.Format("Попытка выгрузки файла {0}: {1}", destination.AbsoluteUri, unitAttempts), LogLevel.Information);
                        ftpClient.UploadFile(string.IsNullOrEmpty(ftpClient.BaseAddress) ? destination.AbsoluteUri : destination.LocalPath, source.OriginalString);
                    }
                    else
                    {
                        LogHelper.Write2Log(String.Format("Попытка загрузки файла {0}: {1}", source.AbsoluteUri, unitAttempts), LogLevel.Information);
                        ftpClient.DownloadFile(string.IsNullOrEmpty(ftpClient.BaseAddress) ? source.AbsoluteUri : source.LocalPath, destination.OriginalString);
                    }
                    return true;
                }
                catch (WebException ex)
                {
                    LogHelper.Write2Log(String.Format("Не удалось передать файл {0}", source.OriginalString), LogLevel.Information);
                    unitAttempts--;
                    if (unitAttempts == 0)
                        throw new WebException(String.Format("передача {0} -> {1} (Ошибка: {2})", source.OriginalString, destination.OriginalString, ex.Message), ex);
                    Thread.Sleep(new TimeSpan(0, waitTime, 0));
                }
            }
        }

        public List<FtpEntry> GetListEntrys(Uri ftpPath = null)
        {
            string respStr = null;
            List<FtpEntry> ftpEntrys = new List<FtpEntry>();
            try
            {
                using (FtpWebResponse response = GetResponse(ftpPath ?? new Uri(ftpClient.BaseAddress), WebRequestMethods.Ftp.ListDirectoryDetails))
                {
                    using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1251)))
                    {
                        respStr = sr.ReadToEnd();
                    }
                }
            }
            catch (WebException ex) // файл не найден или иная проблема при запросе
            {
                LogHelper.Write2Log(ex);
                return ftpEntrys;
            }

            string[] entrys = respStr.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string entry in entrys)
            {
                string[] detail = entry.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);
                if ("<DIR>".Equals(detail[2]))
                {
                    ftpEntrys.Add(new FtpEntry { Type = FtpEntryType.Directory, Uri = new Uri(String.Format("{0}/{1}", ftpPath, detail[3].Trim())), Name = detail[3].Trim() });
                }
                else
                {
                    DateTime modifiedDate = DateTime.ParseExact(detail[0] + " " + detail[1], "MM-dd-yy hh:mmtt", CultureInfo.InvariantCulture);
                    long fileSize = long.Parse(detail[2]);
                    string name = detail[3].Trim();
                    ftpEntrys.Add(new FtpEntry
                    {
                        Type = FtpEntryType.File,
                        Name = name,
                        Uri = ftpPath.AbsolutePath.EndsWith(name)
                            ? ftpPath
                            : new Uri(String.Format("{0}/{1}", ftpPath, name)),
                        Size = fileSize,
                        CreatedTime = modifiedDate
                    });
                }
            }
            return ftpEntrys;
        }

        public bool DownloadFile(Uri source, string destination)
        {
            return DownloadFile(source, new Uri(destination));
        }

        public bool DownloadFile(string source, string destination)
        {
            return DownloadFile(new Uri(source), new Uri(destination));
        }

        public bool DownloadFile(Uri source, Uri destination)
        {
            List<FtpEntry> ftpEntrys = GetListEntrys(source);
            if (ftpEntrys.Count == 0)
            {
                LogHelper.Write2Log("По адресу " + source + " ничего нет", LogLevel.Information);
                return false;
            }
            return DownloadFile(ftpEntrys[0], destination.OriginalString);
        }

        public bool DownloadFile(FtpEntry source, string destination)
        {
            FileInfo dnldFile = new FileInfo(destination);
            if (dnldFile.Exists && (source.CreatedTime - dnldFile.LastWriteTime).TotalSeconds < 1 && dnldFile.Length == source.Size)
            {
                return false;
            }
            else if (dnldFile.Exists)
                dnldFile.Delete();
            LogHelper.Write2Log(source.Uri + " -> " + destination, LogLevel.Information);
            return ExchangeFile(source.Uri, new Uri(dnldFile.FullName));
        }

        public bool UploadFile(Uri source, Uri destination)
        {
            FileInfo uplFile = new FileInfo(source.OriginalString);
            if (!uplFile.Exists)
            {
                LogHelper.Write2Log(string.Format("Файл {0} отсутствует", uplFile.Name), LogLevel.Information);
                return false;
            }
            return ExchangeFile(new Uri(uplFile.FullName), destination);
        }

        private FtpWebResponse GetResponse(Uri requestUri, string method)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(requestUri);
            request.Credentials = ftpClient.Credentials;
            request.Proxy = null; // работа в локальной сети компании, прокси ни к чему
            request.Method = method;// WebRequestMethods.Ftp.GetDateTimestamp;
            return (FtpWebResponse)request.GetResponse();
        }

        public void DeleteFile(Uri destination)
        {
            using (FtpWebResponse response = GetResponse(destination, WebRequestMethods.Ftp.DeleteFile))
            {
                LogHelper.Write2Log(String.Format("Статус удаления файла {0}: {1}", destination, response.StatusDescription), LogLevel.Information);
            }
        }

        public DateTime GetFileDateTime(Uri destination)
        {
            using (FtpWebResponse response = GetResponse(destination, WebRequestMethods.Ftp.GetDateTimestamp))
            {
                return response.LastModified;
            }
        }
    }
}
