using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Ugoria.URBD.Shared;
using System.Collections.Concurrent;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Shared.Reporting;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.CentralService.Logging;
using Ugoria.URBD.CentralService.CommandBuilding;

namespace Ugoria.URBD.CentralService
{
    class RemoteServicesManager
    {
        private string uriPattern = "net.tcp://{0}/URBDRemoteService";
        //private string centralAddr = "net.tcp://localhost:8000/URBDCentralService";

        private ChannelFactory<IRemoteService> channelFactory = null;
        private ServiceHost serviceHost;
        private ILogger logger = null;
        private IReporter reporter = null;
        private IAlarmer alarmer = null;
        private CentralConfigurationManager configurationManager;
        private IConfiguration centralServiceConguration;
        private ConcurrentDictionary<int, EndpointAddress> serviceBases = new ConcurrentDictionary<int, EndpointAddress>();

        public CentralConfigurationManager ConfigurationManager
        {
            get { return configurationManager; }
        }

        public IReporter Reporter
        {
            get { return reporter; }
            set { reporter = value; }
        }

        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public IAlarmer Alarmer
        {
            get { return alarmer; }
            set { alarmer = value; }
        }

        public RemoteServicesManager(CentralConfigurationManager configurationManager = null)
        {
            this.configurationManager = configurationManager;
            Init();
        }

        private void Init()
        {
            centralServiceConguration = configurationManager.GetCentralServiceConfiguration();

            CentralService centralService = new CentralService();
            channelFactory = new ChannelFactory<IRemoteService>(new NetTcpBinding(SecurityMode.None));

            centralService.RemoteRequestConfiguration += RemoteRequestConfiguartion;
            centralService.RemoteNoticePID1C += RemoteNoticePID1C;
            centralService.RemoteNoticeReport += RemoteNoticeReport;

            serviceHost = new ServiceHost(centralService);


            if (configurationManager != null)
            {
                IConfiguration centralConguration = configurationManager.GetCentralServiceConfiguration();

                Uri centralUri = new Uri((string)centralConguration.GetParameter("service_central_address"));
                serviceHost.AddServiceEndpoint(typeof(ICentralService),
                    new NetTcpBinding(SecurityMode.None),
                    centralUri);
            }
        }

        private RemoteConfiguration RemoteRequestConfiguartion(ICentralService sender, RequestConfigureEventArgs args)
        {
            LogHelper.Write2Log("Запрос конфигурации для сервиса " + args.Uri, LogLevel.Information);

            // получить конфигурацию для удаленного сервиса (ftp, путь до 1с, список баз)
            string remoteAddress = String.Format("{0}:{1}", args.Uri.Host, args.Uri.Port);

            IConfiguration remoteServiceConfiguration = configurationManager.GetRemoteServiceConfiguration(remoteAddress);
            //IConfiguration centralServiceConguration = configurationManager.GetCentralServiceConfiguration();

            RemoteConfiguration remoteConfiguration = new RemoteConfiguration
            {
                ftpAddress = (string)centralServiceConguration.GetParameter("ftp_address"),
                ftpUsername = (string)centralServiceConguration.GetParameter("ftp_username"),
                ftpPassword = (string)centralServiceConguration.GetParameter("ftp_password"),
                threadsCount = int.Parse((string)centralServiceConguration.GetParameter("max_threads")),
                extFormsPath = (string)centralServiceConguration.GetParameter("extforms_path"),
                file1CPath = (string)remoteServiceConfiguration.GetParameter("1c_path"),
                ftpCP = (string)centralServiceConguration.GetParameter("ftp_cp"),
                ftpPC = (string)centralServiceConguration.GetParameter("ftp_pc"),
                waitTime = int.Parse(centralServiceConguration.GetParameter("wait_time").ToString()),
                packetExchangeAttempts = int.Parse(centralServiceConguration.GetParameter("packet_exchange_attempts").ToString()),
                packetExchangeWaitTime = int.Parse(centralServiceConguration.GetParameter("packet_exchange_wait_time").ToString())
            };

            foreach (IConfiguration baseConfiguration in (List<IConfiguration>)remoteServiceConfiguration.GetParameter("bases"))
            {
                Base base1c = new Base();
                base1c.baseId = (int)baseConfiguration.GetParameter("base_id");
                base1c.baseName = (string)baseConfiguration.GetParameter("base_name");
                base1c.basePath = (string)baseConfiguration.GetParameter("1c_database");
                base1c.username = (string)baseConfiguration.GetParameter("1c_username");
                base1c.password = (string)baseConfiguration.GetParameter("1c_password");

                foreach (IConfiguration packetConfiguration in (List<IConfiguration>)baseConfiguration.GetParameter("packets"))
                {
                    base1c.packetList.Add(new Packet
                    {
                        filename = (string)packetConfiguration.GetParameter("filename"),
                        type = "L".Equals((string)packetConfiguration.GetParameter("type")) ? PacketType.Load : PacketType.Unload
                    });
                }
                remoteConfiguration.baseList.Add(base1c);
            }

            return remoteConfiguration;
        }

        private void RemoteNoticePID1C(ICentralService sender, NoticePID1CArgs args)
        {
            if (reporter == null)
                return;

            LogHelper.Write2Log(String.Format("Запущен автообмен процессом 1С pid: {0}", args.LaunchReport.pid), LogLevel.Information);

            reporter.SetPID1C(args.LaunchReport);

            if (logger != null)
                logger.Information(args.Uri, String.Format("Запущен автообмен процессом 1С pid: {0}", args.LaunchReport.pid));
        }

        private void RemoteNoticeReport(ICentralService sender, NoticeReportArgs args)
        {
            if (reporter == null)
                return;

            LogHelper.Write2Log(String.Format("Пришел отчет с сервиса {0}", args.Uri), LogLevel.Information);

            OperationReport report = args.Report;

            reporter.SetReport(args.Report);

            switch (report.status)
            {
                case ReportStatus.Interrupt:
                    if (logger != null)
                        logger.Information(args.Uri, "Задача для ИБ " + report.baseName + " была прервана");
                    break;
                case ReportStatus.ExchangeSuccess:
                    if (logger != null)
                        logger.Information(args.Uri, "Обмен пакетами базы " + report.baseName + " прошел успешно");
                    break;
                case ReportStatus.ExchangeFail:
                    if (logger != null)
                        logger.Warning(args.Uri, "При обмене пакетами базы " + report.baseName + " возникла ошибка: " + report.message);
                    if (alarmer != null)
                        alarmer.Alarm(args.Uri, "Обмен не прошел. База: " + report.baseName);
                    break;
                case ReportStatus.ExchangeWarning:
                    if (logger != null)
                        logger.Warning(args.Uri, "При попытке обмена пакетами" + report.baseName + " получено предупржедение: " + report.message);
                    break;
                case ReportStatus.ExtFormsSuccess:
                    if (logger != null)
                        logger.Information(args.Uri, "Загрузка ExtForms для базы " + report.baseName + " прошла успешно");
                    break;
                case ReportStatus.ExtFormsFail:
                    if (logger != null)
                        logger.Warning(args.Uri, "При загрузке ExtForms для базы " + report.baseName + " возникла ошибка: " + report.message);
                    if (alarmer != null)
                        alarmer.Alarm(args.Uri, "Загрузка ExtForms не прошла. База: " + report.baseName);
                    break;
            }
        }

        public void AddBaseService(string addr, int baseId)
        {
            EndpointAddress endpoint = new EndpointAddress(new Uri(String.Format(uriPattern, addr)));
            serviceBases.AddOrUpdate(baseId, endpoint, (key, oVal) => endpoint);
        }

        public void SendCommand(CommandBuilder commandBuilder, int userId)
        {
            ReportInfo reportInfo = reporter.GetLastCommand(commandBuilder.BaseId);
            if (!serviceBases.ContainsKey(commandBuilder.BaseId))
            {
                LogHelper.Write2Log("Не удалось найти сервис для ИБ " + reportInfo.BaseName, LogLevel.Error);
                return;
            }

            EndpointAddress remoteServiceEndpoint = serviceBases[commandBuilder.BaseId];

            // предыдущая задача не завершена
            if (reportInfo != null && reportInfo.CompleteDate == DateTime.MinValue)
            {
                logger.Fail(remoteServiceEndpoint.Uri, String.Format("Запрос для ИБ {1} отклонен, т.к. предыдущий процесс не завершен",
                    commandBuilder.Description,
                    reportInfo.BaseName));
                return;
            }

            commandBuilder.ReleaseUpdate = reportInfo.ReleaseDate;
            commandBuilder.ConfigurationChangeDate = reportInfo.ConfigurationChangeDate;

            Command command = commandBuilder.Build();

            using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, remoteServiceEndpoint))
            {
                proxy.CommandExecute(command);
                string message = String.Format("Запрос на {0}.\nИБ: {1}.",
                    commandBuilder.Description,
                    command.baseId);
                if (proxy.IsSuccess)
                {
                    reporter.SetCommandReport(new Report { baseId = commandBuilder.BaseId, dateCommand = command.commandDate, reportGuid = command.reportGuid }, userId);
                    logger.Information(remoteServiceEndpoint.Uri, message);
                }
                else
                {
                    RemoteServiceError(remoteServiceEndpoint, proxy.Exception);
                    LogHelper.Write2Log(proxy.Exception);
                    // в случае сбоя сервиса следующий код не нужен, отчеты только для БД
                    /*RemoteNoticeReport(null, new NoticeReportArgs(new OperationReport
                    {
                        baseName = command.baseName,
                        dateCommand = command.commandDate,
                        dateComplete = DateTime.Now,
                        reportGuid = command.guid,
                        status = command.commandType == CommandType.Exchange ? ReportStatus.ExchangeFail : ReportStatus.ExtFormsFail, 
                        message = "Сервис недоступен"
                    },
            remoteServiceEndpoint.Uri));*/
                }
            }
        }

        private IRemoteService GetChannel(Uri address)
        {
            IRemoteService remoteService = channelFactory.CreateChannel(new EndpointAddress(address));
            return remoteService;
        }

        private IRemoteService GetChannel(string address)
        {
            Uri uri = new Uri(String.Format(uriPattern, address));
            return GetChannel(uri);
        }

        private IRemoteService GetChannel(string address, string name)
        {
            Uri uri = new Uri(String.Format(uriPattern, address));
            return GetChannel(uri);
        }

        private void RemoteServiceError(EndpointAddress endpointAddr, Exception exp)
        {
            Uri address = endpointAddr.Uri;

            if (exp is FaultException)
            {
                if (logger != null)
                    logger.Warning(address, "Произошла ошибка на стороне удаленного сервиса: " + exp.Message);
                if (alarmer != null)
                    alarmer.Alarm(address, exp.Message);
            }
            else if (exp is CommunicationException)
            {
                if (logger != null)
                    logger.Fail(address, "Соединение с сервисом потеряно: " + exp.Message);
                // оповещение о неработе сервиса
                if (alarmer != null)
                    alarmer.Alarm(address, "Проблема в работе сервиса " + address);
            }
            else
            {
                if (logger != null)
                    logger.Fail(address, "Прочие ошибки: " + exp.Message);
                // оповещение о неработе сервиса
                if (alarmer != null)
                    alarmer.Alarm(address, "Проблема в работе сервиса " + address);
            }
        }

        public void CheckProcess(CommandBuilder commandBuilder)
        {
            // запрос процессов неотработавших задач
            ReportInfo reportInfo = reporter.GetLastCommand(commandBuilder.BaseId);

            // Не проверяем завершенные задачи
            if (reportInfo.CompleteDate != DateTime.MinValue)
                return;

            CheckCommand checkCommand = new CheckCommand
            {
                launchGuid = reportInfo.LaunchGuid,
                reportGuid = reportInfo.ReportGuid
            };

            string alarmMessage = "";

            if (!serviceBases.ContainsKey(commandBuilder.BaseId))
            {
                LogHelper.Write2Log("Не удалось найти сервис для ИБ " + reportInfo.BaseName, LogLevel.Error);
                return;
            }

            EndpointAddress endpointAddr = serviceBases[commandBuilder.BaseId];
            if (reportInfo.StartDate != DateTime.MinValue)
            {
                int wait = (int)(reportInfo.StartDate - DateTime.Now).Add(new TimeSpan(0, int.Parse(centralServiceConguration.GetParameter("delay_check").ToString()), 0)).TotalMilliseconds;
                Thread.Sleep(wait < 0 ? 0 : wait);
                using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, endpointAddr))
                {
                    RemoteProcessStatus status = proxy.CheckProcess(checkCommand);

                    switch (status)
                    {
                        case RemoteProcessStatus.Miss: alarmMessage = "Процесс {0} для ИБ \"{1}\" был утерян. Время запуска процесса: {2:dd.MM.yyyy hh:mm:ss}"; break;
                        case RemoteProcessStatus.LongProcess: alarmMessage = "Процесс {0} для ИБ \"{1}\" выполняется свыше установленного периода ожидания. Время запуска процесса: {2:dd.MM.yyyy hh:mm:ss}"; break;
                        case RemoteProcessStatus.ServiceFail: alarmMessage = "Служба УРБД после запуска {0} для ИБ \"{1}\" была аварийно завершена и потеряла связь с процессом. Время запуска процесса: {2:dd.MM.yyyy hh:mm:ss}"; break;
                        case RemoteProcessStatus.UnknownFail: alarmMessage = "Неизестный сбой процесса {0} для ИБ \"{1}\". Время запуска процесса: {2:dd.MM.yyyy hh:mm:ss}"; break;
                    }
                    if (!string.IsNullOrEmpty(alarmMessage))
                    {
                        alarmMessage = String.Format(alarmMessage,
                            commandBuilder.Description,
                            reportInfo.BaseName,
                            reportInfo.StartDate);
                    }
                    if (!proxy.IsSuccess)
                        RemoteServiceError(endpointAddr, proxy.Exception);
                    LogHelper.Write2Log("Проверка работы процесса 1С " + reportInfo.ReportGuid + ": " + status, LogLevel.Information);
                }
            }
            else if (reportInfo.StartDate == null)
            {
                using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, endpointAddr))
                {
                    RemoteProcessStatus status = proxy.CheckProcess(checkCommand);
                    switch (status)
                    {
                        case RemoteProcessStatus.LongStart: alarmMessage = "Процесс на {0} для ИБ \"{1}\" не был запущен в течение установленного периода ожидания."; break;
                        case RemoteProcessStatus.ServiceFail: alarmMessage = "Служба УРБД после запуска на {0} для ИБ \"{1}\" была аварийно завершена и процесс не был запущен."; break;
                    }
                    if (!string.IsNullOrEmpty(alarmMessage))
                    {
                        alarmMessage = String.Format(alarmMessage,
                            commandBuilder.Description,
                            reportInfo.BaseName);
                    }
                    if (!proxy.IsSuccess)
                        RemoteServiceError(endpointAddr, proxy.Exception);
                    LogHelper.Write2Log("Проверка работы процесса 1С " + reportInfo.ReportGuid + ": " + status, LogLevel.Information);
                }
            }
            if (alarmer != null && !string.IsNullOrEmpty(alarmMessage))
                alarmer.Alarm(reportInfo.ServiceAddress, alarmMessage);
            if (logger != null && !string.IsNullOrEmpty(alarmMessage))
                logger.Warning(reportInfo.ServiceAddress, alarmMessage);
        }

        public void Start()
        {
            serviceHost.Open();
        }

        public void Stop()
        {
            serviceHost.Close();
        }

        public void InterruptCommand(int baseId)
        {
            ReportInfo reportInfo = reporter.GetLastCommand(baseId);

            // нельзя снять отработанную задачу
            if (reportInfo.CompleteDate != DateTime.MinValue)
                return;
            EndpointAddress remoteServiceEndpoint = serviceBases[baseId];
            using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, remoteServiceEndpoint))
            {
                proxy.InterruptProcess(reportInfo.ReportGuid);

                if (!proxy.IsSuccess)
                    RemoteServiceError(remoteServiceEndpoint, proxy.Exception);
            }
        }
    }
}
