using System;
using System.Data;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Core;
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService
{
    public class URBDCentralWorker
    {
        private RemoteServicesManager remoteServiceManager;
        private SchedulerManager schedulerManager;
        private IConfiguration conf;
        private IDataProvider dataProvider;// = new DBDataProvider();

        public URBDCentralWorker()
        {
            Init();
        }

        private void Init()
        {
            schedulerManager = new SchedulerManager();
            dataProvider = new DBDataProvider();
            ConfigurationManager confManager = new ConfigurationManager(dataProvider);
            conf = confManager.GetCentralServiceConfiguration();

            remoteServiceManager = new RemoteServicesManager(confManager);
            remoteServiceManager.Logger = new Logger(dataProvider);
            remoteServiceManager.Reporter = new Reporter(dataProvider);

            DataSet scheduleData = dataProvider.GetScheduleData();

            foreach (DataRow baseRow in scheduleData.Tables["Base"].Rows)
            {
                remoteServiceManager.AddBaseService(baseRow["address"].ToString(), baseRow["base_name"].ToString());

                ExchangeScheduleConfigure(baseRow);
                ExtFormsScheduleConfigure(baseRow);
            }
        }

        public void ExchangeScheduleConfigure(DataRow baseRow)
        {            
            foreach (DataRow schedExchRow in baseRow.GetChildRows("BaseScheduleExchange"))
            {
                ModeType mode = ModeType.Normal;
                switch (schedExchRow["mode"].ToString())
                {
                    case "A": mode = ModeType.Aggresive; break;
                    case "E": mode = ModeType.Extreme; break;
                    case "P": mode = ModeType.Passive; break;
                }
                Command command = new Command
                {
                    baseName = baseRow["base_name"].ToString(),
                    commandType = Ugoria.URBD.Contracts.Service.CommandType.Exchange,
                    modeType = mode,
                    withMD = (bool)schedExchRow["with_md"]
                };
                schedulerManager.AddScheduleLaunch(command,
                    remoteServiceManager.SendCommand,
                    remoteServiceManager.CheckProcess,
                    schedExchRow["time"].ToString(), 
                    int.Parse(conf.GetParameter("delay_check").ToString()));
            }
        }

        public void ExtFormsScheduleConfigure(DataRow baseRow)
        {
            foreach (DataRow schedEFRow in baseRow.GetChildRows("BaseScheduleExtForms"))
            {
                Command command = new Command
                {
                    baseName = baseRow["base_name"].ToString(),
                    commandType = Ugoria.URBD.Contracts.Service.CommandType.ExtForms
                };
                schedulerManager.AddScheduleLaunch(command,
                    remoteServiceManager.SendCommand,
                    remoteServiceManager.CheckProcess,
                    schedEFRow["time"].ToString(),
                    int.Parse(conf.GetParameter("delay_check").ToString()));
            }
        }

        public void AddExchangeTask (string basename, ModeType modeType, bool withMD)
        {
            Command command = new Command
            {
                baseName = basename,
                commandType = Ugoria.URBD.Contracts.Service.CommandType.Exchange,
                modeType = modeType,
                withMD = withMD                
            };
            
        }

        public void Start()
        {
            remoteServiceManager.Start();
            schedulerManager.Start();
        }
    }
}
