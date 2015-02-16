using Ugoria.URBD.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Security;
using System.Security.AccessControl;
using System.IO;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for SecureHelperTest and is intended
    ///to contain all SecureHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SecureHelperTest
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
        ///A test for ConvertToSecureString
        ///</summary>
        [TestMethod()]
        public void ConvertToSecureStringTest()
        {
            string line = "//TODO: Initialize to an appropriate value";
            SecureString actual;
            actual = SecureHelper.ConvertToSecureString(line);

            Assert.IsNotNull(actual);
            Assert.AreEqual(line.Length, actual.Length);
        }

        /// <summary>
        ///A test for ConvertUserdomainToClassic
        ///</summary>
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\ADusers.xml", "User", DataAccessMethod.Sequential)]
        [DeploymentItem("UnitTests\\ADusers.xml")]
        [TestMethod]
        public void ConvertUserdomainToClassicTest()
        {
            string userdomain = Convert.ToString(testContextInstance.DataRow["Input"]); // TODO: Initialize to an appropriate value
            string expected = Convert.ToString(testContextInstance.DataRow["Output"]); // TODO: Initialize to an appropriate value
            string actual;
            actual = SecureHelper.ConvertUserdomainToClassic(userdomain);
            Assert.AreEqual(expected, actual);
        }
    }
}
