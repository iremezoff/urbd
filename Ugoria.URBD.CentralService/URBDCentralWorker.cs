using System;
using System.Linq;
using System.Data;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Contracts.Services;
using System.ServiceModel;
using Ugoria.URBD.CentralService.Scheduler;
using System.Collections;
using System.ServiceModel.Description;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.CentralService.DataProvider;
using Ugoria.URBD.CentralService.Logging;
using Ugoria.URBD.Contracts.Data.Commands;
using System.Reflection;
using System.Collections.Generic;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.CentralService.Alarming;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Service;

namespace Ugoria.URBD.CentralService
{
    public class URBDCentralWorker
    {
        private RemoteServicesManager remoteServiceManager;
        private SchedulerManager schedulerManager;
        private ServiceHost controlHost;
        private CentralConfigurationManager confManager;
        private ChannelFactory<IWebService> webChannelFactory = new ChannelFactory<IWebService>(new NetTcpBinding(SecurityMode.None), new EndpointAddress("net.tcp://localhost:9999/URBDWebService"));

        public URBDCentralWorker()
        {
            Init();
        }

        private void Init()
        {
            LogHelper.IsConsoleOutputEnabled = true;
            LogHelper.CurrentComponent = URBDComponent.Central;

            ControlService controlService = new ControlService();

            controlService.SendTask += (sender, args) => AddExchangeTask(args.UserId, args.Command);
            controlService.SendInterruptTask += (sender, args) => InterruptProcess(args.Command);
            controlService.BaseReconfigure += (sender, args) => ReconfigureBaseOfService(args.EntityId);
            controlService.ServiceReconfigure += (sender, args) => ReconfigureRemoteService(args.EntityId);
            controlService.CentralReconfigure += (sender, args) => ReconfigureCentralService();
            controlHost = new ServiceHost(controlService);

            confManager = new CentralConfigurationManager();

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            WCFMessageHandler messageHandler = new WCFMessageHandler();
            foreach (Type exportType in currentAssembly.GetExportedTypes())
            {
                if (exportType.IsClass && !exportType.IsAbstract && typeof(DataHandler).IsAssignableFrom(exportType))
                    messageHandler.AddHandler((DataHandler)Activator.CreateInstance(exportType));
            }

            schedulerManager = new SchedulerManager();

            try
            {
                ReconfigureCentralService();
                remoteServiceManager = new RemoteServicesManager(confManager);
                remoteServiceManager.CommandSended += CommandSended;
                remoteServiceManager.ReportReceived += ReportReceived;

                IConfiguration conf = new Configuration(confManager.GetCentralServiceConfiguration());
                Uri controlUri = new Uri((string)conf.GetParameter("main.service_control_address"));
                controlHost.AddServiceEndpoint(typeof(IControlService),
                    new NetTcpBinding(SecurityMode.None),
                    controlUri);

                Logger logger = new Logger();
                remoteServiceManager.Logger = logger;
                remoteServiceManager.Handler = messageHandler;
                Alarmer alarmer = new Alarmer(conf);
                remoteServiceManager.Alarmer = alarmer;

                using (DBDataProvider dataProvider = new DBDataProvider())
                {
                    DataSet scheduleData = dataProvider.GetScheduleData();

                    foreach (DataRow baseRow in scheduleData.Tables["Base"].Rows)
                    {
                        remoteServiceManager.AddBaseService(baseRow["address"].ToString(), (int)baseRow["base_id"]);

                        ExchangeScheduleConfigure(baseRow);
                        ExtDirectoriesScheduleConfigure(baseRow);
                        MlgCollectScheduleConfigure(baseRow);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
        }

        public void MlgCollectScheduleConfigure(DataRow baseRow)
        {
            foreach (DataRow schedExchRow in baseRow.GetChildRows("BaseScheduleMlgCollect"))
            {
                ExecuteCommand command = new MlgCollectCommand
                {
                    baseId = (int)schedExchRow["base_id"],
                    baseName = (string)baseRow["base_name"]
                };
                schedulerManager.AddScheduleLaunch(remoteServiceManager.SendCommand,
                    command,
                    (string)schedExchRow["time"],
                    remoteServiceManager.CheckProcess);
            }
        }

        public void ExchangeScheduleConfigure(DataRow baseRow)
        {
            foreach (DataRow schedExchRow in baseRow.GetChildRows("BaseScheduleExchange"))
            {                
                ExecuteCommand command = new ExchangeCommand
                {
                    baseId = (int)schedExchRow["base_id"],
                    baseName = (string)baseRow["base_name"],
                    modeType = "N".Equals((string)schedExchRow["mode"]) ? ModeType.Normal : ModeType.Passive
                };
                schedulerManager.AddScheduleLaunch(remoteServiceManager.SendCommand,
                    command,
                    (string)schedExchRow["time"],
                    remoteServiceManager.CheckProcess);
            }
        }

        public void ExtDirectoriesScheduleConfigure(DataRow baseRow)
        {
            foreach (DataRow schedEFRow in baseRow.GetChildRows("BaseScheduleExtDirectories"))
            {               
                ExecuteCommand command = new ExtDirectoriesCommand
                {
                    baseId = (int)schedEFRow["base_id"],
                    baseName = (string)baseRow["base_name"]                    
                };
                schedulerManager.AddScheduleLaunch(remoteServiceManager.SendCommand,
                    command,
                    (string)schedEFRow["time"],
                    remoteServiceManager.CheckProcess);
            }
        }

        public void AddExchangeTask(int userId, ExecuteCommand command)
        {
            LogHelper.Write2Log(String.Format("Пришел запрос на команду {0} от userId {1}, ИБ {2}", command.GetType(), userId, command.baseName), LogLevel.Information);

            remoteServiceManager.SendCommand(command, userId);
            schedulerManager.AddScheduleLaunch(remoteServiceManager.CheckProcess, command);
        }

        private void CommandSended(ExecuteCommand command)
        {
            LogHelper.Write2Log("Оповещение веб-сервиса о успешной отправке команды", LogLevel.Information);
            using (WebServiceProxy proxy = new WebServiceProxy(webChannelFactory))
            {
                proxy.NotifyCommand(command);
                if (!proxy.IsSuccess)
                    LogHelper.Write2Log(proxy.Exception);
            }
        }

        private void ReportReceived(Report report)
        {
            LogHelper.Write2Log("Оповещение веб-сервиса о получении отчета", LogLevel.Information);            
            using (WebServiceProxy proxy = new WebServiceProxy(webChannelFactory))
            {
                proxy.NotifyReport(report);
                if (!proxy.IsSuccess)
                    LogHelper.Write2Log(proxy.Exception);
            }
        }

        public void ReconfigureBaseOfService(int baseId)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                DataSet scheduleData = dataProvider.GetScheduleData(baseId);

                if (scheduleData.Tables["Base"].Rows.Count == 0)
                    return;

                DataRow baseRow = scheduleData.Tables["Base"].Rows[0];

                remoteServiceManager.AddBaseService(baseRow["address"].ToString(), (int)baseRow["base_id"]);

                schedulerManager.RemoveScheduleLaunch(baseRow["base_id"].ToString());

                ExchangeScheduleConfigure(baseRow);
                ExtDirectoriesScheduleConfigure(baseRow);
                MlgCollectScheduleConfigure(baseRow);
            }
        }

        public void ReconfigureRemoteService(int serviceId)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                DataTable serviceBases = dataProvider.GetServiceBases(serviceId);

                foreach (DataRow baseRow in serviceBases.Rows)
                {
                    remoteServiceManager.AddBaseService(baseRow["address"].ToString(), (int)baseRow["base_id"]);
                }
            }
        }

        public void ReconfigureCentralService()
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                confManager = new CentralConfigurationManager();
                Hashtable conf = confManager.GetCentralServiceConfiguration();

                if (remoteServiceManager != null)
                    remoteServiceManager.ConfigurationManager = confManager;

                if (schedulerManager != null)
                    schedulerManager.DelayCheck = int.Parse((string)conf["main.delay_check"]);
            }
        }

        private void InterruptProcess(ExecuteCommand command)
        {
            LogHelper.Write2Log("Пришел запрос на остановку задачи для ИБ id=" + command.baseId, LogLevel.Information);
            remoteServiceManager.InterruptCommand(command);
        }

        public void Start()
        {
            LogHelper.Write2Log("Запуск центрального сервиса", LogLevel.Information);
            try
            {
                controlHost.Open();
                remoteServiceManager.Start();
                schedulerManager.Start();
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
                throw;
            }
        }

        public void Stop()
        {
            LogHelper.Write2Log("Остановка центрального сервиса", LogLevel.Information);
            try
            {
                controlHost.Close();
                remoteServiceManager.Stop();
                schedulerManager.Stop();
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
                throw;
            }
        }
    }
}
