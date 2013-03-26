using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Ugoria.URBD.CentralService.Logging;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Handlers;
using System.Security;
using System.Collections;
using System.Reflection;
using Ugoria.URBD.CentralService.Alarming;

namespace Ugoria.URBD.CentralService
{
    class RemoteServicesManager
    {
        public event Action<ExecuteCommand> CommandSended;
        public event Action<Report> ReportReceived;
        private string uriPattern = "net.tcp://{0}:{1}/URBDRemoteService";
        //private string centralAddr = "net.tcp://localhost:8000/URBDCentralService";

        private ChannelFactory<IRemoteService> channelFactory = null;
        private ServiceHost serviceHost;
        private ILogger logger = null;
        //private IReporter reporter = null;
        private WCFMessageHandler handler;

        private IAlarmer alarmer = null;
        private CentralConfigurationManager configurationManager;
        private Hashtable centralServiceConguration;
        private ConcurrentDictionary<int, EndpointAddress> serviceBases = new ConcurrentDictionary<int, EndpointAddress>();
        private object lockConfManager = new object();

        public WCFMessageHandler Handler
        {
            get { return handler; }
            set { handler = value; }
        }

        public CentralConfigurationManager ConfigurationManager
        {
            get { return configurationManager; }
            set
            {
                lock (lockConfManager)
                {
                    configurationManager = value;
                    if (serviceHost != null && serviceHost.State == CommunicationState.Opened)
                        serviceHost.Close();
                    Stop();
                    Init();
                    Start();
                }
            }
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

        public RemoteServicesManager(CentralConfigurationManager configurationManager)
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

            Uri centralUri = new Uri((string)centralServiceConguration["main.service_central_address"]);
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            binding.MaxReceivedMessageSize = 1024 * 1024;
            serviceHost.AddServiceEndpoint(typeof(ICentralService),
                binding,
                centralUri);
        }

        private RemoteConfiguration RemoteRequestConfiguartion(ICentralService sender, RequestConfigureEventArgs args)
        {
            LogHelper.Write2Log("Запрос конфигурации для сервиса " + args.Uri, LogLevel.Information);

            // получить конфигурацию для удаленного сервиса (ftp, путь до 1с, список баз)
            string remoteAddress = String.Format("{0}:{1}", args.Uri.Host, args.Uri.Port);

            Hashtable remoteServiceConfiguration = null;
            lock (lockConfManager)
            {
                remoteServiceConfiguration = configurationManager.GetRemoteServiceConfiguration(remoteAddress);
                //IConfiguration centralServiceConguration = configurationManager.GetCentralServiceConfiguration();
            }
            RemoteConfiguration remoteConfiguration = new RemoteConfiguration { configuration = new Hashtable(centralServiceConguration), bases = new Dictionary<int, Hashtable>() };
            remoteConfiguration.configuration.Add("service.1c_path", remoteServiceConfiguration["service.1c_path"]);

            foreach (Hashtable baseHash in (List<Hashtable>)remoteServiceConfiguration["base_list"])
            {
                remoteConfiguration.bases.Add((int)baseHash["base.base_id"], baseHash);
            }
            return remoteConfiguration;
        }

        private void RemoteNoticePID1C(ICentralService sender, NoticePID1CArgs args)
        {
            LogHelper.Write2Log(String.Format("На сервисе {0} запущен автообмен процессом 1С pid: {1}", args.Uri, args.LaunchReport.pid), LogLevel.Information);

            if (handler != null)
                handler.HandleReport(args.LaunchReport);
            if (ReportReceived != null)
                ReportReceived(args.LaunchReport);
            if (logger != null)
                logger.Information(args.Uri, String.Format("Запущен автообмен процессом 1С pid: {0}", args.LaunchReport.pid));
        }

        private void RemoteNoticeReport(ICentralService sender, NoticeReportArgs args)
        {
            LogHelper.Write2Log(String.Format("Пришел отчет {0} с сервиса {1}", args.Report.GetType().Name, args.Uri), LogLevel.Information);

            if (handler == null)
                return;
            handler.HandleReport(args.Report);
            ReportStatus status = args.Report.status;

            if (ReportReceived != null)
                ReportReceived(args.Report);

            string alarmMesage = "Получен отчет о выполнении операции<br/><br/><b>Тип отчета:</b> {0}<br/><b>ИБ:</b> {1}<br/><b>Статус:</b> {2}<br/><b>Время завершения:</b> {3:HH:mm:ss dd.MM.yyyy}";

            try
            {
                switch (status)
                {
                    case ReportStatus.Success:
                        if (logger != null)
                            logger.Information(args.Uri, String.Format("Получен отчет {0} об успешном выполнении операции на ИБ {1}", args.Report.GetType().Name, args.Report.baseName));
                        if (alarmer != null)
                            alarmer.Alarm(args.Report.reportGuid, String.Format(alarmMesage, args.Report.GetType().Name, args.Report.baseName, "успешно", args.Report.dateComplete));
                        break;
                    case ReportStatus.Warning:
                    case ReportStatus.Interrupt:
                        if (logger != null)
                            logger.Warning(args.Uri, String.Format("Получен отчет {0} с предупреждением во время выполнения операции на ИБ {1}", args.Report.GetType().Name, args.Report.baseName));
                        if (alarmer != null)
                            alarmer.Alarm(args.Report.reportGuid, String.Format(alarmMesage, args.Report.GetType().Name, args.Report.baseName, "есть предупреждения", args.Report.dateComplete));
                        break;
                    case ReportStatus.Fail:
                        if (logger != null)
                            logger.Warning(args.Uri, String.Format("Получен отчет {0} с ошибкой во время выполнения операции на ИБ {1}", args.Report.GetType().Name, args.Report.baseName));
                        if (alarmer != null)
                            alarmer.Alarm(args.Report.reportGuid, String.Format(alarmMesage, args.Report.GetType().Name, args.Report.baseName, "есть ошибки", args.Report.dateComplete));
                        break;
                    case ReportStatus.Critical:
                        if (logger != null)
                            logger.Warning(args.Uri, String.Format("Получен отчет {0} с серьезным сбоем во время выполнения операции на ИБ {1}", args.Report.GetType().Name, args.Report.baseName));
                        if (alarmer != null)
                            alarmer.Alarm(args.Report.reportGuid, String.Format(alarmMesage, args.Report.GetType().Name, args.Report.baseName, "серьезный сбой", args.Report.dateComplete));
                        break;
                }
            }
            catch (AlarmException ex)
            {
                if (logger != null)
                    logger.Fail(args.Uri, ex.Message);
            }
        }

        public void AddBaseService(string addr, int baseId)
        {
            string[] addrArr = addr.Split(new char[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            EndpointAddress endpoint = new EndpointAddress(new Uri(
                String.Format(uriPattern,
                    addrArr[0],
                    string.IsNullOrEmpty(addrArr[1]) ? "7000" : addrArr[1])));
            serviceBases.AddOrUpdate(baseId, endpoint, (key, oVal) => endpoint);
        }

        public void SendCommand(ExecuteCommand command)
        {
            SendCommand(command, 1); // cron
        }

        public void SendCommand(ExecuteCommand command, int userId)
        {
            ExecuteCommand preparedCommand = handler.PrepareCommand(command);

            if (!serviceBases.ContainsKey(command.baseId))
            {
                LogHelper.Write2Log("Не удалось найти сервис для ИБ " + command.baseName, LogLevel.Error);
                return;
            }

            EndpointAddress remoteServiceEndpoint = serviceBases[command.baseId];

            // предыдущая задача не завершена
            if (preparedCommand.commandDate == DateTime.MinValue)
            {
                logger.Fail(remoteServiceEndpoint.Uri, String.Format("Команда {0} для ИБ {1} отклонена, т.к. предыдущий процесс не завершен", command.GetType(), command.baseName));
                return;
            }

            using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, remoteServiceEndpoint))
            {
                proxy.CommandExecute(preparedCommand);
                string message = String.Format("Отправка команды {0}.\nИБ: {1}", command.GetType(), command.baseName);
                if (proxy.IsSuccess)
                {
                    handler.SetCommandReport(preparedCommand, userId);
                    if (CommandSended != null)
                        CommandSended(preparedCommand);
                    if (logger != null)
                        logger.Information(remoteServiceEndpoint.Uri, message);
                }
                else
                {
                    RemoteServiceError(command, proxy.Exception);
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

        private void RemoteServiceError(Command command, Exception exp)
        {
            Uri address = serviceBases[command.baseId].Uri;

            if (exp is FaultException)
            {
                if (logger != null)
                    logger.Warning(address, "Произошла ошибка на стороне удаленного сервиса: " + exp.Message);
                if (alarmer != null)
                    alarmer.Alarm(command.reportGuid, exp.Message);
            }
            else if (exp is CommunicationException)
            {
                if (logger != null)
                    logger.Fail(address, "Соединение с сервисом потеряно: " + exp.Message);
                // оповещение о неработе сервиса
                if (alarmer != null)
                    alarmer.Alarm(command.reportGuid, "Проблема в работе сервиса " + address);
            }
            else
            {
                if (logger != null)
                    logger.Fail(address, "Прочие ошибки: " + exp.Message);
                // оповещение о неработе сервиса
                if (alarmer != null)
                    alarmer.Alarm(command.reportGuid, "Проблема в работе сервиса " + address);
            }
        }

        public void CheckProcess(ExecuteCommand command)
        {
            // запрос процессов неотработавших задач
            LaunchReport launchReport = handler.GetLaunchReport(command);

            // Не проверяем завершенные задачи
            if (launchReport == null)
                return;
            CheckCommand checkCommand = new CheckCommand
            {
                reportGuid = launchReport.reportGuid,
                launchGuid = launchReport.launchGuid,
                baseId = command.baseId,
                baseName = command.baseName,
                commandDate = launchReport.commandDate,
                configurationChangeDate = handler.PrepareCommand(command).configurationChangeDate
            };

            string alarmMessage = string.Empty;

            if (!serviceBases.ContainsKey(command.baseId))
            {
                LogHelper.Write2Log("Не удалось найти сервис для ИБ " + launchReport.baseName, LogLevel.Error);
                return;
            }

            EndpointAddress endpointAddr = serviceBases[launchReport.baseId];
            if (launchReport.startDate != DateTime.MinValue)
            {
                int wait = (int)(launchReport.startDate - DateTime.Now).Add(new TimeSpan(0, int.Parse((string)centralServiceConguration["main.delay_check"]), 0)).TotalMilliseconds;
                Thread.Sleep(wait < 0 ? 0 : wait);
                using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, endpointAddr))
                {
                    RemoteProcessStatus status = proxy.CheckProcess(checkCommand);

                    switch (status)
                    {
                        case RemoteProcessStatus.Miss: alarmMessage = "Процесс {0} для ИБ \"{1}\" был утерян. Время запуска процесса: {2:HH:mm:ss dd.MM.yyyy}"; break;
                        case RemoteProcessStatus.LongProcess: alarmMessage = "Процесс {0} для ИБ \"{1}\" выполняется свыше установленного периода ожидания. Время запуска процесса: {2:HH:mm:ss dd.MM.yyyy}"; break;
                        case RemoteProcessStatus.ServiceFail: alarmMessage = "Служба УРБД после запуска {0} для ИБ \"{1}\" была аварийно завершена и потеряла связь с процессом. Время запуска процесса: {2:HH:mm:ss dd.MM.yyyy}"; break;
                        case RemoteProcessStatus.UnknownFail: alarmMessage = "Неизестный сбой процесса {0} для ИБ \"{1}\". Время запуска процесса: {2:HH:mm:ss dd.MM.yyyy}"; break;
                    }
                    if (!string.IsNullOrEmpty(alarmMessage))
                    {
                        alarmMessage = String.Format(alarmMessage, command.GetType().Name, launchReport.baseName, launchReport.startDate);
                    }
                    if (!proxy.IsSuccess)
                        RemoteServiceError(command, proxy.Exception);
                    LogHelper.Write2Log(String.Format("Проверка работы процесса 1С {0}: {1}", launchReport.reportGuid, status), LogLevel.Information);
                }
            }
            else if (launchReport.startDate == null)
            {
                using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, endpointAddr))
                {
                    RemoteProcessStatus status = proxy.CheckProcess(checkCommand);
                    switch (status)
                    {
                        case RemoteProcessStatus.LongStart: alarmMessage = "Процесс для запроса {0} для ИБ \"{1}\" не был запущен в течение установленного периода ожидания."; break;
                        case RemoteProcessStatus.ServiceFail: alarmMessage = "Служба УРБД после запуска запроса {0} для ИБ \"{1}\" была аварийно завершена и процесс не был запущен."; break;
                        case RemoteProcessStatus.UpdateRequired: alarmMessage = "Службе УРБД требуется обновление."; break;
                    }
                    if (!string.IsNullOrEmpty(alarmMessage))
                    {
                        alarmMessage = String.Format(alarmMessage, command.GetType().Name, launchReport.baseName);
                    }
                    if (!proxy.IsSuccess)
                    {
                        RemoteServiceError(command, proxy.Exception);
                    }
                    LogHelper.Write2Log(String.Format("Проверка работы процесса 1С {0}: {1}", launchReport.reportGuid, status), LogLevel.Information);
                }
            }
            try
            {
                if (alarmer != null && !string.IsNullOrEmpty(alarmMessage))
                    alarmer.Alarm(checkCommand.reportGuid, alarmMessage);
            }
            catch (AlarmException ex)
            {
                if (logger != null)
                    logger.Fail(endpointAddr.Uri, ex.Message);
            }
            if (logger != null && !string.IsNullOrEmpty(alarmMessage))
                logger.Warning(endpointAddr.Uri, alarmMessage);
        }

        public void Start()
        {
            serviceHost.Open();
        }

        public void Stop()
        {
            serviceHost.Close();
        }

        public void InterruptCommand(ExecuteCommand command)
        {
            LaunchReport launchReport = handler.GetLaunchReport(command);

            // нельзя снять отработанную задачу
            if (launchReport == null)
                return;
            EndpointAddress remoteServiceEndpoint = serviceBases[command.baseId];

            ExecuteCommand preparedCommand = handler.PrepareCommand(command);

            using (RemoteServiceProxy proxy = new RemoteServiceProxy(channelFactory, remoteServiceEndpoint))
            {
                proxy.InterruptProcess(preparedCommand);
                if (!proxy.IsSuccess)
                    RemoteServiceError(command, proxy.Exception);
            }
        }
    }
}
