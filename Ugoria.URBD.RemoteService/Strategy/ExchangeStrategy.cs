using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Ionic.Zip;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.RemoteService.Kit;
using Ugoria.URBD.RemoteService.Strategy.Exchange.Mode;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Contracts.Handlers.Strategy.Exchange.Mode;

namespace Ugoria.URBD.RemoteService.Strategy
{
    public class ExchangeStrategy : ICommandStrategy
    {
        private ExchangeContext context;

        public ExchangeContext Context
        {
            get { return context; }
        }

        private FtpKit ftpKit;
        private Process1CLauncherKit processKit;
        private NetworkConnection netConn;

        private bool isComplete = false;
        public bool IsComplete
        {
            get { return isComplete; }
        }

        private volatile bool isInterrupt = false;
        public bool IsInterrupt
        {
            get { return isInterrupt; }
        }

        public void Interrupt()
        {
            isInterrupt = true;
            try
            {
                if (context.LaunchGuid != Guid.Empty)
                {
                    processKit.Interrupt();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
        }

        public ExchangeStrategy(ExchangeContext context)
        {
            this.context = context;

            NetworkCredential credential = new NetworkCredential(context.Username, context.Password);
            this.ftpKit = new FtpKit(context.FtpAddress, credential);
            ftpKit.Attempts = context.FtpAttemptCount;
            ftpKit.WaitTime = context.FtpDelayTime;

            this.processKit = new Process1CLauncherKit(context.Path1C, context.User1C, context.Password1C, context.BasePath, LaunchMode.Config);
            this.processKit.PrmFile = context.PrmFile;
            this.processKit.Username = SecureHelper.ConvertUserdomainToClassic(context.Username);
            this.processKit.Password = SecureHelper.ConvertToSecureString(context.Password);
        }

        private bool MDInclusion(string filepath)
        {
            using (ZipFile zipFile = new ZipFile(filepath))
            {
                return zipFile.Any(f => f.FileName.EndsWith("md", StringComparison.InvariantCultureIgnoreCase));
            }
        }

        public void StartLaunch()
        {
            try
            {
                int pid = processKit.BeginStart(null);
                context.StartTime = DateTime.Now;
                context.LaunchGuid = Guid.NewGuid();
                context.Pid = pid;
                LogHelper.Write2Log(String.Format("Запущен процесс 1С {0} для ИБ {1}", context.LaunchGuid, context.Command.baseName), LogLevel.Information);
            }
            catch (Win32Exception ex)
            {
                LogHelper.Write2Log(ex);
                throw new URBDException("Запуск 1С: " + ex.Message, ex);
            }
        }

        public bool EndLaunch()
        {
            try
            {
                processKit.EndStart(null);
                LogHelper.Write2Log(String.Format("Процесс {0} для ИБ {1} отработал. ", context.LaunchGuid, context.Command.baseName), LogLevel.Information);

                return context.Mode.CompleteExchange(context.HaveMD);
            }
            catch (Win32Exception ex)
            {
                LogHelper.Write2Log(ex);
                throw new URBDException("Запуск 1С: " + ex.Message, ex);
            }
            catch (IOException ex)
            {
                LogHelper.Write2Log(ex);
                Thread.Sleep(new TimeSpan(0, 0, 30)); // ожидание 30 секунд для возможности освобождения файла логов
            }
            return false;
        }

        private static string GetReleaseNumber(string mdFilePath)
        {
            Regex pattern = new Regex("\"Страхование. Конфигурация Югория\",\"(?:.[^\"]+)\",\"(?<1>.[^\"]+)\""); //?: - не включать в группу, ?<1> - группа 1, .[^\"] - любой символ за исключением кавычки, которая будет 1 раз
            try
            {
                string release = null;
                int halfBufferSize = 16384;
                int readLength = 0;
                Encoding encoding = Encoding.GetEncoding(1251);

                FileInfo tmpFileInfo = new FileInfo(Path.GetTempFileName());
                File.Copy(mdFilePath, tmpFileInfo.FullName, true);
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
            catch (IOException ex)
            {
                LogHelper.Write2Log(ex);
                throw new URBDException("Проверка релиза: " + ex.Message, ex);
            }
        }

        public void Conclusion()
        {
            FileInfo mdFileInfo = new FileInfo(context.BasePath + @"\1cv7.md");
            // для устранения проблемы с погрешностью дат при занесении в БД и извлечении в центральном сервисе (считаем с точностью до секунды)

            if (Math.Abs((mdFileInfo.LastWriteTime - context.DateRelease).TotalSeconds) >= 1)
            {
                context.MDRelease = GetReleaseNumber(mdFileInfo.FullName);
                context.DateRelease = mdFileInfo.LastWriteTime;
                LogHelper.Write2Log(String.Format("Изменение версии MD на {0} у ИБ {1}", context.MDRelease, context.Command.baseName), LogLevel.Information);
            }

            PacketPathBuilder ppb = new PacketPathBuilder();
            ppb.FtpCP = context.FtpCenterDir;
            ppb.FtpPC = context.FtpPeripheryDir;
            ppb.BasePath = context.BasePath;
            ppb.FtpAddress = context.FtpAddress;

            LogHelper.Write2Log("Запуск выгрузки файлов ИБ и сбор информации" + context.Command.baseName, LogLevel.Information);

            try
            {
                DefaultMode mode = (DefaultMode)context.Mode;
                foreach (ReportPacket reportPacket in context.Packets)
                {
                    FileInfo packetFileInfo = null;
                    Uri ftpPacketUri = null;
                    ppb.PacketName = reportPacket.filename;
                    if (reportPacket.type == PacketType.Load)
                    {
                        Uri[] pathsLoad = ppb.BuildLoadPaths();
                        packetFileInfo = new FileInfo(pathsLoad[PacketPathBuilder.DEST].LocalPath);
                        ftpPacketUri = pathsLoad[PacketPathBuilder.SOURCE];
                        if (context.HasDeletePacket)
                            ftpKit.DeleteFile(ftpPacketUri);
                    }
                    else
                    {
                        Uri[] pathsUnload = ppb.BuildUnloadPaths();
                        packetFileInfo = new FileInfo(pathsUnload[PacketPathBuilder.SOURCE].LocalPath);
                        ftpPacketUri = pathsUnload[PacketPathBuilder.DEST];
                        ftpKit.UploadFile(new Uri(packetFileInfo.FullName), ftpPacketUri);
                    }
                    bool isSuccessProcessed = mode.Verifier.MlgReport.Any(r => r.Key.Equals(packetFileInfo.Name, StringComparison.InvariantCultureIgnoreCase) && r.Value.isSuccess && r.Value.type == reportPacket.type);
                    if (isSuccessProcessed)
                    {
                        packetFileInfo.LastWriteTime = ftpKit.GetFileDateTime(ftpPacketUri); // синхронизация времени изменения пакета на ftp и в КИБе
                        reportPacket.datePacket = packetFileInfo.LastWriteTime;
                        reportPacket.fileSize = packetFileInfo.Length;
                    }
                    packetFileInfo.Delete(); // удаление пакета после всех телодвижений
                }
                LogHelper.Write2Log("Процесс выгрузки и сбора информации завершен", LogLevel.Information);
                isComplete = mode.IsSuccess; // пройден весь жизненный цикл стратегии
            }
            catch (WebException ex)
            {
                LogHelper.Write2Log(ex);
                throw new URBDException("Выгрузка на FTP: " + ex.Message, ex);
            }
            finally
            {
                if (netConn != null)
                    netConn.Dispose();
            }
        }

        public void Prepare()
        {
            if (new Uri(context.BasePath).IsUnc)
                this.netConn = new NetworkConnection(context.BasePath, new NetworkCredential(context.Username, context.Password));
            LogHelper.Write2Log("Запуск загрузки файлов", LogLevel.Information);
            PacketPathBuilder ppb = new PacketPathBuilder();
            ppb.FtpCP = context.FtpCenterDir;
            ppb.FtpPC = context.FtpPeripheryDir;
            ppb.BasePath = context.BasePath;
            ppb.FtpAddress = context.FtpAddress;

            context.HaveMD = false;
            try
            {
                foreach (ReportPacket reportPacket in context.Packets)
                {
                    if (isInterrupt)
                        break;
                    ppb.PacketName = reportPacket.filename;
                    if (reportPacket.type != PacketType.Load)
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
                    LogHelper.Write2Log(fi.FullName, LogLevel.Information);
                    if (fi.Exists)
                        fi.Delete();
                    if (!ftpKit.DownloadFile(paths[PacketPathBuilder.SOURCE], paths[PacketPathBuilder.DEST]))
                        continue;
                    fi.LastWriteTime = ftpKit.GetFileDateTime(paths[PacketPathBuilder.SOURCE]);
                    context.HaveMD |= MDInclusion(fi.FullName); // есть ли MD                    
                }
                LogHelper.Write2Log("Загрузка завершена", LogLevel.Information);

                if (context.HaveMD)
                    LogHelper.Write2Log(context.Command.baseName + ": загрузка с MD", LogLevel.Information);
            }
            catch (WebException ex)
            {
                LogHelper.Write2Log(ex);
                throw new URBDException(string.Format("Загрузка с FTP: {0} ({1})", ex.Message, ex.Response!=null?ex.Response.ResponseUri.ToString():string.Empty), ex);
            }
            catch (ZipException ex)
            {
                LogHelper.Write2Log(ex);
                throw new URBDException("Проверка Zip-архива: " + ex.Message, ex);
            }
        }
    }
}
