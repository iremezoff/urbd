using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Text.RegularExpressions;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.RemoteService.Services;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.RemoteService
{
    public class URBDRemoteWorker
    {
        private readonly string currentVersion;

        private Ugoria.URBD.RemoteService.Services.RemoteService service;
        private ServiceHost serviceHost;
        private QueueExecuteManager queueManager;
        private RemoteConfigurationManager remoteConfigurationManager;
        private ChannelFactory<ICentralService> channelFactory;
        private DateTime configurationChangeDate = DateTime.MinValue;
        private volatile bool isUpdateRequire = false;
        private List<Guid> interruptedTask = new List<Guid>(); // накопление задач, прерванных из-за необходимости обновления сервиса
        private volatile bool isFailedConfig = false;

        public URBDRemoteWorker()
        {
            currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString(); // извлечение версии из полного имени сборки
            Init();
        }

        private void Init()
        {
            LogHelper.IsConsoleOutputEnabled = true;
            LogHelper.CurrentComponent = URBDComponent.Remote;

            remoteConfigurationManager = new RemoteConfigurationManager();
            queueManager = new QueueExecuteManager(remoteConfigurationManager);
            queueManager.TaskExecuted += OperationCallback;

            service = new Ugoria.URBD.RemoteService.Services.RemoteService();
            service.CommandSended += ServiceCommandSender;
            service.ProcessChecked += ServiceProcessCheck;
            service.Interrupted += ServiceInterrupt;
            service.Validation += ValidateConfiguration;
            serviceHost = new ServiceHost(service);

            List<IStrategyBuilder> strategyBuilders = new List<IStrategyBuilder>();
            foreach (Type exportType in Assembly.GetExecutingAssembly().GetExportedTypes())
            {
                if (exportType.IsClass && !exportType.IsAbstract && typeof(IStrategyBuilder).IsAssignableFrom(exportType))
                    strategyBuilders.Add((IStrategyBuilder)Activator.CreateInstance(exportType));
            }
            queueManager.MessageHandler = new MessageHandler(strategyBuilders);

            channelFactory = new ChannelFactory<ICentralService>(new NetTcpBinding(SecurityMode.None));
        }

        public void ServiceCommandSender(IRemoteService service, CommandEventArgs args)
        {
            RequireConfiguration(args.Command);

            // Новые команды не ставим в очередь, пока не получим обновление
            if (isUpdateRequire)
            {
                LogHelper.Write2Log("Требуется обновление, задача будет снята (без запуска, для отправления отчета по полученной задаче)", LogLevel.Information);
                interruptedTask.Add(args.Command.reportGuid);
                queueManager.KillTask(args.Command, OperationCallback);
                return;
            }
            if (isFailedConfig)
            {
                LogHelper.Write2Log("Обнаружена ошибка конфигурации, задача будет снята (без запуска, для отправления отчета по полученной задаче)", LogLevel.Information);                
                queueManager.KillTask(args.Command, OperationCallback);
                return;
            }

            LogHelper.Write2Log(String.Format("Постановка задачи в очередь ИБ {0}, время команды: {1:HH:mm:ss dd.MM.yyyy}", args.Command.baseName, args.Command.commandDate), LogLevel.Information);
            queueManager.AddOperation(args.Command);
        }

        public void ServiceInterrupt(IRemoteService service, CommandEventArgs e)
        {
            RequireConfiguration(e.Command);

            LogHelper.Write2Log("Запрос на снятие задачи " + e.Command.reportGuid, LogLevel.Information);
            try
            {
                queueManager.KillTask(e.Command, OperationCallback);
                LogHelper.Write2Log("Попытка снятия задачи завершена " + e.Command.reportGuid, LogLevel.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
        }

        private void UpdateService()
        {
            LogHelper.Write2Log("Подготовка к обновлению", LogLevel.Information);
            RemoteConfiguration configuration = remoteConfigurationManager.RemoteConfiguration;
            FileInfo updaterFileInfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + @"\updater.exe");

            LogHelper.Write2Log("Запуск программы обновления", LogLevel.Information);
            FileInfo copiedFileInfo = updaterFileInfo.CopyTo(Path.GetTempFileName().Replace(".tmp",".exe"), true);
            ProcessStartInfo updaterInfo = new ProcessStartInfo(copiedFileInfo.FullName,
                string.Format("/u:{0} /p:{1} /a:{2}",
                    configuration.configuration["main.username"],
                    configuration.configuration["main.password"],
                    configuration.configuration["main.ftp_address"] + "/urbd/update"));
            Process.Start(updaterInfo);
        }

        public void RequireConfiguration(Command command)
        {
            if ((command.configurationChangeDate - configurationChangeDate).TotalSeconds < 1 || isUpdateRequire)
                return;
            LogHelper.Write2Log("Конфигурирование сервиса", LogLevel.Information);

            try
            {
                using (CentralServiceProxy proxy = new CentralServiceProxy(channelFactory, new EndpointAddress(service.CentralUri)))
                {
                    remoteConfigurationManager.RemoteConfiguration = proxy.RequestConfiguration(service.LocalUri);
                    queueManager.ConfigurationManager = remoteConfigurationManager;
                    if (!currentVersion.Equals(remoteConfigurationManager.RemoteConfiguration.configuration["main.remote_service_version"]))
                    {
                        isUpdateRequire = true;
                        LogHelper.Write2Log(String.Format("Обнаружена новая версия: {0}. Текущая версия: {1}", remoteConfigurationManager.RemoteConfiguration.configuration["main.remote_service_version"], currentVersion), LogLevel.Information);
                        return;
                    }

                    foreach (IStrategyBuilder builder in queueManager.MessageHandler.BuildersStore)
                    {
                        builder.PrepareSystem(remoteConfigurationManager.RemoteConfiguration);
                    }

                    if (!proxy.IsSuccess)
                        LogHelper.Write2Log(proxy.Exception);
                    else
                    {
                        configurationChangeDate = command.configurationChangeDate;
                        LogHelper.Write2Log("Сервис сконфигурирован", LogLevel.Information);
                    }
                }
                isFailedConfig = false;
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
                isFailedConfig = true;
            }
        }

        public IDictionary<string, string> ValidateConfiguration(IRemoteService service, ValidationEventArgs args)
        {
            IDictionary<string, string> report = new Dictionary<string, string>(RemoteConfigurationManager.RemoteValidation(args.Configuration));
            foreach (IStrategyBuilder builder in queueManager.MessageHandler.BuildersStore)
            {
                IDictionary<string, string> validateReport = builder.ValidateConfiguration(args.Configuration);
                if (validateReport == null)
                    continue;
                foreach (KeyValuePair<string, string> entry in validateReport)
                {
                    report.Add(entry);
                }
            }
            return report;
        }

        private void OperationCallback(Report report)
        {
            LogHelper.Write2Log(String.Format("Отправка отчета {0} ИБ {1}, время команды {2:HH:mm:ss dd.MM.yyy}", report.GetType().Name, report.baseName, report.commandDate), LogLevel.Information);

            using (CentralServiceProxy proxy = new CentralServiceProxy(channelFactory, new EndpointAddress(service.CentralUri)))
            {
                if (report is LaunchReport)
                    proxy.NoticePID1C((LaunchReport)report, service.LocalUri);
                else
                {
                    if (isUpdateRequire && interruptedTask.Remove(report.reportGuid))
                        ((OperationReport)report).message = "Задача прервана из-за обновления удаленного сервиса";
                    else if(isFailedConfig)
                        ((OperationReport)report).message = "Задача прервана из-за ошибки конфигурации. Устраните проблемы";
                    proxy.NoticeReport((OperationReport)report, service.LocalUri);
                }
                if (!proxy.IsSuccess)
                    LogHelper.Write2Log(proxy.Exception);
            }

            // Проверяем, остались ли ещё выполняемые задачи и запускаем обновление при случае
            if (isUpdateRequire && queueManager.ExecutedProcessCount == 0)
                UpdateService();
        }

        private RemoteProcessStatus ServiceProcessCheck(IRemoteService service, CheckEventArgs args)
        {
            RequireConfiguration(args.CheckCommand);
            LogHelper.Write2Log(String.Format("Проверка работы процесса 1С {0} для ИБ {1}", args.CheckCommand.reportGuid, args.CheckCommand.baseName), LogLevel.Information);
            // гребаная логика
            // если запуск 1С был, известны время старта (startDate) и pid 
            if (args.CheckCommand.launchGuid != Guid.Empty)
            {
                // Процесс присутствует в памяти и исполняется (в одном из потоков)
                if (queueManager.IsProcessLaunch(args.CheckCommand.reportGuid))
                    return RemoteProcessStatus.LongProcess;
                // Процесс запущен и в списке выполняющихся не замечен, значит службу рестартовали, потеряв все данные о запущенных процессах
                else
                    return RemoteProcessStatus.Miss;
            }
            // обновление сервиса
            else if (isUpdateRequire)
                return RemoteProcessStatus.UpdateRequired;
            // запуска 1С не было и команда в очереди, значит долго не запускается
            else if (queueManager.IsFromQueue(args.CheckCommand.reportGuid))
                return RemoteProcessStatus.LongStart;
            // общий случай, служба была перезапущена
            return RemoteProcessStatus.ServiceFail;
        }

        public void Start()
        {
            try
            {
                LogHelper.Write2Log("Запуск удаленного сервиса", LogLevel.Information);
                remoteConfigurationManager.RemoteConfiguration = null;
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                LogHelper.Write2Log("Остановка удаленного сервиса", LogLevel.Information);
                serviceHost.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
                throw;
            }
        }
    }
}
