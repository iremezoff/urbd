using System;
using System.ServiceModel;
using Ugoria.URBD.Contracts;
using System.ServiceModel.Channels;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.RemoteService.Services;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.RemoteService
{
    public class URBDRemoteWorker
    {
        private Ugoria.URBD.RemoteService.Services.RemoteService service;
        private ServiceHost serviceHost;
        private QueueExecuteManager queueManager;
        private RemoteConfigurationManager remoteConfigurationManager;
        private ChannelFactory<ICentralService> channelFactory;
        private DateTime configurationChangeDate = DateTime.MinValue;

        public URBDRemoteWorker()
        {
            Init();
        }

        private void Init()
        {
            LogHelper.IsConsoleOutputEnabled = true;
            LogHelper.CurrentComponent = URBDComponent.Remote;

            remoteConfigurationManager = new RemoteConfigurationManager();
            queueManager = new QueueExecuteManager(remoteConfigurationManager);

            service = new Ugoria.URBD.RemoteService.Services.RemoteService();
            service.CommandSended += ServiceCommandSender;
            service.ProcessChecked += ServiceProcessCheck;
            service.Interrupted += ServiceInterrupt;
            serviceHost = new ServiceHost(service);

            channelFactory = new ChannelFactory<ICentralService>(new NetTcpBinding(SecurityMode.None));
        }

        public void ServiceInterrupt(IRemoteService service, InterruptEventArgs e)
        {
            //RequireConfiguration(e);

            LogHelper.Write2Log("Запрос на снятие задачи " + e.CommandGuid, LogLevel.Information);
            try
            {
                queueManager.KillTask(e.CommandGuid, OperationCallback);
                LogHelper.Write2Log("Попытка снятия задачи завершена " + e.CommandGuid, LogLevel.Information);
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
        }

        public void ResetConfiguration(IRemoteService service, EventArgs args)
        {
            LogHelper.Write2Log("Сброс конфигурации", LogLevel.Information);
            remoteConfigurationManager.RemoteConfiguration = null;
        }

        public void RequireConfiguration(Command command)
        {
            // конфигурирование только при отсутствующих процессах 1С - предусмотреть
            // проверка даты конфигурации, устарела ли
            if ((command.configurationChangeDate - configurationChangeDate).TotalSeconds < 1)
                return;
            LogHelper.Write2Log("Конфигурирование сервиса", LogLevel.Information);

            try
            {
                using (CentralServiceProxy proxy = new CentralServiceProxy(channelFactory, new EndpointAddress(service.CentralUri)))
                {
                    remoteConfigurationManager.RemoteConfiguration = proxy.RequestConfiguration(service.LocalUri);
                    remoteConfigurationManager.CreateFileAndRegistryKeys();
                    queueManager.ConfigurationManager = remoteConfigurationManager;

                    if (!proxy.IsSuccess)
                        LogHelper.Write2Log(proxy.Exception);
                    else
                    {
                        configurationChangeDate = command.configurationChangeDate;
                        LogHelper.Write2Log("Сервис сконфигурирован", LogLevel.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
        }

        public void ServiceCommandSender(IRemoteService service, CommandEventArgs args)
        {
            RequireConfiguration(args.Command);

            LogHelper.Write2Log("Постановка задачи в очередь", LogLevel.Information);
            queueManager.AddOperation(args.Command, OperationCallback);
        }

        private void OperationCallback(Report report)
        {
            LogHelper.Write2Log("Отправка отчета " + report.GetType(), LogLevel.Information);

            using (CentralServiceProxy proxy = new CentralServiceProxy(channelFactory, new EndpointAddress(service.CentralUri)))
            {
                if (report is LaunchReport)
                    proxy.NoticePID1C((LaunchReport)report, service.LocalUri);
                else
                    proxy.NoticeReport((OperationReport)report, service.LocalUri);
                if (!proxy.IsSuccess)
                    LogHelper.Write2Log(proxy.Exception);
            }
        }

        private RemoteProcessStatus ServiceProcessCheck(IRemoteService service, CheckEventArgs args)
        {
            RequireConfiguration(args.CheckCommand);
            LogHelper.Write2Log("Проверка работы процесса 1С", LogLevel.Information);
            // гребаная логика
            // если запуск 1С был, известны время старта (startDate) и pid 
            if (args.CheckCommand.launchGuid != Guid.Empty)
            {
                // Процесс присутствует в памяти и исполняется (в одном из потоков)
                if (queueManager.IsProcessLaunch(args.CheckCommand.launchGuid))
                    return RemoteProcessStatus.LongProcess;
                // Процесс запущен и в списке выполняющихся не замечен, значит службу рестартовали, потеряв все данные о запущенных процессах
                else
                    return RemoteProcessStatus.Miss;
            }
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
            }
        }
    }
}
