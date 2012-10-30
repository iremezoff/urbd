using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ugoria.URBD.Shared;
using System.Net;
using System.IO;
using System.Threading;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data.Reports;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using Ugoria.URBD.Shared.Configuration;

namespace Ugoria.URBD.RemoteService.CommandStrategy
{
    class ExchangeStrategy : ICommandStrategy
    {
        private IConfiguration configuration;
        private IModeStrategy modeStrategy;
        private Guid reportGuid;

        private Process process;
        private WebClient ftpClient;
        private DateTime releaseDate;

        private Guid launchGuid;
        private volatile bool isInterrupt = false;

        public bool IsInterrupt
        {
            get { return isInterrupt; }
        }

        public Guid LaunchGuid
        {
            get { return launchGuid; }
        }

        public void Launch(ReportAsyncCallback callback)
        {
            ReportBuilder reportBuilder = ReportBuilder.Create();
            reportBuilder.BaseName = configuration.GetParameter("base_name").ToString();
            reportBuilder.ReportGuid = reportGuid;
            LaunchReportBuilder lpBldr = LaunchReportBuilder.Create();
            OperationReportBuilder orBldr = OperationReportBuilder.Create();

            try
            {
                LogHelper.Write2Log("Запуск загрузки файлов", LogLevel.Information);
                PacketPathBuilder ppb = new PacketPathBuilder();
                ppb.FtpCP = configuration.GetParameter("ftp_cp").ToString();
                ppb.FtpPC = configuration.GetParameter("ftp_pc").ToString();
                ppb.BasePath = configuration.GetParameter("base_path").ToString();
                ppb.FtpAddress = configuration.GetParameter("ftp_address").ToString();

                List<Uri> loadPacketUris = new List<Uri>();
                foreach (IConfiguration packetConfig in (List<IConfiguration>)configuration.GetParameter("packets"))
                {
                    ppb.PacketName = packetConfig.GetParameter("filename").ToString();
                    if ((PacketType)packetConfig.GetParameter("type") != PacketType.Load)
                    {
                        // очистка ранее неудаленных пакетов выгрузки
                        FileInfo packetFile = new FileInfo(ppb.BuildUnloadPaths()[PacketPathBuilder.SOURCE].LocalPath);
                        if (packetFile.Exists)
                            packetFile.Delete();
                        continue;
                    }

                    Uri[] paths = ppb.BuildLoadPaths();
                    // Удаление ранее загруженных пакетов
                    FileInfo fi = new FileInfo(paths[PacketPathBuilder.DEST].OriginalString);
                    if (fi.Exists)
                        fi.Delete();
                    LogHelper.Write2Log(paths[PacketPathBuilder.SOURCE] + " -> " + paths[PacketPathBuilder.DEST].OriginalString, LogLevel.Information);
                    ExchangePacket(paths[PacketPathBuilder.SOURCE],
                        paths[PacketPathBuilder.DEST],
                        (int)configuration.GetParameter("packet_exchange_attempts"),
                        (int)configuration.GetParameter("packet_exchange_wait_time"));

                    loadPacketUris.Add(paths[PacketPathBuilder.SOURCE]);
                }
                LogHelper.Write2Log("Загрузка завершена", LogLevel.Information);

                bool launchRequired = true;
                while (launchRequired && !isInterrupt)
                {
                    process.Start();
                    launchGuid = Guid.NewGuid();
                    LogHelper.Write2Log(String.Format("Запущен процесс 1С {0}", launchGuid), LogLevel.Information);
                    lpBldr.Pid = process.Id;
                    lpBldr.StartDate = DateTime.Now;
                    lpBldr.Guid = launchGuid;

                    reportBuilder.ConcreteBuilder = lpBldr;
                    // отправка отчета о запуске процесса
                    callback(reportBuilder.Build());

                    process.WaitForExit();
                    LogHelper.Write2Log(String.Format("Процесс {0} отработал", launchGuid), LogLevel.Information);
                    launchRequired = !modeStrategy.CompleteExchange();
                }

                FileInfo mdFileInfo = new FileInfo((string)configuration.GetParameter("base_path") + @"\1cv7.md");
                // для устранения проблемы с погрешностью дат (считаем с точностью до секунды)
                if (Math.Abs((mdFileInfo.LastWriteTime - releaseDate).TotalSeconds) >= 1)
                {
                    orBldr.MDRelease = GetRelease(mdFileInfo.FullName);
                    orBldr.ReleaseDate = mdFileInfo.LastWriteTime;
                    LogHelper.Write2Log("Изменение версии MD: " + orBldr.MDRelease, LogLevel.Information);
                }

                LogHelper.Write2Log("Запуск выгрузки файлов", LogLevel.Information);
                foreach (IConfiguration packetCfg in (List<IConfiguration>)configuration.GetParameter("packets"))
                {
                    if ((PacketType)packetCfg.GetParameter("type") == PacketType.Load)
                    {
                        ppb.PacketName = packetCfg.GetParameter("filename").ToString();
                        Uri[] pathsLoad = ppb.BuildLoadPaths();
                        FileInfo packetFileLoad = new FileInfo(pathsLoad[PacketPathBuilder.DEST].LocalPath);
                        bool isSuccess = modeStrategy.Verifier.MlgReport.Any(r => r.Key.Equals(packetFileLoad.Name, StringComparison.InvariantCultureIgnoreCase) && r.Value.isSuccess && r.Value.type == PacketType.Load);
                        if (isSuccess)
                        {
                            orBldr.PacketList.Add(new ReportPacket
                            {
                                filename = packetCfg.GetParameter("filename").ToString(),
                                datePacket = packetFileLoad.CreationTime,
                                fileHash = ServiceUtil.GetFileMd5Hash(packetFileLoad.FullName),
                                fileSize = packetFileLoad.Length,
                                type = PacketType.Load
                            });
                        }
                        // подчищаем загруженные пакеты, если все загрузились успешно (не удаляем файлы по одному),
                        // т.к. повторная попытка (если состоится) загрузки потребует этих же файлов
                        packetFileLoad.Delete();
                        if (modeStrategy.IsSuccess)
                            DeletePacketOnFtp(pathsLoad[PacketPathBuilder.SOURCE]);
                        continue;
                    }
                    ppb.PacketName = packetCfg.GetParameter("filename").ToString();
                    Uri[] paths = ppb.BuildUnloadPaths();

                    LogHelper.Write2Log(paths[PacketPathBuilder.SOURCE].OriginalString + " -> " + paths[PacketPathBuilder.DEST], LogLevel.Information);
                    ExchangePacket(paths[PacketPathBuilder.SOURCE],
                        paths[PacketPathBuilder.DEST],
                        (int)configuration.GetParameter("packet_exchange_attempts"),
                        (int)configuration.GetParameter("packet_exchange_wait_time"));

                    FileInfo packetFile = new FileInfo(paths[PacketPathBuilder.SOURCE].LocalPath);
                    orBldr.PacketList.Add(new ReportPacket
                    {
                        datePacket = packetFile.CreationTime,
                        fileHash = ServiceUtil.GetFileMd5Hash(packetFile.FullName),
                        filename = packetCfg.GetParameter("filename").ToString(),
                        fileSize = packetFile.Length,
                        type = PacketType.Unload
                    });
                    packetFile.Delete(); // удаление пакета после выгрузки
                }
                LogHelper.Write2Log("Процесс выгрузки завершена", LogLevel.Information);
            }
            catch (WebException ex)
            {
                orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Message = "FTP: " + ex.Message;
                LogHelper.Write2Log(ex);
            }
            catch (FileNotFoundException ex)
            {
                orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Message = "FTP: " + ex.Message;
                LogHelper.Write2Log(ex);
            }
            catch (Win32Exception ex)
            {
                orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Message = "Запуск 1С: " + ex.Message;
                LogHelper.Write2Log(ex);
            }
            catch (Exception ex)
            {
                orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Message = ex.GetType() + ": " + ex.Message;
                LogHelper.Write2Log(ex);
            }
            if (isInterrupt)
            {
                orBldr.Status = ReportStatus.Interrupt;
                orBldr.Message = "Процесс прерван вручную. " + orBldr.Message;
            }
            else if (!modeStrategy.IsSuccess && !string.IsNullOrEmpty(modeStrategy.Message))
            {
                orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Message = modeStrategy.Message;
            }
            else if (orBldr.Status != ReportStatus.ExchangeFail && modeStrategy.IsSuccess && modeStrategy.IsWarning)
            {
                orBldr.Status = ReportStatus.ExchangeWarning;
                orBldr.Message = modeStrategy.Message;
            }
            else if (orBldr.Status != ReportStatus.ExchangeFail)
            {
                orBldr.Status = ReportStatus.ExchangeSuccess;
                orBldr.Message = modeStrategy.Message;
            }
            orBldr.CompleteDate = DateTime.Now;
            reportBuilder.ConcreteBuilder = orBldr;
            callback(reportBuilder.Build());
        }

        private static string GetRelease(string mdFilepath)
        {
            string release = null;
            int halfBufferSize = 5096;
            int readLength = 0;
            Encoding encoding = Encoding.GetEncoding(1251);
            Regex pattern = new Regex("\"Страхование. Конфигурация Югория\",\"(?:.[^\"]+)\",\"(?<1>.[^\"]+)\"");

            FileInfo tmpFileInfo = new FileInfo(Path.GetTempFileName());
            FileInfo mdFileInfo = new FileInfo(mdFilepath);
            mdFileInfo.CopyTo(tmpFileInfo.FullName, true);
            using (FileStream sr = new FileStream(tmpFileInfo.FullName, FileMode.Open))
            {
                byte[] buffer = new byte[halfBufferSize * 2];
                while ((readLength = sr.Read(buffer, 0, halfBufferSize * 2)) > 0)
                {
                    string str = encoding.GetString(buffer, 0, readLength);
                    if (readLength == halfBufferSize * 2)
                        sr.Seek(0 - halfBufferSize, SeekOrigin.Current); // перемещаем позицию на полбуфера назад, на случай, если искомая строка находится на пересечении считываемых блоков
                    Match match = pattern.Match(str);
                    if (!match.Success)
                        continue;
                    release = match.Groups[1].Value;
                    break;
                }
            }
            tmpFileInfo.Delete();
            return release;
        }

        private void ExchangePacket(Uri packetSource, Uri packetDest, int attempts, int waitTime)
        {
            while (attempts > 0)
            {
                try
                {
                    // WebClient не поддерживает локальные пути по формату Uri (file://[local_path])
                    if (packetSource.IsFile)
                    {
                        FileInfo uplFile = new FileInfo(packetSource.OriginalString);
                        if (!uplFile.Exists)
                        {
                            LogHelper.Write2Log("Файл пакета " + uplFile.Name + " отсутствует", LogLevel.Information);
                            //continue;
                        }
                        LogHelper.Write2Log(String.Format("Попытка загрузки файла {0}: {1}", packetDest.AbsoluteUri, attempts), LogLevel.Information);
                        ftpClient.UploadFile(string.IsNullOrEmpty(ftpClient.BaseAddress) ? packetDest.AbsoluteUri : packetDest.LocalPath, uplFile.FullName);
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(packetDest.AbsoluteUri);
                        request.Credentials = ftpClient.Credentials;
                        request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                        {
                            uplFile.CreationTime = response.LastModified;
                        }
                    }
                    else
                    {
                        FileInfo dnldFile = new FileInfo(packetDest.OriginalString);
                        LogHelper.Write2Log(String.Format("Попытка загрузки файла {0}: {1}", packetSource.AbsoluteUri, attempts), LogLevel.Information);
                        ftpClient.DownloadFile(string.IsNullOrEmpty(ftpClient.BaseAddress) ? packetSource.AbsoluteUri : packetSource.LocalPath, dnldFile.FullName);
                        // установка даты файла с FTP
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(packetSource.AbsoluteUri);
                        request.Credentials = ftpClient.Credentials;
                        request.Method = WebRequestMethods.Ftp.GetDateTimestamp;
                        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                        {
                            dnldFile.CreationTime = response.LastModified;
                        }
                    }
                    return;
                }
                catch (WebException ex)
                {
                    LogHelper.Write2Log(String.Format("Не удалось передать файл {0}", packetSource.OriginalString), LogLevel.Information);
                    attempts--;
                    if (attempts == 0)
                        throw new WebException(String.Format("передача {0} -> {1} (Ошибка: {2})", packetSource.OriginalString, packetDest.OriginalString, ex.Message), ex);
                    Thread.Sleep(new TimeSpan(0, waitTime, 0));
                }
            }
        }

        private void DeletePacketOnFtp(Uri packetUri)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(packetUri);
            request.Credentials = ftpClient.Credentials;
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            LogHelper.Write2Log(String.Format("Удаление файла {0}: {1}", packetUri, response.StatusDescription), LogLevel.Information);
            response.Close();
        }

        public void Interrupt()
        {
            try
            {
                if (launchGuid != Guid.Empty)
                    process.Kill();
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
            isInterrupt = true;
        }

        public ExchangeStrategy(IConfiguration configuration, Guid reportGuid, IModeStrategy modeStrategy, DateTime releaseDate)
        {
            this.configuration = configuration;
            this.modeStrategy = modeStrategy;
            this.reportGuid = reportGuid;
            this.releaseDate = releaseDate;

            this.process = new Process();
            process.StartInfo.FileName = (string)configuration.GetParameter("1c_path");
            process.StartInfo.Arguments = String.Format("config /D\"{0}\" /N {1} /P {2} /@\"{3}\"",
                configuration.GetParameter("base_path"),
                configuration.GetParameter("username"),
                configuration.GetParameter("password"),
                configuration.GetParameter("prm_path"));

            this.ftpClient = new WebClient();
            ftpClient.BaseAddress = (string)configuration.GetParameter("ftp_address");
            ftpClient.Proxy = null; // для предотвращения использования стандартных настроек прокси Windows
            ftpClient.Credentials = new NetworkCredential((string)configuration.GetParameter("ftp_username"), (string)configuration.GetParameter("ftp_password"));
        }
    }
}
