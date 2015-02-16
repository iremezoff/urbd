using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;


namespace URBDRemoteService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller serviceProcessInstaller;
        private string serviceName = "Ugoria.URBD.RemoteService";
        public ProjectInstaller()
        {
            InitializeComponent();

            serviceInstaller = new ServiceInstaller();
            serviceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = serviceName;
            serviceInstaller.DisplayName = "Ugoria.URBD.RemoteService";
            serviceInstaller.Description = "Обеспечение обмена данными между центральной и периферийными распределенными информационными базами 1С";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            Installers.Add(serviceInstaller);
            Installers.Add(serviceProcessInstaller);
        }

        private string GetContextParameter(string key)
        {
            if (Context.Parameters.ContainsKey(key))
                return Context.Parameters[key];
            return string.Empty;            
        }

        protected override void OnBeforeInstall(IDictionary savedState)
        {
            if (ServiceHelper.ServiceIsInstalled(serviceName))
                ServiceHelper.Uninstall(serviceName);

            base.OnBeforeInstall(savedState);

            //serviceProcessInstaller.Username = GetContextParameter("user").Trim();
            //serviceProcessInstaller.Password = GetContextParameter("password").Trim();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);
            ServiceHelper.StartService(serviceName);
        }

        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);            
        }
    }
}
