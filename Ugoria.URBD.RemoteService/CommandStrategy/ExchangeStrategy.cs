using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Ugoria.URBD.Core;
using Ugoria.URBD.Contracts;
using System.Net;
using System.IO;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Ugoria.URBD.Contracts.Service;

namespace Ugoria.URBD.RemoteService
{
    class ExchangeStrategy : ICommandStrategy
    {
        private IConfiguration configuration;
        private IModeStrategy modeStrategy;
        private bool withMD = false;
        private Guid reportGuid;

        private Process process;
        private WebClient ftpClient;

        private Guid launchGuid;

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
                PacketPathBuilder ppb = new PacketPathBuilder();
                ppb.PCPath = configuration.GetParameter("pc_path").ToString();
                ppb.CPPath = configuration.GetParameter("cp_path").ToString();
                ppb.BasePath = configuration.GetParameter("base_path").ToString();
                ppb.FtpAddress = configuration.GetParameter("ftp_address").ToString();

                List<string> loadFiles = new List<string>();
                foreach (IConfiguration loadPacket in (List<IConfiguration>)configuration.GetParameter("packets"))
                {
                    if ((PacketType)loadPacket.GetParameter("type") != PacketType.Load)
                        continue;
                    ppb.PacketName = loadPacket.GetParameter("path").ToString();
                    string[] paths = ppb.BuildLoadPaths();

                    ExchangePacket(paths[PacketPathBuilder.SOURCE],
                        paths[PacketPathBuilder.DEST],
                        ftpClient,
                        (int)configuration.GetParameter("packet_exchange_attempts"),
                        (int)configuration.GetParameter("packet_exchange_wait_time"));
                    loadFiles.Add(paths[PacketPathBuilder.DEST]);
                    bool haveMD = MDInclusion(paths[PacketPathBuilder.DEST]);
                    if (haveMD && !withMD)
                    {
                        orBldr.Status = ReportStatus.ExchangeWarning;
                        orBldr.Message = String.Format("Запуск без MD. Пакет {0} содержит MD. Операция обмена отложена", ppb.PacketName);
                        reportBuilder.ConcreteBuilder = orBldr;
                        callback(reportBuilder.Build());
                        return;
                    }
                }

                bool isComplete = false;
                while (!isComplete)
                {
                    process.Start();
                    launchGuid = Guid.NewGuid();
                    lpBldr.Pid = process.Id;
                    lpBldr.StartDate = DateTime.Now;
                    lpBldr.Guid = launchGuid;

                    reportBuilder.ConcreteBuilder = lpBldr;
                    // отправка отчета о запуске процесса
                    callback(reportBuilder.Build());

                    //process.WaitForExit();
                    Thread.Sleep(900000);
                    process.Kill();
                    isComplete = modeStrategy.Verification();
                }

                if (!modeStrategy.IsVerified) orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Messages.AddRange(modeStrategy.Messages);
                orBldr.Message = modeStrategy.Status;
                orBldr.CompleteDate = DateTime.Now;

                loadFiles.ForEach(f => File.Delete(f)); // удаление загруженных пакетов

                foreach (IConfiguration loadPacket in (List<IConfiguration>)configuration.GetParameter("packets"))
                {
                    if ((PacketType)loadPacket.GetParameter("type") != PacketType.Unload)
                        continue;

                    ppb.PacketName = loadPacket.GetParameter("path").ToString();
                    string[] paths = ppb.BuildUnloadPaths();
                    ExchangePacket(paths[PacketPathBuilder.SOURCE],
                        paths[PacketPathBuilder.DEST],
                        ftpClient,
                        (int)configuration.GetParameter("packet_exchange_attempts"),
                        (int)configuration.GetParameter("packet_exchange_wait_time"));
                    File.Delete(paths[PacketPathBuilder.SOURCE]); // удаление пакета после выгрузки
                }
            }
            catch (Exception ex)
            {
                orBldr.Status = ReportStatus.ExchangeFail;
                orBldr.Message = ex.Message;
            }
            reportBuilder.ConcreteBuilder = orBldr;
            callback(reportBuilder.Build());
        }

        private static bool MDInclusion(string filename)
        {
            using (ZipInputStream s = new ZipInputStream(File.OpenRead(filename)))
            {
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    if ("1cv7.md".Equals(theEntry.Name.ToLower()))
                        return true;
                }
            }
            return false;
        }

        private static void ExchangePacket(string packetSource, string packetDest, WebClient client, int attempts, int waitTime)
        {
            bool isComplete = false;
            while (!isComplete)
            {
                try
                {
                    if (packetSource.Contains("ftp"))
                        client.DownloadFile(packetSource, packetDest);
                    else
                        client.UploadFile(packetSource, packetDest);
                    isComplete = true;
                }
                catch (WebException)
                {
                    attempts--;
                    if (attempts == 0)
                        throw;
                    Thread.Sleep(new TimeSpan(0, waitTime, 0));
                }
            }
        }

        public ExchangeStrategy(IConfiguration configuration, Guid reportGuid, IModeStrategy modeStrategy, bool withMD)
        {
            this.configuration = configuration;
            this.withMD = withMD;
            this.modeStrategy = modeStrategy;
            this.reportGuid = reportGuid;

            this.process = new Process();
            process.StartInfo.FileName = (string)configuration.GetParameter("1c_path");
            process.StartInfo.Arguments = String.Format("config /D\"{0}\" /N {1} /P {2} /@{3}",
                configuration.GetParameter("base_path"),
                configuration.GetParameter("username"),
                configuration.GetParameter("password"),
                configuration.GetParameter("prm_path"));

            this.ftpClient = new WebClient();
            ftpClient.BaseAddress = (string)configuration.GetParameter("ftp_address");
            ftpClient.Proxy = null; // для предотвращения использования стандартных настроек прокси Windows
            ftpClient.Credentials = new NetworkCredential(configuration.GetParameter("ftp_username").ToString(),
                configuration.GetParameter("ftp_password").ToString());
        }
    }
}
