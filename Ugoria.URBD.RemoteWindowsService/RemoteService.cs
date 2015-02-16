using System.Diagnostics;
using System.Security.Principal;
using System.ServiceProcess;
using Ugoria.URBD.RemoteService;
using Ugoria.URBD.Shared;

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
            //System.Diagnostics.Debugger.Launch();
            remoteWorker.Start();
            AddLog("start service");
            AddLog(SecureHelper.ConvertUserdomainToClassic(WindowsIdentity.GetCurrent().Name));
            AddLog(WindowsIdentity.GetCurrent().Name);
            AddLog(LogHelper.LogDir);
            //AddLog(LogHelper.LogDir);            
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
