using Ugoria.URBD.CentralService.DataProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Ugoria.URBD.Contracts.Data.Commands;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for DBDataProviderTest and is intended
    ///to contain all DBDataProviderTest Unit Tests
    ///</summary>
    [TestClass()]
    [DeploymentItem("app.config")]
    public class DBDataProviderTest
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
        string address = "127.0.0.1:7000";
        Guid exchGuid = Guid.NewGuid();
        Guid extGuid = Guid.NewGuid();
        Guid lGuid1 = Guid.NewGuid();
        Guid lGuid2 = Guid.NewGuid();
        DateTime launchDate1 = DateTime.Now.AddMinutes(-1);
        DateTime launchDate2 = DateTime.Now.AddMinutes(0.5);
        DateTime commandDate = DateTime.Now.AddMinutes(-2);
        int pid = 1000;
        int baseId = 5;
        int userId = 2;
        DBDataProvider target = null;

        ExchangeCommand command;

        TransactionScope scope = null;
        [TestInitialize()]
        public void MyTestInitialize()
        {
            scope = new TransactionScope();
            target = new DBDataProvider(); // TODO: Initialize to an appropriate value

            target.SetCommandReport(userId, exchGuid, baseId, commandDate, "Exchange");
            target.SetCommandReport(userId, extGuid, baseId, commandDate, "ExtDirectories");
            target.SetPID1C(exchGuid, lGuid1, launchDate1, pid);
            target.SetPID1C(extGuid, lGuid2, launchDate2, pid);

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
        }
        //
        //Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup()
        {
            target.Dispose();
            scope.Dispose();
        }
        //
        #endregion


        /// <summary>
        ///A test for GetSettings
        ///</summary>
        [TestMethod()]
        public void GetSettingsTest()
        {
            string key = "main.username"; // TODO: Initialize to an appropriate value
            DataTable actual = target.GetSettings(key);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Rows.Count > 0);
        }

        /// <summary>
        ///A test for GetScheduleExtForms
        ///</summary>
        [TestMethod()]
        public void GetScheduleExtDirectoriesTest()
        {
            DataTable actual = target.GetScheduleExtDirectories(baseId);
            Assert.IsNotNull(actual);

            using (SqlConnection conn = target.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select count(*) from ScheduleExtDirectories where base_id = @base_id";
                cmd.Parameters.AddWithValue("@base_id", baseId);
                int count = (int)cmd.ExecuteScalar();

                Assert.AreEqual(count, actual.Rows.Count);
            }
        }

        /// <summary>
        ///A test for GetServiceSettings
        ///</summary>
        [TestMethod()]
        public void GetServiceSettingsTest()
        {
            int tables = 3;
            DataSet actual = target.GetServiceSettings(address);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Tables.Count == tables);
        }

        /// <summary>
        ///A test for GetScheduleExchangeData
        ///</summary>
        [TestMethod()]
        public void GetScheduleExchangeDataTest()
        {
            DataTable actual;
            actual = target.GetScheduleExchangeData(baseId);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Rows.Count > 0);
        }

        /// <summary>
        ///A test for GetScheduleData
        ///</summary>
        [TestMethod()]
        public void GetScheduleDataTest()
        {
            int tables = 3;
            DataSet actual;
            actual = target.GetScheduleData(baseId);
            Assert.IsTrue(actual.Tables.Count == tables);
        }

        /// <summary>
        ///A test for GetReport
        ///</summary>
        [TestMethod()]
        public void GetReportTest()
        {
            DataTable actual;

            Guid emptyGuid = Guid.NewGuid();
            actual = target.GetReport(emptyGuid);
            Assert.IsTrue(actual.Rows.Count == 0);

            actual = target.GetReport(extGuid);
            Assert.IsTrue(actual.Rows.Count > 0);
        }

        /// <summary>
        ///A test for GetPreparedExchangeComman
        ///</summary>
        [TestMethod()]
        public void GetPreparedCommandTest()
        {
            // Negative test
            DataTable actual = target.GetPreparedCommand(baseId, "Exchange").Tables[0];
            Assert.IsTrue(actual.Rows.Count > 0);
            Assert.AreEqual(exchGuid, actual.Rows[0]["guid"]);
            Assert.AreEqual(DBNull.Value, actual.Rows[0]["date_complete"]);
        }

        /// <summary>
        ///A test for GetLaunchReport
        ///</summary>
        [TestMethod()]
        public void GetLaunchReportTest()
        {
            DataRow actual;

            // Exchange
            actual = target.GetLaunchReport(baseId, "Exchange").Tables[0].Rows[0];
            Assert.IsNotNull(actual);
            Assert.AreEqual(exchGuid, actual["report_guid"]);
            Assert.AreEqual(lGuid1, actual["launch_guid"]);
            Assert.AreEqual(pid, actual["pid"]);
            Assert.IsTrue(((DateTime)actual["date_start"] - launchDate1).TotalSeconds < 1);

            // ExtDirectories
            actual = target.GetLaunchReport(baseId, "ExtDirectories").Tables[0].Rows[0];
            Assert.IsNotNull(actual);
            Assert.AreEqual(extGuid, actual["report_guid"]);
            Assert.AreEqual(lGuid2, actual["launch_guid"]);
            Assert.AreEqual(pid, actual["pid"]);
            Assert.IsTrue(((DateTime)actual["date_start"] - launchDate2).TotalSeconds < 1);
        }

        /// <summary>
        ///A test for GetBase
        ///</summary>
        [TestMethod()]
        public void GetBaseTest()
        {
            DataRow actual;
            actual = target.GetBase(baseId);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for GetAlarmList
        ///</summary>
        [TestMethod()]
        public void GetAlarmListTest()
        {
            DataTable actual;
            actual = target.GetAlarmList(exchGuid);
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Rows.Count > 0);
        }

        /// <summary>
        ///A test for SetLog
        ///</summary>
        [TestMethod()]
        public void SetLogTest()
        {
            DateTime messageDate = DateTime.Now;
            char status = 'I';
            string message = "test message";
            target.SetLog(address, messageDate, status, message);

            using (SqlConnection conn = target.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select text, date, status from ServiceLog sl inner join Service s on s.service_id = sl.service_id where s.address = @addr and sl.log_id=(select max(log_id) from ServiceLog)";
                cmd.Parameters.AddWithValue("@addr", address);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataTable);

                Assert.IsTrue(dataTable.Rows.Count > 0);
                Assert.AreEqual(message, dataTable.Rows[0]["text"]);
                Assert.AreEqual(status.ToString(), dataTable.Rows[0]["status"]);
                Assert.IsTrue(((DateTime)dataTable.Rows[0]["date"] - messageDate).TotalSeconds < 1);
            }
        }

        /// <summary>
        ///A test for SetPID1C
        ///</summary>
        [TestMethod()]
        public void SetPID1CTest()
        {
            Guid lGuid = Guid.NewGuid();
            DateTime startDate = DateTime.Now.AddMinutes(3);
            target.SetPID1C(exchGuid, lGuid, startDate, pid);

            using (SqlConnection conn = target.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select date_start, pid, l.guid from Report r inner join Launch1C l on l.report_id = r.report_id where r.guid = @guid order by launch_id desc";
                cmd.Parameters.AddWithValue("@guid", exchGuid);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataTable);

                Assert.IsTrue(dataTable.Rows.Count > 0);
                Assert.AreEqual(lGuid, dataTable.Rows[0]["guid"]);
                Assert.AreEqual(pid, dataTable.Rows[0]["pid"]);
                Assert.IsTrue(((DateTime)dataTable.Rows[0]["date_start"] - startDate).TotalSeconds < 1);
            }
        }

        /// <summary>
        ///A test for SetReport
        ///</summary>
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\ComponentReport.xml", "Report", DataAccessMethod.Sequential)]
        [DeploymentItem("UnitTests\\ComponentReport.xml")]
        [TestMethod()]
        public void SetReportTest()
        {
            DateTime completeDate = DateTime.Now;
            string status = Convert.ToString(testContextInstance.DataRow["Status"]);
            string message = "message";
            string mdRelease = "1.80";
            DateTime releaseDate = DateTime.Now.AddDays(-1);

            Guid guid = "Exchange".Equals(Convert.ToString(testContextInstance.DataRow["Component"])) ? exchGuid : extGuid;

            target.SetReport(guid, completeDate, status, message);//, mdRelease, releaseDate);

            using (SqlConnection conn = target.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select md_release, message, date_release, date_complete, rs.name status, c.name component from Report r inner join Base b on b.base_id = r.base_id inner join ComponentReportStatus crs on crs.component_status_id = r.component_status_id inner join ReportStatus rs on rs.status_id = crs.status_id inner join Component c on c.component_id = crs.component_id where r.guid = @guid";
                cmd.Parameters.AddWithValue("@guid", guid);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataTable);

                bool isNotNull = Convert.ToBoolean(testContextInstance.DataRow["IsNotNull"]);

                if (!isNotNull)
                {
                    Assert.AreEqual(DBNull.Value, dataTable.Rows[0]["date_complete"]);
                    return;
                }

                Assert.IsTrue(dataTable.Rows.Count > 0);
                Assert.AreEqual(mdRelease, dataTable.Rows[0]["md_release"]);
                StringAssert.Equals(Convert.ToString(testContextInstance.DataRow["Component"]), dataTable.Rows[0]["component"]);
                Assert.AreEqual(message, dataTable.Rows[0]["message"]);
                Assert.AreEqual(status, dataTable.Rows[0]["status"]);
                Assert.IsTrue(((DateTime)dataTable.Rows[0]["date_complete"] - completeDate).TotalSeconds < 1);
            }
        }

        /// <summary>
        ///A test for SetReportFile
        ///</summary>
        [TestMethod()]
        public void SetReportFileTest()
        {
            string filename = "yeees";
            DateTime createdDate = DateTime.Now.AddDays(-5);
            long size = 123;
            target.SetReportFile(extGuid, filename, createdDate, size);

            using (SqlConnection conn = target.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select filename, size, date_copied from Report r inner join ExtDirectoryFile edf on edf.report_id = r.report_id where r.guid = @guid";
                cmd.Parameters.AddWithValue("@guid", extGuid);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataTable);

                Assert.IsTrue(dataTable.Rows.Count > 0);
                Assert.AreEqual(filename, dataTable.Rows[0]["filename"]);
                Assert.AreEqual(size, dataTable.Rows[0]["size"]);
                Assert.IsTrue(((DateTime)dataTable.Rows[0]["date_copied"] - createdDate).TotalSeconds < 1);
            }
        }

        /// <summary>
        ///A test for SetReportPacket
        ///</summary>
        [TestMethod()]
        public void SetReportPacketTest()
        {
            string filename = @"CP\PSKOV_C.zip";
            DateTime packetDate = DateTime.Now.AddDays(-10);
            char type = 'U';
            long filesize = 123456;
            target.SetReportPacket(exchGuid, filename, packetDate, type, filesize);

            using (SqlConnection conn = target.Connection)
            {
                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select size, date_created from Report r inner join ReportPacket rp on rp.report_id = r.report_id where r.guid = @guid";
                cmd.Parameters.AddWithValue("@guid", exchGuid);

                DataTable dataTable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataTable);

                Assert.IsTrue(dataTable.Rows.Count > 0);
                Assert.AreEqual(filesize, dataTable.Rows[0]["size"]);
                Assert.IsTrue(((DateTime)dataTable.Rows[0]["date_created"] - packetDate).TotalSeconds < 1);
            }
        }
    }
}
