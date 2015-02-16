using Ugoria.URBD.RemoteService.Kit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Threading;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for Process1CLauncherKitTest and is intended
    ///to contain all Process1CLauncherKitTest Unit Tests
    ///</summary>
    [TestClass()]
    public class Process1CLauncherKitTest
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
        string path1c = @"C:\Program Files\1Cv77\BIN\1cv7s.exe";
        string user1c = "Администратор";
        string password1c = "gfhjkm911";
        string basePath = @"c:\ITS_401_1C_Pskov_Centr\";
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
        ///A test for Start
        ///</summary>
        [TestMethod]
        [DeploymentItem("test.prm")]
        public void StartTest()
        {
            Process1CLauncherKit target = new Process1CLauncherKit(path1c, user1c, password1c, basePath, LaunchMode.Config);
            
            int expected = 0;
            int actual = 0;
            // Negative test
            try
            {
                actual = target.Start();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }

            // Positive test
            target.PrmFile = "test.prm";
            actual = target.Start();
            Assert.AreNotEqual(expected, actual);
            Process process = Process.GetProcessById(actual);
            Assert.AreEqual(actual, process.Id);
        }

        /// <summary>
        ///A test for Interrupt
        ///</summary>
        [TestMethod]
        [DeploymentItem("test.prm")]
        public void InterruptTest()
        {
            Process1CLauncherKit target = new Process1CLauncherKit(path1c, user1c, password1c, basePath, LaunchMode.Config);
            int actual = 1;
            // Negative test
            try
            {
                actual = target.Start();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }

            // Positive test
            target.PrmFile = "test.prm";
            actual = target.Start();
            target.Interrupt();
            try
            {
                Thread.Sleep(2000);
                Process.GetProcessById(actual);               
                Assert.Fail("Процесс ещё работает");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        /// <summary>
        ///A test for EndStart
        ///</summary>
        [TestMethod()]
        [DeploymentItem("test.prm")]
        public void EndStartTest()
        {
            Process1CLauncherKit target = new Process1CLauncherKit(path1c, user1c, password1c, basePath, LaunchMode.Config);
            target.PrmFile = "test.prm";
            int actual = target.BeginStart(null);
            target.EndStart(null);
            try
            {
                Process.GetProcessById(actual);
                Assert.Fail("Процесс ещё работает");
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }
    }
}
