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
using Ugoria.URBD.Core;

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
        public void AddScheduleLaunch(Action<int, CommandType, ModeType, int> commanAction,
            IConfiguration schedCfg,
            CommandType commandType,
            Action<int, CommandType> checkAction,
            int delayCheck)
        {
            IJobDetail jobDetail = GetJobDetail(schedCfg.GetParameter("base_id").ToString(), commandType);

            string triggerName = "";
            if (commandType == CommandType.Exchange)
                triggerName = String.Format("{0}_{1}_{2}",
                    schedCfg.GetParameter("base_id"),
                    schedCfg.GetParameter("time"),
                    schedCfg.GetParameter("mode"));
            else
                triggerName = String.Format("{0}_{1}", schedCfg.GetParameter("base_id"), schedCfg.GetParameter("time"));

            ITrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, commandType.ToString())
                                                      .WithCronSchedule(SchedulerUtil.CronExpressionBuild((string)schedCfg.GetParameter("time")))
                                                      .ForJob(jobDetail)
                                                      .Build();

            trigger.JobDataMap["command_type"] = commandType;
            trigger.JobDataMap["cfg"] = schedCfg;
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
            IConfiguration cfg = (IConfiguration)sender.JobDataMap["cfg"];
            AddScheduleLaunch((Action<int, CommandType>)sender.JobDataMap["check_action"],
                (int)cfg.GetParameter("base_id"),
                (CommandType)sender.JobDataMap["command_type"],
                (int)sender.JobDataMap["delay_check"]);
        }

        // для запусков заданий проверок (единовременные запуски)
        public void AddScheduleLaunch(Action<int, CommandType> checkAction, int baseId, CommandType commandType, int delayCheck)
        {
            IJobDetail jobDetail = GetJobDetail(baseId.ToString(), CommandType.Checker);

            string triggerName = String.Format("{0}_checker_{1:yyyy-MM-dd_HHmmss-ffffff}", baseId, DateTime.Now);

            ITrigger trigger = (ISimpleTrigger)TriggerBuilder.Create()
                                                      .WithIdentity(triggerName, CommandType.Checker.ToString())
                                                      .StartAt(DateBuilder.FutureDate(delayCheck, IntervalUnit.Minute))  // временной промежуток
                                                      .WithSimpleSchedule()
                                                      .ForJob(jobDetail)
                                                      .Build();

            trigger.JobDataMap["action"] = checkAction;
            trigger.JobDataMap["command_type"] = commandType;

            scheduler.ScheduleJob(trigger);
        }

        private IJobDetail GetJobDetail(string jobName, CommandType type)
        {
            JobKey jobKey = new JobKey(type.ToString(), jobName);
            IJobDetail jobDetail = null;
            if (!scheduler.CheckExists(jobKey)) // проверка на наличие у шедулера текущего задания
            {
                JobBuilder builder = null;// jobDetail = JobBuilder.Create<ExchangeJob>();
                switch (type)
                {
                    case CommandType.Exchange:
                        builder = JobBuilder.Create<ExchangeJob>();
                        break;
                    case CommandType.ExtForms:
                        builder = JobBuilder.Create<ExtFormsJob>();
                        break;
                    case CommandType.Checker:
                        builder = JobBuilder.Create<CheckJob>();
                        break;
                }
                jobDetail = builder.WithIdentity(jobKey).Build();
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
