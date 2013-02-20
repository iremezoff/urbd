using Ugoria.URBD.RemoteService.Strategy.Exchange.Mode;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ugoria.URBD.RemoteService.Strategy.Exchange;
using System.IO;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for NormalModeTest and is intended
    ///to contain all NormalModeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NormalModeTest
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
        string basepath = @"c:\ITS_401_1C_Pskov_Centr";

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
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
        ///A test for CompleteExchange
        ///</summary>
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\Modes.xml", "Mode", DataAccessMethod.Sequential)]
        [DeploymentItem("UnitTests\\Modes.xml")]
        [TestMethod]
        public void CompleteExchangeTest()
        {
            string logFilename = Convert.ToString(testContextInstance.DataRow["FileName1"]);
            Verifier verifier = new Verifier("mode_log.txt");
            File.Copy(logFilename, "mode_log.txt", true);
            int waitTime = 1;
            NormalMode target = new NormalMode(verifier, basepath, waitTime);
            bool haveMD = Convert.ToBoolean(testContextInstance.DataRow["HaveMD"]);
            bool expected = Convert.ToBoolean(testContextInstance.DataRow["Success"]);
            DateTime timeStart = DateTime.Now;
            bool actual = target.CompleteExchange(haveMD);
            DateTime timeEnd = DateTime.Now;
            Assert.AreEqual(expected, actual);
            if (!actual)
                Assert.IsTrue((timeEnd - timeStart).TotalMinutes >= waitTime && (timeEnd - timeStart).Seconds <= waitTime * 60 / 6);
            else
                Assert.IsTrue((timeEnd - timeStart).TotalSeconds <= 1);
            if (actual)
                return;
            if (haveMD)
                Assert.IsTrue(File.Exists(string.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath)));
            string logFilename2 = Convert.ToString(testContextInstance.DataRow["FileName2"]);
            File.Copy(logFilename2, "mode_log.txt", true);

            timeStart = DateTime.Now;
            actual = target.CompleteExchange(haveMD);
            timeEnd = DateTime.Now;
            Assert.IsTrue((timeEnd - timeStart).TotalSeconds < 1);
            if (haveMD)
                Assert.IsTrue(!File.Exists(string.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath)));
            Assert.IsTrue(actual);
        }
    }
}
