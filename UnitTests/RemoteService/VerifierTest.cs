using Ugoria.URBD.RemoteService.Strategy.Exchange;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Ugoria.URBD.Contracts.Services;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for VerifierTest and is intended
    ///to contain all VerifierTest Unit Tests
    ///</summary>
    [TestClass()]
    public class VerifierTest
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


        /// <summary>
        ///A test for Verification
        ///</summary>
        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\MlgFiles.xml", "TestFile", DataAccessMethod.Sequential)]
        [DeploymentItem("UnitTests\\MlgFiles.xml")]
        [DeploymentItem("TestMLG")]
        public void VerificationTest()
        {
            string logFilename = Convert.ToString(testContextInstance.DataRow["FileName"]); // TODO: Initialize to an appropriate value
            FileInfo fi = new FileInfo(logFilename);
            fi = fi.CopyTo(logFilename + "0", true);

            Verifier target = new Verifier(logFilename);
            fi.CopyTo(logFilename, true);

            target.Verification();

        IEnumerable<DataRow> rowsList = testContextInstance.DataRow.GetChildRows("TestFile_Packet");

            Assert.AreEqual(rowsList.Count(), target.MlgReport.Count);
            foreach (DataRow childRow in rowsList)
            {
                PacketInfo pi = target.MlgReport.FirstOrDefault(p => p.Key.Equals(Convert.ToString(childRow["Name"]))).Value;
                Assert.IsNotNull(pi);
                Assert.AreEqual(pi.type, Enum.Parse(typeof(PacketType), Convert.ToString(childRow["Type"])));
                Assert.AreEqual(pi.isSuccess, Convert.ToBoolean(childRow["Success"]));
            }
            //Assert.AreNotEqual(testContextInstance.DataRow.GetChildRows(["FileName"]), target.MlgReport.Count);
        }

        /// <summary>
        ///A test for Verifier Constructor
        ///</summary>
        [TestMethod]
        public void VerifierConstructorTest()
        {
            string logFilename = string.Empty; // TODO: Initialize to an appropriate value

            string filename = "test.txt";
            FileInfo f = new FileInfo(filename);
            using (FileStream fs = f.Create())
            {
                fs.Write(new byte[] { 0, 1, 2, 3 }, 0, 4); // заполянем файл данными
            }
            Verifier target = new Verifier(filename);

            bool expected = true;
            bool actual = f.Exists;
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(0, f.Length); // файл должен быть пустым
        }
    }
}
