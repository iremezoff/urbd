﻿using Ugoria.URBD.CentralService.DataProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers;
using System.Transactions;
using System.Data.SqlClient;
using System.Data;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for ExchangeDataHandlerTest and is intended
    ///to contain all ExchangeDataHandlerTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("app.config")]
    public class ExchangeDataHandlerTest
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
        public ExchangeDataHandlerTest()
        {
            command = new ExchangeCommand
                {
                    baseId = 5,
                    reportGuid = Guid.NewGuid(),
                    commandDate = DateTime.Now.AddSeconds(-10),
                    modeType = Ugoria.URBD.Contracts.Services.ModeType.Passive,
                    pools = new System.Collections.Generic.List<int>(),
                    releaseUpdate = DateTime.Now.AddDays(-10),
                    configurationChangeDate = DateTime.Now.AddDays(-5)
                };
            launchReport = new LaunchReport
                {
                    baseId = command.baseId,
                    commandDate = command.commandDate,
                    launchGuid = Guid.NewGuid(),
                    pid = 1000,
                    reportGuid = command.reportGuid,
                    startDate = DateTime.Now
                };
        }
        int userId = 2;
        ExecuteCommand command = null;
        LaunchReport launchReport = null;
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
        ExchangeDataHandler target = null;
        TransactionScope scope = null;
        [TestInitialize()]
        public void MyTestInitialize()
        {
            scope = new TransactionScope();
            target = new ExchangeDataHandler(); // TODO: Initialize to an appropriate value
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            scope.Dispose();
        }
        //
        #endregion

        /// <summary>
        ///A test for GetLaunchReport
        ///</summary>
        [TestMethod()]
        public void GetLaunchReportTest()
        {
            LaunchReport expected = null; // TODO: Initialize to an appropriate value
            LaunchReport actual;

            // Negative test
            actual = target.GetLaunchReport(command);
            Assert.IsNull(actual);

            // Half-negative test
            target.SetCommandReport(command, userId);
            actual = target.GetLaunchReport(command);
            Assert.IsNotNull(actual);
            Assert.AreEqual(DateTime.MinValue, actual.startDate);
            Assert.AreEqual(Guid.Empty, actual.launchGuid);
            Assert.AreEqual(0, actual.pid);
            Assert.AreEqual(command.reportGuid, actual.reportGuid);
            Assert.IsTrue((actual.commandDate - command.commandDate).TotalSeconds < 1);

            // Positive test
            target.SetLaunchReport(launchReport);
            actual = target.GetLaunchReport(command);
            Assert.IsNotNull(actual);
            Assert.AreEqual(launchReport.launchGuid, actual.launchGuid);
            Assert.AreEqual(launchReport.reportGuid, actual.reportGuid);
            Assert.AreEqual(launchReport.pid, actual.pid);
            Assert.IsTrue((actual.commandDate - launchReport.commandDate).TotalSeconds < 1);
            Assert.IsTrue((actual.startDate - launchReport.startDate).TotalSeconds < 1);
            Assert.AreEqual(launchReport.baseId, actual.baseId);
        }

        /// <summary>
        ///A test for GetPreparedCommand
        ///</summary>
        [TestMethod()]
        public void GetPreparedCommandTest()
        {
            ExecuteCommand actual;

            // Positive test
            actual = target.GetPreparedCommand(command);
            Assert.AreEqual(command.baseId, actual.baseId);
            Assert.IsTrue((actual.commandDate - DateTime.Now).TotalMinutes < 1);
            Assert.AreNotEqual(Guid.Empty, actual.reportGuid);
            Assert.AreNotEqual(command.reportGuid, actual.reportGuid);
            Assert.AreNotEqual(DateTime.MinValue, actual.configurationChangeDate);

            // Negative test
            target.SetCommandReport(command);
            actual = target.GetPreparedCommand(command);
            Assert.AreEqual(DateTime.MinValue, actual.commandDate);
            Assert.AreEqual(command.reportGuid, actual.reportGuid);
        }

        /// <summary>
        ///A test for GetPreparedExchangeCommand
        ///</summary>
        [TestMethod()]
        public void GetPreparedExchangeCommandTest()
        {
            ExchangeCommand actual;

            actual = target.GetPreparedExchangeCommand((ExchangeCommand)command);
            Assert.AreEqual(((ExchangeCommand)command).modeType, actual.modeType);
            Assert.AreNotEqual(DateTime.MinValue, ((ExchangeCommand)command).releaseUpdate);
        }

        /// <summary>
        ///A test for SetCommandReport
        ///</summary>
        [TestMethod()]
        public void SetCommandReportTest()
        {
            ExecuteCommand actual;
            target.SetCommandReport(command, userId);

            using (SqlConnection conn = DB.Instance.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED select base_id, guid, date_command, date_complete, message, user_id, c.name component, rs.name status from Report r inner join ComponentReportStatus crs on crs.component_status_id=r.component_status_id inner join ReportStatus rs on rs.status_id = crs.status_id inner join Component c on c.component_id = crs.component_id where r.guid =  @guid";
                cmd.Parameters.AddWithValue("@guid", command.reportGuid);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataTable);

                Assert.IsTrue(dataTable.Rows.Count > 0);
                Assert.AreEqual(command.baseId, dataTable.Rows[0]["base_id"]);
                Assert.AreEqual(command.reportGuid, dataTable.Rows[0]["guid"]);
                Assert.IsTrue(((DateTime)dataTable.Rows[0]["date_command"] - command.commandDate).TotalSeconds < 1);
                Assert.AreEqual(DBNull.Value, dataTable.Rows[0]["date_complete"]);
                Assert.AreEqual(DBNull.Value, dataTable.Rows[0]["message"]);
                Assert.AreEqual(userId, dataTable.Rows[0]["user_id"]);
                StringAssert.Equals("Exchange", dataTable.Rows[0]["component"]);
                StringAssert.Equals("Busy", dataTable.Rows[0]["status"]);
            }
        }

        /// <summary>
        ///A test for SetReport
        ///</summary>
        [TestMethod()]
        public void SetReportTest()
        {
            command.reportGuid = Guid.NewGuid();
            OperationReport report = new ExchangeReport
            {
                baseId = command.baseId,
                commandDate = command.commandDate,
                dateComplete = DateTime.Now,
                status = ReportStatus.Warning,
                dateRelease = DateTime.Now.AddDays(-1),
                mdRelease = "1.99",
                message = "super good",
                reportGuid = command.reportGuid
            };
            ReportStatus expected = ReportStatus.Warning; // TODO: Initialize to an appropriate value
            ReportStatus actual;
            target.SetReport(report);

            //Assert.AreEqual(expected, actual);

            using (SqlConnection conn = DB.Instance.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED select base_id, guid, date_command, date_complete, message, user_id, c.name component, rs.name status from Report r inner join ComponentReportStatus crs on crs.component_status_id=r.component_status_id inner join ReportStatus rs on rs.status_id = crs.status_id inner join Component c on c.component_id = crs.component_id where r.guid =  @guid";
                cmd.Parameters.AddWithValue("@guid", command.reportGuid);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);

                // Negative test
                adapter.Fill(dataTable);
                Assert.IsTrue(dataTable.Rows.Count == 0);

                // Positive test
                target.SetCommandReport(command, userId);
                target.SetReport(report);
                adapter.Fill(dataTable);
                Assert.IsTrue(dataTable.Rows.Count > 0);

                DataRow dataRow = dataTable.Rows[0];
                Assert.AreEqual(command.baseId, dataRow["base_id"]);
                Assert.AreEqual(command.reportGuid, dataRow["guid"]);
                Assert.IsTrue(((DateTime)dataRow["date_command"] - report.commandDate).TotalSeconds < 1);
                Assert.IsTrue(((DateTime)dataRow["date_complete"] - report.dateComplete).TotalSeconds < 1);
                Assert.AreEqual(userId, dataRow["user_id"]);
                StringAssert.Equals("Exchange", dataRow["component"]);
                StringAssert.Equals(((ExchangeReport)report).status.ToString(), dataRow["status"]);
                StringAssert.Equals(((ExchangeReport)report).message, dataRow["message"]);
            }
        }
    }
}
