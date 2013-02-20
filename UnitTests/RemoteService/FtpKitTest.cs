using Ugoria.URBD.RemoteService.Kit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;

namespace UnitTests
{


    /// <summary>
    ///This is a test class for FtpKitTest and is intended
    ///to contain all FtpKitTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FtpKitTest
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
        string address = "ftp://10.86.1.48";
        NetworkCredential credential = new NetworkCredential(@"ugsk\254-svc-urbd", "UgskUrbd@54!");
        Uri ftpFile = null;
        Uri localTempFile = null;
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test

        public FtpKitTest()
        {
            WebClient ftpClient = new WebClient();
            ftpClient.Credentials = credential;
            ftpFile = new Uri(address + "/test/" + Guid.NewGuid());
            ftpClient.UploadFile(ftpFile, Path.GetTempFileName());
            localTempFile = new Uri(Path.GetTempFileName());
        }
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///A test for DeleteFile
        ///</summary>
        [TestMethod()]
        public void DeleteFileTest()
        {
            FtpKit target = new FtpKit(address, credential); // TODO: Initialize to an appropriate value
            target.DeleteFile(ftpFile);
        }

        /// <summary>
        ///A test for DownloadFile
        ///</summary>
        [TestMethod()]
        public void DownloadFileTest()
        {
            FtpKit target = new FtpKit(address, credential); // TODO: Initialize to an appropriate value
            bool expected = true;
            // Negative test
            bool actual = target.DownloadFile(ftpFile, localTempFile); // tmp-файл является более свежим, т.к. создан позже файла на ftp
            Assert.IsTrue(!actual && File.Exists(localTempFile.OriginalString));

            // Positive test
            FileInfo localFile = new FileInfo("downloaded.txt");
            actual = target.DownloadFile(ftpFile, localFile.FullName);
            Assert.IsTrue(actual && localFile.Exists);
        }

        /// <summary>
        ///A test for GetFileDateTime
        ///</summary>
        [TestMethod()]
        public void GetFileDateTimeTest()
        {
            FtpKit target = new FtpKit(address, credential); // TODO: Initialize to an appropriate value
            DateTime expected = DateTime.MinValue; // TODO: Initialize to an appropriate value
            DateTime actual = target.GetFileDateTime(ftpFile);
            Assert.AreNotEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetListEntrys
        ///</summary>
        [TestMethod()]
        public void GetListEntrysTest()
        {
            FtpKit target = new FtpKit(address, credential); // TODO: Initialize to an appropriate value
            List<FtpEntry> actual = target.GetListEntrys(ftpFile);
            Assert.AreEqual(1, actual.Count);
        }

        /// <summary>
        ///A test for UploadFile
        ///</summary>
        [TestMethod()]
        public void UploadFileTest()
        {
            FtpKit target = new FtpKit(address, credential); // TODO: Initialize to an appropriate value
            bool expected = true; // TODO: Initialize to an appropriate value
            bool actual = target.UploadFile(localTempFile, new Uri(address + "/test/" + Guid.NewGuid()));
            Assert.AreEqual(expected, actual);
        }
    }
}
