using Ugoria.URBD.CentralService.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ugoria.URBD.Contracts.Data.Commands;
using Quartz;
using Quartz.Impl.Matchers;
using System.Linq;
using Quartz.Util;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for SchedulerManagerTest and is intended
    ///to contain all SchedulerManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SchedulerManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        ExecuteCommand cmd = null;
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            
        }

        public SchedulerManagerTest()
        {
            cmd = new ExecuteCommand
            {
                baseId = 5,
                commandDate = DateTime.Now
            };
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        SchedulerManager target = null;
        [TestInitialize()]
        public void MyTestInitialize()
        {
            target = new SchedulerManager(); // TODO: Initialize to an appropriate value            
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
        }
        //
        #endregion



        //public void AddScheduleLaunch(Action<ExecuteCommand> commanAction, ExecuteCommand command, string time, Action<ExecuteCommand> checkAction)
        [TestMethod]
        public void AddScheduleLaunchTest()
        {
            Action<ExecuteCommand> commandAction = (command) => { };
            string time = "12:00";
            Action<ExecuteCommand> checkAction = (command) => { };
            target.AddScheduleLaunch(commandAction, cmd, time, checkAction);

            IScheduler sched = target.Scheduler;

            JobKey jobKey = new JobKey(cmd.GetType().Name, cmd.baseId.ToString());
            TriggerKey triggerKey = new TriggerKey(String.Format("{0}_{1}", cmd.baseId, time), cmd.GetType().Name);

            ITrigger trigger = sched.GetTriggersOfJob(jobKey).FirstOrDefault();
            Assert.IsNotNull(trigger);
            DirtyFlagMap<string, object> map = new DirtyFlagMap<string, object>();
            map.Add("action", commandAction);
            Assert.IsTrue(trigger.JobDataMap.ContainsKey("action"));
            Assert.IsTrue(trigger.JobDataMap.ContainsValue(commandAction));
            Assert.IsTrue(trigger.JobDataMap.ContainsKey("check_action"));
            Assert.IsTrue(trigger.JobDataMap.ContainsValue(checkAction));
            Assert.IsTrue(trigger.JobDataMap.ContainsKey("command"));
            Assert.IsTrue(trigger.JobDataMap.ContainsValue(cmd));

            StringAssert.Equals(trigger.JobKey.Name, jobKey.Name);
            StringAssert.Equals(trigger.JobKey.Group, jobKey.Group);
        }

        [TestMethod]
        public void AddScheduleLaunchTest1()
        {
            Action<ExecuteCommand> checkAction = (command) => { };

            target.AddScheduleLaunch(checkAction, cmd);

            IScheduler sched = target.Scheduler;

            JobKey jobKey = new JobKey("checker", cmd.baseId.ToString());
            TriggerKey triggerKey = new TriggerKey(String.Format("{0}_checker_{1:yyyy-MM-dd_HHmmss-ffffff}", cmd.baseId, DateTime.Now), cmd.GetType().Name);

            ITrigger trigger = sched.GetTriggersOfJob(jobKey).FirstOrDefault();
            Assert.IsNotNull(trigger);
            Assert.IsTrue(trigger.JobDataMap.ContainsKey("action"));
            Assert.IsTrue(trigger.JobDataMap.ContainsValue(checkAction));
            Assert.IsTrue(trigger.JobDataMap.ContainsKey("command"));
            Assert.IsTrue(trigger.JobDataMap.ContainsValue(cmd));
            StringAssert.Equals("checker", trigger.Key.Group);
        }

        /// <summary>
        ///A test for RemoveScheduleLaunch
        ///</summary>
        [TestMethod()]
        public void RemoveScheduleLaunchTest()
        {
            string groupName = cmd.baseId.ToString();

            Action<ExecuteCommand> commandAction = (command) => { };
            string time = "12:00";
            Action<ExecuteCommand> checkAction = (command) => { };
            target.AddScheduleLaunch(commandAction, cmd, time, checkAction);

            IScheduler sched = target.Scheduler;

            JobKey jobKey = new JobKey(cmd.GetType().Name, cmd.baseId.ToString());
            TriggerKey triggerKey = new TriggerKey(String.Format("{0}_{1}", cmd.baseId, time), cmd.GetType().Name);

            IJobDetail jobDetail = sched.GetJobDetail(jobKey);
            Assert.IsNotNull(jobDetail);

            target.RemoveScheduleLaunch(groupName);

            GroupMatcher<JobKey> matcher = GroupMatcher<JobKey>.GroupEquals(groupName);
            jobDetail = sched.GetJobDetail(jobKey);
            Assert.IsTrue(sched.GetJobKeys(matcher).Count == 0);
            Assert.IsNull(jobDetail);
        }
    }
}
