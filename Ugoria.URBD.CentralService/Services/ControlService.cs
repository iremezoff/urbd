using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.CentralService
{
    public delegate void RunTaskHandler(object sender, TaskEventArgs args);
    public delegate void InterruptTaskHandler(object sender, InterruptTaskEventArgs args);
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class ControlService : IControlService
    {
        public event RunTaskHandler SendTask;
        public event InterruptTaskHandler SendInterruptTask;

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
}
