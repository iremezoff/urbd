using Ugoria.URBD.RemoteService.Strategy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ugoria.URBD.RemoteService.Strategy.Exchange;
using Ugoria.URBD.RemoteService.Strategy.ExtDirectory;
using Ugoria.URBD.Contracts.Data;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for ExtFormsStrategyTest and is intended
    ///to contain all ExtFormsStrategyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ExtDirectoryStrategyTest
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
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        ExtDirectoriesContext context = null;
        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            
        }

        public ExtDirectoryStrategyTest()
        {
            context = new ExtDirectoriesContext
            {
                BasePath = "base_test_path",
                //Command = command,
                Username = @"ugsk\254-svc-urbd",
                Password = "UgskUrbd@54!",
                FtpAddress = "ftp://10.86.1.48",
                FtpAttemptCount = 3,
                FtpDelayTime = 10,
                Files = new List<ExtDirectoriesFile>(),
                Directories = new Dictionary<string, string>() { { "test/extforms", "extforms" }, { "test/extdb", "extdb" } }
            };
            Directory.CreateDirectory(context.BasePath);
        }

        /// <summary>
        ///A test for Conclusion
        ///</summary>
        [TestMethod()]
        public void ConclusionTest()
        {
            ExtDirectoriesStrategy target = new ExtDirectoriesStrategy(context); // TODO: Initialize to an appropriate value
            target.Conclusion();
            Assert.AreEqual(true, target.IsComplete);
        }

        /// <summary>
        ///A test for EndLaunch
        ///</summary>
        [TestMethod()]
        public void EndLaunchTest()
        {
            ExtDirectoriesStrategy target = new ExtDirectoriesStrategy(context); // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual;
            actual = target.EndLaunch();
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Interrupt
        ///</summary>
        [TestMethod()]
        public void InterruptTest()
        {
            ExtDirectoriesStrategy target = new ExtDirectoriesStrategy(context); // TODO: Initialize to an appropriate value
            target.Interrupt();
            Assert.AreEqual(target.IsInterrupt, true);
        }

        /// <summary>
        ///A test for StartLaunch
        ///</summary>
        [TestMethod()]
        public void StartLaunchTest()
        {
            ExtDirectoriesStrategy target = new ExtDirectoriesStrategy(context); // TODO: Initialize to an appropriate value
            target.StartLaunch();
            Assert.AreNotEqual(context.StartTime, DateTime.MinValue);
            Assert.AreNotEqual(context.LaunchGuid, Guid.Empty);
        }
    }
}
