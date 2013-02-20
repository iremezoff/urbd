using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Quartz.Impl;
using Ugoria.URBD.Contracts;
using Quartz.Impl.Matchers;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;

namespace Ugoria.URBD.CentralService.Scheduler
{
    class SchedulerManager
    {
        private IScheduler scheduler;

        public IScheduler Scheduler
        {
            get { return scheduler; }
        }
        private volatile int delayCheck = 1;

        public int DelayCheck
        {
            get { return delayCheck; }
            set { delayCheck = value; }
        }

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
        public void AddScheduleLaunch(Action<ExecuteCommand> commanAction, ExecuteCommand command, string time, Action<ExecuteCommand> checkAction)
        {
            IJobDetail jobDetail = GetJobDetail(command.baseId.ToString(), command.GetType().Name);

            string triggerName = String.Format("{0}_{1}", command.baseId, time);

            ITrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, command.GetType().Name)
                                                      .WithCronSchedule(SchedulerUtil.CronExpressionBuild(time))
                                                      .ForJob(jobDetail)
                                                      .Build();

            trigger.JobDataMap["command"] = command;
            trigger.JobDataMap["user_id"] = 1;
            trigger.JobDataMap["action"] = commanAction;
            trigger.JobDataMap["check_action"] = checkAction;

            if (!scheduler.CheckExists(trigger.Key))
                scheduler.ScheduleJob(trigger);
            TriggerListener triggerListener = new TriggerListener(triggerName);
            triggerListener.Complete += TriggerComplete;

            IMatcher<TriggerKey> matcher = KeyMatcher<TriggerKey>.KeyEquals(trigger.Key);
            scheduler.ListenerManager.AddTriggerListener(triggerListener, matcher);
        }

        // для запуска проверок
        public void TriggerComplete(ITrigger sender, EventArgs args)
        {
            AddScheduleLaunch((Action<ExecuteCommand>)sender.JobDataMap["check_action"], (ExecuteCommand)sender.JobDataMap["command"]);
        }

        // для запусков заданий проверок (единовременные запуски)
        public void AddScheduleLaunch(Action<ExecuteCommand> checkAction, ExecuteCommand command)
        {
            IJobDetail jobDetail = GetJobDetail(command.baseId.ToString(), "checker");

            string triggerName = String.Format("{0}_checker_{1:yyyy-MM-dd_HHmmss-ffffff}", command.baseId, DateTime.Now);

            ITrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, "checker")
                                                      .StartAt(DateBuilder.FutureDate(delayCheck, IntervalUnit.Minute))  // временной промежуток
                                                      .WithSimpleSchedule()
                                                      .ForJob(jobDetail)
                                                      .Build();

            trigger.JobDataMap["action"] = checkAction;
            trigger.JobDataMap["command"] = command;

            scheduler.ScheduleJob(trigger);
        }

        private IJobDetail GetJobDetail(string jobName, string description)
        {
            JobKey jobKey = new JobKey(description, jobName);
            IJobDetail jobDetail = null;
            if (!scheduler.CheckExists(jobKey)) // проверка на наличие у шедулера текущего задания
            {
                jobDetail = JobBuilder.Create<CommandJob>().
                    WithIdentity(jobKey).
                    Build();
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

        public void RemoveScheduleLaunch(string groupName)
        {
            GroupMatcher<JobKey> matcher = GroupMatcher<JobKey>.GroupEquals(groupName);

            IEnumerable<JobKey> set = scheduler.GetJobKeys(matcher).Where(jk => !jk.Name.Equals(typeof(CheckCommand).Name));

            foreach (JobKey jobKey in scheduler.GetJobKeys(matcher).Where(jk => !jk.Name.Equals(typeof(CheckCommand).Name)).Select(j => j))
            {
                scheduler.UnscheduleJobs(scheduler.GetTriggersOfJob(jobKey).Select(x => x.Key).ToList());
            }
        }

        public void RemoveScheduleLaunch(string groupName, string nameStart)
        {

        }

        public void Stop()
        {
            scheduler.Shutdown();
        }
    }
}
