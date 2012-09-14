using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using Ugoria.URBD.Contracts;
using Quartz.Impl.Matchers;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Service;

namespace Ugoria.URBD.CentralService
{
    public delegate void TriggerListenerHandler(ITrigger sender, EventArgs args);
    class SchedulerManager
    {
        private IScheduler scheduler;

        public SchedulerManager()
        {
            Init();
        }

        private void Init()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            scheduler = schedulerFactory.GetScheduler();
        }

        // для установки запуска
        public void AddScheduleLaunch(Command command, Action<Command> commanAction, Action<Command> checkAction, string time, int delayCheck)
        {
            CommandType commandType = command.commandType;
            IJobDetail jobDetail = GetJobDetail(command.baseName, command.commandType);

            string triggerName = "";
            if (command.commandType == CommandType.Exchange)
                triggerName = String.Format("{0}_{1}_{2}_{3}", command.baseName, time, command.modeType.ToString(), (command.withMD ? "withMD" : "withoutMD"));
            else
                triggerName = String.Format("{0}_{1}", command.baseName, time);

            ITrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, commandType.ToString())
                                                      .WithCronSchedule(SchedulerUtil.CronExpressionBuild(time))
                                                      .ForJob(jobDetail)
                                                      .Build();


            trigger.JobDataMap["command"] = command;
            trigger.JobDataMap["action"] = commanAction;
            trigger.JobDataMap["check_action"] = checkAction;
            trigger.JobDataMap["delay_check"] = delayCheck;

            scheduler.ScheduleJob(trigger);
            TriggerListener triggerListener = new TriggerListener(triggerName);
            triggerListener.Complete += TriggerComplete;

            IMatcher<TriggerKey> matcher = KeyMatcher<TriggerKey>.KeyEquals(trigger.Key);


            scheduler.ListenerManager.AddTriggerListener(triggerListener, matcher);
        }

        // для запуска проверок
        public void TriggerComplete(ITrigger sender, EventArgs args)
        {
            AddScheduleLaunch((Command)sender.JobDataMap["command"], (Action<Command>)sender.JobDataMap["check_action"], (int)sender.JobDataMap["delay_check"]);
        }

        // для запусков заданий проверок (единовременные запуски)
        public void AddScheduleLaunch(Command command, Action<Command> checkAction, int delayCheck)
        {
            IJobDetail jobDetail = GetJobDetail(command.baseName, CommandType.Checker);

            string triggerName = String.Format("{0}_checker", command.baseName);

            ITrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, CommandType.Checker.ToString())
                                                      .StartAt(DateBuilder.FutureDate(delayCheck, IntervalUnit.Minute))  // временной промежуток
                                                      .WithSimpleSchedule()
                                                      .ForJob(jobDetail)
                                                      .Build();
            trigger.JobDataMap["action"] = checkAction;
            trigger.JobDataMap["command"] = command;

            scheduler.ScheduleJob(trigger);
        }

        private IJobDetail GetJobDetail(string jobName, CommandType type)
        {
            JobKey jobKey = new JobKey(jobName, type.ToString());
            IJobDetail jobDetail = null;
            if (!scheduler.CheckExists(jobKey)) // проверка на наличие у шедулера текущего задания
            {
                jobDetail = JobBuilder.Create<CommandJob>()
                        .WithIdentity(jobKey)
                        .Build();
                jobDetail.JobDataMap["base_name"] = jobName;
                scheduler.AddJob(jobDetail, true);
            }
            else
                jobDetail = scheduler.GetJobDetail(jobKey);
            return jobDetail;
        }

        public void Start()
        {
            scheduler.Start();
        }

        public void Pause(string jobName)
        {
            scheduler.PauseAll();
        }

        public void Stop()
        {
            scheduler.Shutdown();
        }
    }

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
