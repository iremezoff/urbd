using System;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.RemoteService
{
    class ReportBuilder : IReportBuilder
    {
        private string baseName;
        private DateTime commandDate = DateTime.MinValue;
        private Guid reportGuid;
        private IReportBuilder concreteBuilder;

        public Guid ReportGuid
        {
            get { return reportGuid; }
            set { reportGuid = value; }
        }

        public DateTime CommandDate
        {
            get { return commandDate; }
            set { commandDate = value; }
        }

        public string BaseName
        {
            get { return baseName; }
            set { baseName = value; }
        }               

        public IReportBuilder ConcreteBuilder
        {
            get { return concreteBuilder; }
            set { concreteBuilder = value; }
        }

        private ReportBuilder() { }

        public static ReportBuilder Create()
        {
            return new ReportBuilder();
        }

        public Report Build()
        {
            Report report = concreteBuilder != null ? concreteBuilder.Build() : new Report();
            report.baseName = baseName ?? Guid.NewGuid().ToString();
            report.dateCommand = commandDate;
            report.reportGuid = reportGuid;

            return report;
        }
    }
}
