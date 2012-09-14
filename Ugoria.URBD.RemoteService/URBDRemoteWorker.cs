using System;
using System.ServiceModel;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Core;
using System.ServiceModel.Channels;
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.RemoteService
{
    class URBDRemoteWorker
    {
        private RemoteService service;
        private ServiceHost serviceHost;
        private QueueExecuteManager queueManager;
        private ICentralService centralService;
        //private ILogger logger;
        private RemoteConfigurationManager remoteConfigurationManager;
        private ChannelFactory<ICentralService> channelFactory;
        private Uri localUri;
        private Uri centralUri;
        private string port = "7000";
        private ICommunicationObject comm;

        public URBDRemoteWorker ()
        {
            Init();
        }

        private void Init ()
        {
            remoteConfigurationManager = new RemoteConfigurationManager();
            queueManager = new QueueExecuteManager(remoteConfigurationManager);

            service = new RemoteService();
            service.Registered += ServiceRegistered;
            service.Configured += ServiceConfigure;
            service.CommandSended += ServiceCommandSender;
            service.ProcessChecked += ServiceProcessCheck;
            serviceHost = new ServiceHost(service);
            serviceHost.AddServiceEndpoint(typeof(IRemoteService),
                new NetTcpBinding(SecurityMode.None),
                new Uri(String.Format("net.tcp://localhost:{0}/URBDRemoteService", port)));

            channelFactory = new ChannelFactory<ICentralService>(new NetTcpBinding(SecurityMode.None));
        }

        public void ServiceConfigure (IRemoteService service, ConfigureEventArgs args)
        {
            // конфигурирование только при отсутствующих процессах 1С - предусмотреть
            remoteConfigurationManager.RemoteConfiguration = args.Configuration;
            lock (queueManager)
            {
                remoteConfigurationManager.CreateFileAndRegistryKeys();
                queueManager.ConfigurationManager = remoteConfigurationManager;
            }
        }

        public void ServiceRegistered (IRemoteService service, RegisteredEventArgs args)
        {
            centralUri = args.CentralUri;
            centralService = channelFactory.CreateChannel(new EndpointAddress(centralUri));

            ICommunicationObject comm = (ICommunicationObject)centralService;
            comm.Open();

            localUri = args.LocalUri;

            if (remoteConfigurationManager.RemoteConfiguration == null)
                centralService.RequestConfiguration(args.LocalUri);
        }

        public void ServiceCommandSender (IRemoteService service, CommandEventArgs args)
        {
            queueManager.AddOperation(args.Command, OperationCallback);
        }

        private void OperationCallback (Report report)
        {
            bool isDone = false;
            int attemptCount = 3;
            while (!isDone)
            {
                try
                {
                    if (report is LaunchReport)
                        centralService.NoticePID1C((LaunchReport)report, localUri);
                    else
                    {
                        Console.WriteLine(((ICommunicationObject)centralService).State);
                        centralService.NoticeReport((OperationReport)report, localUri);
                        Console.WriteLine(((ICommunicationObject)centralService).State);
                    }
                    isDone = true;
                }
                catch (Exception ex)
                {
                    attemptCount--;
                    if (attemptCount > 0)
                        centralService = channelFactory.CreateChannel(new EndpointAddress(centralUri));
                    else
                    {
                        // panick
                        isDone = true;
                    }
                }
            }
        }

        private RemoteProcessStatus ServiceProcessCheck (IRemoteService service, CheckEventArgs args)
        {
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

        public void Start ()
        {
            serviceHost.Open();
        }

        public void Stop ()
        {
            serviceHost.Close();
        }
    }
}
