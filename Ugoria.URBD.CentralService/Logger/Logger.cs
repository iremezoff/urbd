using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.CentralService.DataProvider;

namespace Ugoria.URBD.CentralService.Logging
{
    public class Logger : ILogger
    {
        private bool isFailEnabled = true;
        private bool isInformationEnabled = true;
        private bool isWarningEnabled = true;

        public bool IsFailEnabled
        {
            get { return isFailEnabled; }
            set { isFailEnabled = value; }
        }

        public bool IsInformationEnabled
        {
            get { return isInformationEnabled; }
            set { isInformationEnabled = value; }
        }

        public bool IsWarningEnabled
        {
            get { return isWarningEnabled; }
            set { isWarningEnabled = value; }
        }

        public void Fail(Uri uri, string message)
        {
            Fail(Uri2AddressString(uri), message);
        }

        public void Fail(string address, string message)
        {
            if (!isFailEnabled)
                return;
            SetLog(address, 'F', message);
        }

        public void Information(Uri uri, string message)
        {
            Information(Uri2AddressString(uri), message);
        }

        public void Information(string address, string message)
        {
            if (!isInformationEnabled)
                return;
            SetLog(address, 'I', message);
        }

        public void Warning(Uri uri, string message)
        {
            Warning(Uri2AddressString(uri), message);
        }

        public void Warning(string address, string message)
        {
            if (!isWarningEnabled)
                return;
            SetLog(address, 'W', message);
        }

        private void SetLog(string address, char type, string message)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                dataProvider.SetLog(address, DateTime.Now, type, message);
            }
        }

        public Logger()
        {
        }

        private static string Uri2AddressString(Uri uri)
        {
            return String.Format("{0}:{1}", uri.Host, uri.Port);
        }
    }
}
