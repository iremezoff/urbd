using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.RemoteService
{
    class LaunchReportBuilder : IReportBuilder
    {
        private DateTime startDate;

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        private int pid;

        public int Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        private Guid guid;

        public Guid Guid
        {
            get { return guid; }
            set { guid = value; }
        }

        private LaunchReportBuilder() { }

        public static LaunchReportBuilder Create()
        {
            return new LaunchReportBuilder();
        }

        public Report Build()
        {
            return new LaunchReport
            {
                pid = pid,
                startDate = startDate,
                launchGuid = guid
            };
        }
    }
}
