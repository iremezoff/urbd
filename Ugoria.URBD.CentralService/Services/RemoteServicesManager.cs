using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Ugoria.URBD.Core;
using System.Collections.Concurrent;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Core.Reporting;

namespace Ugoria.URBD.CentralService
{
    class RemoteServicesManager
    {
        private static string uriPattern = "net.tcp://{0}/URBDRemoteService";
        private static readonly string centralAddr = "net.tcp://localhost:8000/URBDCentralService";

        private ChannelFactory<IRemoteService> channelFactory = null;
        private Dictionary<string, IRemoteService> services = new Dictionary<string, IRemoteService>();
        private ServiceHost serviceHost;
        private ILogger logger = null;
        private IReporter reporter = null;
        private IAlarmer alarmer = null;
        private ConfigurationManager configurationManager;
        private IConfiguration centralServiceConguration;
        private ConcurrentDictionary<string, IRemoteService> serviceBases = new ConcurrentDictionary<string, IRemoteService>();

        public ICollection<KeyValuePair<string, IRemoteService>> Services
        {
            get { return services; }
        }

        public ConfigurationManager ConfigurationManager
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

        public RemoteServicesManager (ConfigurationManager configurationManager = null)
        {
            this.configurationManager = configurationManager;
            Init();
        }

        private void Init ()
        {
            centralServiceConguration = configurationManager.GetCentralServiceConfiguration();

            CentralService centralService = new CentralService();
            channelFactory = new ChannelFactory<IRemoteService>(new NetTcpBinding(SecurityMode.None));

            centralService.RemoteRequestConfiguration += RemoteRequestConfiguartion;
            centralService.RemoteNoticePID1C += RemoteNoticePID1C;
            centralService.RemoteNoticeReport += RemoteNoticeReport;

            serviceHost = new ServiceHost(centralService);
            serviceHost.AddServiceEndpoint(typeof(ICentralService),
                new NetTcpBinding(SecurityMode.None),
                new Uri(centralAddr));

            if (configurationManager != null)
            {
                IConfiguration centralConguration = configurationManager.GetCentralServiceConfiguration();
                string serviceName = (string)centralConguration.GetParameter("service_name");
                uriPattern = String.Format("net.tcp://{{0}}/{0}", serviceName);
            }
        }

        private void RemoteRequestConfiguartion (ICentralService sender, RequestConfigureEventArgs args)
        {
            // получить конфигурацию для удаленного сервиса (ftp, путь до 1с, список баз)
            string remoteAddress = String.Format("{0}:{1}", args.Uri.Host, args.Uri.Port);

            IConfiguration remoteServiceConfiguration = configurationManager.GetRemoteServiceConfiguration(remoteAddress);
            //IConfiguration centralServiceConguration = configurationManager.GetCentralServiceConfiguration();

            RemoteConfiguration remoteConfiguration = new RemoteConfiguration
            {
                ftpAddress = (string)centralServiceConguration.GetParameter("ftp_address"),
                threadsCount = int.Parse((string)centralServiceConguration.GetParameter("max_threads")),
                extFormsPath = (string)centralServiceConguration.GetParameter("extforms_path"),
                file1CPath = (string)remoteServiceConfiguration.GetParameter("1c_path"),
                cpPath = (string)centralServiceConguration.GetParameter("cp_path"),
                pcPath = (string)centralServiceConguration.GetParameter("pc_path"),
                waitTime = int.Parse(centralServiceConguration.GetParameter("wait_time").ToString()),
                packetExchangeAttempts = int.Parse(centralServiceConguration.GetParameter("packet_exchange_attempts").ToString()),
                packetExchangeWaitTime = int.Parse(centralServiceConguration.GetParameter("packet_exchange_wait_time").ToString())
            };

            foreach (IConfiguration baseConfiguration in (List<IConfiguration>)remoteServiceConfiguration.GetParameter("bases"))
            {
                Base base1c = new Base
                {
                    baseName = (string)baseConfiguration.GetParameter("base_name"),
                    basePath = (string)baseConfiguration.GetParameter("1c_database"),
                    username = (string)baseConfiguration.GetParameter("1с_username"),
                    password = (string)baseConfiguration.GetParameter("1с_password")
                };
                foreach (IConfiguration packetConfiguration in (List<IConfiguration>)baseConfiguration.GetParameter("packets"))
                {
                    base1c.packetList.Add(new Packet
                    {
                        filePath = (string)packetConfiguration.GetParameter("file_path"),
                        packetType = "L".Equals((string)packetConfiguration.GetParameter("type")) ? PacketType.Load : PacketType.Unload
                    });
                }
                remoteConfiguration.baseList.Add(base1c);
            }

            services[remoteAddress].Configure(remoteConfiguration); // отправка через прокси-объект
        }

        private void RemoteNoticePID1C (ICentralService sender, NoticePID1CArgs args)
        {
            if (reporter == null)
                return;

            reporter.SetPID1C(args.LaunchReport);
        }

        private void RemoteNoticeReport (ICentralService sender, NoticeReportArgs args)
        {
            if (reporter == null)
                return;

            OperationReport report = args.Report;

            reporter.SetReport(args.Report);

            switch (report.status)
            {
                case ReportStatus.ExchangeSuccess:
                    if (logger != null)
                        logger.Information(args.Uri, "Обмен пакетами базы " + report.baseName + " прошел успешно");
                    break;
                case ReportStatus.ExchangeFail:
                    if (logger != null)
                        logger.Warning(args.Uri, "При обмене пакетами базы " + report.baseName + " возникла ошибка: " + report.message);
                    if (alarmer != null)
                        alarmer.Alarm(args.Uri, "Обмен не прошел. База: " + args.Report.baseName);
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
                        alarmer.Alarm(args.Uri, "Загрузка ExtForms не прошла. База: " + args.Report.baseName);
                    break;
            }
        }

        public void AddBaseService (string ipAddress, string baseName)
        {
            if (!services.ContainsKey(ipAddress))
            {
                RemoteServiceProxy proxy = new RemoteServiceProxy(new Uri(String.Format(uriPattern, ipAddress)), channelFactory);
                proxy.Event += this.RemoteServiceNotice;
                proxy.CommandSended += this.CommandSendedReport;
                services.Add(ipAddress, proxy);
            }

            serviceBases.AddOrUpdate(baseName, services[ipAddress], (key, oVal) => services[ipAddress]);
            //return proxy;
        }

        public void SendCommand (Command command)
        {
            // Проверить, если база вообще существует, иначе записать в логи, что базы нет
            IRemoteService remoteService = serviceBases[command.baseName];

            remoteService.CommandExecute(command);
        }

        private IRemoteService GetChannel (Uri address)
        {
            IRemoteService remoteService = channelFactory.CreateChannel(new EndpointAddress(address));
            return remoteService;
        }

        private IRemoteService GetChannel (string address)
        {
            Uri uri = new Uri(String.Format(uriPattern, address));
            return GetChannel(uri);
        }

        private IRemoteService GetChannel (string address, string name)
        {
            Uri uri = new Uri(String.Format(uriPattern, address));
            return GetChannel(uri);
        }

        private void RemoteServiceNotice (RemoteServiceProxy sender, NoticeEventArgs args)
        {
            Uri address = ((IDuplexContextChannel)sender.RemoteService).RemoteAddress.Uri;

            switch (args.Code)
            {
                case Code.FaultFail:
                    //sender.RemoteService = GetChannel(address);
                    //sender.RegisterCentralService(new Uri(centralAddr)); // повторная попытка подключения      
                    if (logger != null)
                        logger.Warning(address, "Произошла ошибка на стороне удаленного сервиса: " + args.Message);
                    if (alarmer != null)
                        alarmer.Alarm(address, args.Message);
                    break;
                case Code.CommunicationFail:
                    if (logger != null)
                        logger.Fail(address, "Соединение с сервисом потеряно: " + args.Message);
                    // оповещение о неработе сервиса
                    if (alarmer != null)
                        alarmer.Alarm(address, "Проблема в работе сервиса " + address);
                    break;
                default:
                    //sender.AttemptCount = 3; // комманда прошла успешно, кол-во попыток восстановлено
                    if (logger != null)
                        logger.Information(address, args.Message);
                    break;
            }
        }

        private void CommandSendedReport (RemoteServiceProxy sender, SendCommandEventArgs args)
        {
            Uri address = ((IDuplexContextChannel)sender.RemoteService).RemoteAddress.Uri;

            reporter.SetCommandReport(new Report { baseName = args.Command.baseName, dateCommand = args.Command.commandDate, reportGuid = args.Command.guid });
        }

        public void CheckProcess (Command command)
        {
            ReportInfo reportInfo = reporter.CheckReport(command.guid);
            if (reportInfo == null)
                return;
            CheckCommand checkCommand = new CheckCommand
            {
                launchGuid = reportInfo.launchGuid,
                reportGuid = reportInfo.reportGuid
            };

            if (reportInfo.completeDate != DateTime.MinValue) // зачем проверять отработанный процесс?
                return;

            string alarmMessage = "";
            if (reportInfo.startDate != DateTime.MinValue)
            {
                int wait = (int)(reportInfo.startDate - DateTime.Now).Add(new TimeSpan(0, int.Parse(centralServiceConguration.GetParameter("delay_check").ToString()), 0)).TotalMilliseconds;
                Thread.Sleep(wait < 0 ? 0 : wait);
                RemoteProcessStatus status = services[reportInfo.serviceAddress].CheckProcess(checkCommand);
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
                        command.commandType == CommandType.Exchange ? "обмена пакетами" : "загрузки ExtForms",
                        reportInfo.baseName,
                        reportInfo.startDate);
                }
            }
            else if (reportInfo.startDate == null)
            {
                RemoteProcessStatus status = services[reportInfo.serviceAddress].CheckProcess(checkCommand);
                switch (status)
                {
                    case RemoteProcessStatus.LongStart: alarmMessage = "Процесс на {0} для ИБ \"{1}\" не был запущен в течение установленного периода ожидания."; break;
                    case RemoteProcessStatus.ServiceFail: alarmMessage = "Служба УРБД после запуска на {0} для ИБ \"{1}\" была аварийно завершена и процесс не был запущен."; break;
                }
                if (!string.IsNullOrEmpty(alarmMessage))
                {
                    alarmMessage = String.Format(alarmMessage,
                        command.commandType == CommandType.Exchange ? "обмен пакетами" : "загрузку ExtForms",
                        reportInfo.baseName);
                }
            }
            if (alarmer != null && !string.IsNullOrEmpty(alarmMessage))
                alarmer.Alarm(reportInfo.serviceAddress, alarmMessage);
            if (logger != null && !string.IsNullOrEmpty(alarmMessage))
                logger.Warning(reportInfo.serviceAddress, alarmMessage);
        }

        public void Start ()
        {
            Uri centralUri = new Uri(centralAddr);
            foreach (IRemoteService service in services.Values)
            {
                Action<Uri> registerAction = service.RegisterCentralService;
                registerAction.Invoke(centralUri);
            }
            serviceHost.Open();
        }

        public void Stop ()
        {
            foreach (IRemoteService proxy in services.Values)
            {
                Action closeAction = ((ICommunicationObject)((RemoteServiceProxy)proxy).RemoteService).Close;
                closeAction.Invoke();
            }
            serviceHost.Close();
        }
    }

    public class NoticeEventArgs : EventArgs
    {
        private Code code = Code.FaultFail;
        private string message = "";

        public Code Code
        {
            get { return code; }
        }

        public string Message
        {
            get { return message; }
        }

        internal NoticeEventArgs (Code code, string message)
        {
            this.code = code;
            this.message = message;
        }

        internal NoticeEventArgs (string message)
        {
            this.code = Code.Success;
            this.message = message;
        }

        internal NoticeEventArgs (Exception ex)
        {
            if (ex is FaultException)
                this.code = Code.FaultFail;
            else if (ex is CommunicationException)
                this.code = Code.CommunicationFail;
            message = ex.Message;
        }
    }

    public class SendCommandEventArgs : EventArgs
    {
        private Command command;

        public Command Command
        {
            get { return command; }
        }

        internal SendCommandEventArgs (Command command)
        {
            this.command = command;
        }
    }
}
