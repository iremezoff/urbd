using System;
using System.Data;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Core;
using Ugoria.URBD.Contracts.Services;
using System.ServiceModel;
using Ugoria.URBD.CentralService.Scheduler;
using System.Collections;
using Ugoria.URBD.Logging;

namespace Ugoria.URBD.CentralService
{
    public class URBDCentralWorker
    {
        private RemoteServicesManager remoteServiceManager;
        private SchedulerManager schedulerManager;
        private IConfiguration conf;
        private IDataProvider dataProvider;// = new DBDataProvider();
        private ServiceHost controlHost;

        public URBDCentralWorker()
        {
            Init();
        }

        private void Init()
        {
            LogHelper.IsConsoleOutputEnabled = true;
            LogHelper.CurrentComponent = URBDComponent.Central;

            ControlService controlService = new ControlService();

            controlService.SendTask += (sender, args) => AddExchangeTask(args.UserId, args.BaseId, args.ModeType);
            controlService.SendInterruptTask += (sender, args) => InterruptProcess(args.BaseId);
            controlHost = new ServiceHost(controlService);

            schedulerManager = new SchedulerManager();
            dataProvider = new DBDataProvider();

            try
            {
                ConfigurationManager confManager = new ConfigurationManager(dataProvider);
                conf = confManager.GetCentralServiceConfiguration();

                Uri controlUri = new Uri((string)conf.GetParameter("service_control_address"));
                controlHost.AddServiceEndpoint(typeof(IControlService),
                    new NetTcpBinding(SecurityMode.None),
                    controlUri);

                remoteServiceManager = new RemoteServicesManager(confManager);
                Logger logger = new Logger(dataProvider);
                remoteServiceManager.Logger = logger;
                remoteServiceManager.Reporter = new Reporter(dataProvider);
                Alarmer alarmer = new Alarmer(dataProvider, conf);
                alarmer.Logger = logger;
                remoteServiceManager.Alarmer = alarmer;

                DataSet scheduleData = dataProvider.GetScheduleData();

                foreach (DataRow baseRow in scheduleData.Tables["Base"].Rows)
                {
                    remoteServiceManager.AddBaseService(baseRow["address"].ToString(), (int)baseRow["base_id"]);

                    ExchangeScheduleConfigure(baseRow);
                    ExtFormsScheduleConfigure(baseRow);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log(ex);
            }
        }

        public void ExchangeScheduleConfigure(DataRow baseRow)
        {
            foreach (DataRow schedExchRow in baseRow.GetChildRows("BaseScheduleExchange"))
            {
                IConfiguration schedCfg = ConfigurationManager.GetConfiguration(schedExchRow);
                schedulerManager.AddScheduleLaunch(remoteServiceManager.SendCommand,
                    schedCfg,
                    Ugoria.URBD.Contracts.Services.CommandType.Exchange,
                    remoteServiceManager.CheckProcess,
                    int.Parse(conf.GetParameter("delay_check").ToString()));
            }
        }

        public void ExtFormsScheduleConfigure(DataRow baseRow)
        {
            foreach (DataRow schedEFRow in baseRow.GetChildRows("BaseScheduleExtForms"))
            {
                IConfiguration schedCfg = ConfigurationManager.GetConfiguration(schedEFRow);
                schedulerManager.AddScheduleLaunch(remoteServiceManager.SendCommand,
                    schedCfg,
                    Ugoria.URBD.Contracts.Services.CommandType.ExtForms,
                    remoteServiceManager.CheckProcess,
                    int.Parse(conf.GetParameter("delay_check").ToString()));
            }
        }

        public void AddExchangeTask(int userId, int baseId, ModeType modeType)
        {
            LogHelper.Write2Log(String.Format("Пришел запрос на команду от userId {0}, ИБ ID: {1}, Mode {2}", userId, baseId, modeType), LogLevel.Information);

            remoteServiceManager.SendCommand(baseId, Ugoria.URBD.Contracts.Services.CommandType.Exchange, modeType, userId);
            schedulerManager.AddScheduleLaunch(remoteServiceManager.CheckProcess,
                baseId,
                Contracts.Services.CommandType.Exchange,
                int.Parse((string)conf.GetParameter("delay_check")));
        }

        private void InterruptProcess(int baseId)
        {
            LogHelper.Write2Log("Пришел запрос на остановку задачи для ИБ id=" + baseId, LogLevel.Information);
            remoteServiceManager.InterruptCommand(baseId);
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
            }
        }
    }
}
