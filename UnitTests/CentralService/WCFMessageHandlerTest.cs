using Ugoria.URBD.CentralService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ugoria.URBD.CentralService.DataProvider;
using System.Collections.Generic;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for WCFMessageHandlerTest and is intended
    ///to contain all WCFMessageHandlerTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("app.config")]
    public class WCFMessageHandlerTest
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
        ExchangeCommand exchCommand = null;
        ExtDirectoriesCommand extCommand = null;
        LaunchReport launchReport = null;
        OperationReport exchReport;
        OperationReport extReport;


        public WCFMessageHandlerTest()
        {
            exchCommand = new ExchangeCommand
            {
                baseId = 5,
                reportGuid = Guid.NewGuid(),
                commandDate = DateTime.Now.AddSeconds(-10),
                modeType = Ugoria.URBD.Contracts.Services.ModeType.Passive,
                pools = new System.Collections.Generic.List<int>(),
                releaseUpdate = DateTime.Now.AddDays(-10),
                configurationChangeDate = DateTime.Now.AddDays(-5)
            };

            extCommand = new ExtDirectoriesCommand
            {
                baseId = 5,
                reportGuid = Guid.NewGuid(),
                commandDate = DateTime.Now.AddSeconds(-10),
                pools = new System.Collections.Generic.List<int>(),
                configurationChangeDate = DateTime.Now.AddDays(-5)
            };

            launchReport = new LaunchReport
            {
                baseId = exchCommand.baseId,
                commandDate = exchCommand.commandDate,
                launchGuid = Guid.NewGuid(),
                pid = 1000,
                reportGuid = exchCommand.reportGuid,
                startDate = DateTime.Now
            };

            exchReport = new ExchangeReport
            {
                baseId = exchCommand.baseId,
                commandDate = exchCommand.commandDate,
                dateComplete = DateTime.Now,
                status = ExchangeReportStatus.Warning,
                dateRelease = DateTime.Now.AddDays(-1),
                mdRelease = "1.99",
                message = "super good",
                reportGuid = exchCommand.reportGuid
            };

            extReport = new ExtDirectoriesReport
            {
                baseId = extCommand.baseId,
                commandDate = extCommand.commandDate,
                dateComplete = DateTime.Now,
                status = ExtDirectoriesReportStatus.Fail,
                message = "super good ext",
                reportGuid = extCommand.reportGuid,
                files = new List<Ugoria.URBD.Contracts.Data.ExtDirectoriesFile>()
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


        /// <summary>
        ///A test for PrepareCommand
        ///</summary>
        [TestMethod()]
        public void PrepareCommandTest()
        {
            List<DataHandler> handlers = new List<DataHandler>() { new ExchangeDataHandler() };
            WCFMessageHandler target = new WCFMessageHandler(handlers);
            ExecuteCommand actual;

            // Positive test
            actual = target.PrepareCommand(exchCommand);
            Assert.IsNotNull(actual);

            // Negative test
            try
            {
                actual = target.PrepareCommand(extCommand);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(URBDException));
            }
        }

        /// <summary>
        ///A test for HandleReport
        ///</summary>
        [TestMethod()]
        public void HandleReportTest()
        {
            List<DataHandler> handlers = new List<DataHandler>();
            WCFMessageHandler target = new WCFMessageHandler(handlers);
            ReportStatus actual;

            // Negative tests
            // LaunchReport
            try
            {
                actual = target.HandleReport(launchReport);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(URBDException));
            }
            // ExchangeReport
            try
            {
                actual = target.HandleReport(exchReport);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(URBDException));
            }

            // ExtReport
            try
            {
                actual = target.HandleReport(extReport);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(URBDException));
            }

            // Positive tests
            handlers = new List<DataHandler>() { new ExchangeDataHandler(), new ExtDirectoriesDataHandler() };
            target = new WCFMessageHandler(handlers);

            // LaunchReport
            actual = target.HandleReport(launchReport);
            Assert.IsNotNull(actual);

            // ExchangeReport
            actual = target.HandleReport(exchReport);
            Assert.IsNotNull(actual);

            // ExtReport
            actual = target.HandleReport(extReport);
            Assert.IsNotNull(actual);
        }
    }
}
