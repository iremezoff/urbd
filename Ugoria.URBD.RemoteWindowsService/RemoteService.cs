using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Ugoria.URBD.RemoteService;

namespace URBDRemoteService
{
    public partial class Service1 : ServiceBase
    {
        private URBDRemoteWorker remoteWorker;
        public Service1()
        {
            InitializeComponent();
            remoteWorker = new URBDRemoteWorker();
        }

        protected override void OnStart(string[] args)
        {
            remoteWorker.Start();
            AddLog("start service");
        }

        protected override void OnStop()
        {
            remoteWorker.Stop();
            AddLog("stop service");
        }

        public void AddLog(string log)
        {
            try
            {
                if (!EventLog.SourceExists("Ugoria.URBD.RemoteService"))
                {
                    EventLog.CreateEventSource("Ugoria.URBD.RemoteService", "Ugoria.URBD.RemoteService");
                }
                eventLog.Source = "Ugoria.URBD.RemoteService";
                eventLog.WriteEntry(log);
            }
            catch { }
        }
    }
}
