using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService
{
    public delegate void RunTaskHandler(object sender, TaskEventArgs args);
    public delegate void InterruptTaskHandler(object sender, InterruptTaskEventArgs args);
    //public delegate IDictionary<string, string> ValidateConfigurationHandler(object sender, ValidateConfigurationEventArgs args);
    public delegate void ReconfigureHandler(object sender, ReconfigureEventArgs args);
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    class ControlService : IControlService
    {
        public event RunTaskHandler SendTask;
        public event InterruptTaskHandler SendInterruptTask;
        public event ReconfigureHandler BaseReconfigure;
        public event ReconfigureHandler ServiceReconfigure;
        public event EventHandler CentralReconfigure;

        public void RunTask(int userId, ExecuteCommand command)
        {
            if (SendTask != null)
                SendTask(this, new TaskEventArgs(userId, command));
        }

        public void InterruptTask(ExecuteCommand command)
        {
            if (SendInterruptTask != null)
                SendInterruptTask(this, new InterruptTaskEventArgs(command));
        }
        
        public void ReconfigureBaseOfService(int baseId)
        {
            if (BaseReconfigure != null)
                BaseReconfigure(this, new ReconfigureEventArgs(baseId));
        }

        public void ReconfigureCentralService()
        {
            if (CentralReconfigure != null)
                CentralReconfigure(this, new EventArgs());
        }

        public void ReconfigureRemoteService(int serviceId)
        {
            if (ServiceReconfigure != null)
                ServiceReconfigure(this, new ReconfigureEventArgs(serviceId));
        }
    }

    public class TaskEventArgs : EventArgs
    {
        private ExecuteCommand command;

        public ExecuteCommand Command
        {
            get { return command; }
        }

        private int userId;

        public int UserId
        {
            get { return userId; }
        }

        internal TaskEventArgs(int userId, ExecuteCommand command)
        {
            this.userId = userId;
            this.command = command;
        }
    }

    public class ReconfigureEventArgs : EventArgs
    {
        private int entityId;

        public int EntityId
        {
            get { return entityId; }
        }

        internal ReconfigureEventArgs(int entityId)
        {
            this.entityId = entityId;
        }
    }

    public class InterruptTaskEventArgs : EventArgs
    {
        private ExecuteCommand command;

        public ExecuteCommand Command
        {
            get { return command; }
        }

        internal InterruptTaskEventArgs(ExecuteCommand command)
        {
            this.command = command;
        }
    }

    public class ValidateConfigurationEventArgs : EventArgs
    {
        private Uri remoteUri;

        public Uri RemoteUri
        {
            get { return remoteUri; }
        }
        private RemoteConfiguration configuration;

        public RemoteConfiguration Configuration
        {
            get { return configuration; }
        }

        internal ValidateConfigurationEventArgs(Uri remoteUri, RemoteConfiguration configuration)
        {
            this.remoteUri = remoteUri;
            this.configuration = configuration;
        }
    }
}
