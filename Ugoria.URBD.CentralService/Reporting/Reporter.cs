using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using System.Data;
using Ugoria.URBD.Core;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Core.Reporting;

namespace Ugoria.URBD.CentralService
{ 
    class Reporter : IReporter
    {
        private IDataProvider dataProvider = null;

        public void SetPID1C(LaunchReport launchReport)
        {
            dataProvider.SetPID1C(launchReport.reportGuid, launchReport.launchGuid, launchReport.startDate, launchReport.pid);
        }

        public void SetReport(OperationReport report)
        {
            string transactionName = Guid.NewGuid().ToString().Replace("-", "");

            dataProvider.BeginTransaction(transactionName); // начало транзакции

            dataProvider.SetReport(report.reportGuid, report.dateComplete, report.status.ToString(), report.message, transactionName);
            // обрабокта сообщений лога работы 1С на стороне удаленного сервиса
            foreach (MLGMessage message in report.messageList)
            {
                dataProvider.SetReportLog(report.reportGuid, message.eventDate, message.eventType, message.information, transactionName);
            }

            // информация о пакетах
            if (report.packetList.Count > 0)
            {
                foreach (ReportPacket packet in report.packetList)
                {
                    dataProvider.SetReportPacket(report.reportGuid, packet.filename, packet.datePacket, packet.fileSize, packet.fileHash, transactionName);
                }
            }

            dataProvider.Commit(transactionName); // завершение изменений
        }

        public void SetCommandReport(Report commandReport)
        {
            dataProvider.SetCommandReport(commandReport.reportGuid, commandReport.baseName, commandReport.dateCommand);
        }

        public ReportInfo CheckReport(Guid reportGuid)
        {
            DataTable reportData = dataProvider.GetReport(reportGuid);

            if (reportData.Rows.Count == 0)
                return null;
            DataRow dataRow = reportData.Rows[0];
            return new ReportInfo
             {
                 serviceAddress = (string)dataRow["address"],
                 baseName = (string)dataRow["base_name"],
                 startDate = dataRow["date_start"] != DBNull.Value ? (DateTime)dataRow["date_start"] : DateTime.MinValue,
                 commandDate = dataRow["date_command"] != DBNull.Value ? (DateTime)dataRow["date_command"] : DateTime.MinValue,
                 completeDate = dataRow["date_complete"] != DBNull.Value ? (DateTime)dataRow["date_complete"] : DateTime.MinValue,
                 pid = dataRow["pid"] != DBNull.Value ? (int)dataRow["pid"] : 0,
                 reportGuid = (Guid)dataRow["report_guid"],
                 launchGuid = dataRow["launch_guid"] != DBNull.Value ? (Guid)dataRow["launch_guid"] : Guid.Empty
             };
        }

        public Reporter(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }
    }
}
