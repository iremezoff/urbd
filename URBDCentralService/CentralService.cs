using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Ugoria.URBD.CentralService;

namespace URBDCentralService
{
    public partial class CentralService : ServiceBase
    {
        private URBDCentralWorker centralWorker;

        public CentralService()
        {
            InitializeComponent();
            centralWorker =  new URBDCentralWorker();
        }

        protected override void OnStart(string[] args)
        {
            centralWorker.Start();
            AddLog("start service");
        }

        protected override void OnStop()
        {
            centralWorker.Stop();
            AddLog("stop service");
        }

        public void AddLog(string log)
        {
            try
            {
                if (!EventLog.SourceExists("Ugoria.URBD.CentralService"))
                {
                    EventLog.CreateEventSource("Ugoria.URBD.CentralService", "Ugoria.URBD.CentralService");
                }
                eventLog.Source = "Ugoria.URBD.CentralService";
                eventLog.WriteEntry(log);
            }
            catch { }
        }
    }
}
