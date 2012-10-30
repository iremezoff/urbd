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
using Ugoria.URBD.CentralService.CommandBuilding;

namespace Ugoria.URBD.CentralService.Scheduler
{
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
        public void AddScheduleLaunch(Action<CommandBuilder, int> commanAction, CommandBuilder builder, string time, Action<CommandBuilder> checkAction, int delayCheck)
        {
            IJobDetail jobDetail = GetJobDetail(builder.BaseId.ToString(), builder.Description);

            string triggerName = String.Format("{0}_{1}", builder.BaseId, time);

            ITrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, builder.Description)
                                                      .WithCronSchedule(SchedulerUtil.CronExpressionBuild(time))
                                                      .ForJob(jobDetail)
                                                      .Build();

            trigger.JobDataMap["command_builder"] = builder;
            trigger.JobDataMap["user_id"] = 1;
            trigger.JobDataMap["action"] = commanAction;
            trigger.JobDataMap["check_action"] = checkAction;
            trigger.JobDataMap["delay_check"] = delayCheck;

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
            AddScheduleLaunch((Action<CommandBuilder>)sender.JobDataMap["check_action"],
                (CommandBuilder)sender.JobDataMap["command_builder"],
                (int)sender.JobDataMap["delay_check"]);
        }

        // для запусков заданий проверок (единовременные запуски)
        public void AddScheduleLaunch(Action<CommandBuilder> checkAction, CommandBuilder builder, int delayCheck)
        {
            IJobDetail jobDetail = GetJobDetail(builder.BaseId.ToString());

            string triggerName = String.Format("{0}_checker_{1:yyyy-MM-dd_HHmmss-ffffff}", builder.BaseId, DateTime.Now);

            ITrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, CommandType.Checker.ToString())
                                                      .StartAt(DateBuilder.FutureDate(delayCheck, IntervalUnit.Minute))  // временной промежуток
                                                      .WithSimpleSchedule()
                                                      .ForJob(jobDetail)
                                                      .Build();

            trigger.JobDataMap["action"] = checkAction;
            trigger.JobDataMap["command_builder"] = builder;

            scheduler.ScheduleJob(trigger);
        }

        private IJobDetail GetJobDetail(string jobName, string description = null)
        {
            JobKey jobKey = new JobKey(description ?? "checked", jobName);
            IJobDetail jobDetail = null;
            if (!scheduler.CheckExists(jobKey)) // проверка на наличие у шедулера текущего задания
            {
                JobBuilder builder = string.IsNullOrEmpty(description) ? JobBuilder.Create<CheckJob>() : JobBuilder.Create<CommandJob>();
                jobDetail = builder.WithIdentity(jobKey)
                    .Build();
                jobDetail.JobDataMap["base_id"] = jobName;
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

        public void StopForBase(string baseName)
        {
            GroupMatcher<JobKey> matcher = GroupMatcher<JobKey>.GroupContains(baseName);
            scheduler.UnscheduleJobs(
                scheduler.GetTriggersOfJob(
                    new JobKey(CommandType.Exchange.ToString(), baseName)).Select<ITrigger, TriggerKey>(t => t.Key).ToList());
            scheduler.UnscheduleJobs(
                scheduler.GetTriggersOfJob(
                    new JobKey(CommandType.ExtForms.ToString(), baseName)).Select<ITrigger, TriggerKey>(t => t.Key).ToList());
            // под сомнением
            /*scheduler.UnscheduleJobs(
                scheduler.GetTriggersOfJob(
                    new JobKey(CommandType.Checker.ToString(), baseName)).Select<ITrigger, TriggerKey>(t => t.Key).ToList());*/
        }

        public void Stop()
        {
            scheduler.Shutdown();
        }
    }


}
