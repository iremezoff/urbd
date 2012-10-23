using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;

namespace Ugoria.URBD.CentralService.Scheduler
{
    public delegate void TriggerListenerHandler(ITrigger sender, EventArgs args);
    class TriggerListener : ITriggerListener
    {
        public string name;

        public event TriggerListenerHandler Complete;
        public event TriggerListenerHandler Fired;
        public event TriggerListenerHandler Misfired;

        public string Name
        {
            get { return name; }
        }

        public void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            if (Complete != null)
                Complete(trigger, new EventArgs());
        }

        public void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            if (Fired != null)
                Fired(trigger, new EventArgs());
        }

        public void TriggerMisfired(ITrigger trigger)
        {
            if (Misfired != null)
                Misfired(trigger, new EventArgs());
        }

        public bool VetoJobExecution(ITrigger trigger, IJobExecutionContext context)
        {
            return false;
        }

        public TriggerListener(string name)
        {
            this.name = name;
        }
    }
}
