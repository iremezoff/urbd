using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.CentralService
{
    public delegate void RunTaskHandler(object sender, TaskEventArgs args);
    public delegate void InterruptTaskHandler(object sender, InterruptTaskEventArgs args);
    public delegate IDictionary<string, string> ValidateConfigurationHandler(object sender, ValidateConfigurationEventArgs args);
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    class ControlService : IControlService
    {
        public event RunTaskHandler SendTask;
        public event InterruptTaskHandler SendInterruptTask;
        public event ValidateConfigurationHandler Validate;

        public void RunTask(int userId, int baseId, ModeType modeType)
        {
            if (SendTask != null)
                SendTask(this, new TaskEventArgs(userId, baseId, modeType));
        }

        public void InterruptTask(int baseId)
        {
            if (SendInterruptTask != null)
                SendInterruptTask(this, new InterruptTaskEventArgs(baseId));
        }

        public IDictionary<string, string> ValidateConfiguration(Uri remoteUri, RemoteConfiguration configuration)
        {
            if (Validate != null)
                return Validate(this, new ValidateConfigurationEventArgs(remoteUri, configuration));
            return null;
        }
    }

    public class TaskEventArgs : EventArgs
    {
        private int baseId;

        public int BaseId
        {
            get { return baseId; }
        }
        private ModeType modeType;

        public ModeType ModeType
        {
            get { return modeType; }
        }

        private int userId;

        public int UserId
        {
            get { return userId; }
        }

        internal TaskEventArgs(int userId, int baseId, ModeType modeType)
        {
            this.userId = userId;
            this.baseId = baseId;
            this.modeType = modeType;
        }
    }

    public class InterruptTaskEventArgs : EventArgs
    {
        private int baseId;

        public int BaseId
        {
            get { return baseId; }
        }

        internal InterruptTaskEventArgs(int baseId)
        {
            this.baseId = baseId;
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
