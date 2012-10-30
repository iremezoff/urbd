using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using System.Data;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Shared.Reporting;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.CentralService.DataProvider;

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

            dataProvider.SetReport(report.reportGuid, report.dateComplete, report.status.ToString(), report.message, report.mdRelease, report.dateRelease, transactionName);
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
                    dataProvider.SetReportPacket(report.reportGuid, packet.filename, packet.datePacket, (char)packet.type, packet.fileSize, packet.fileHash, transactionName);
                }
            }

            dataProvider.Commit(transactionName); // завершение изменений
        }

        public void SetCommandReport(Report commandReport, int userId)
        {
            dataProvider.SetCommandReport(userId, commandReport.reportGuid, commandReport.baseId, commandReport.dateCommand);
        }

        public ReportInfo GetLastCommand(int baseId)
        {
            DataRow lastCommand = dataProvider.GetLastCommand(baseId);
            if (lastCommand == null)
                return null;
            return new ReportInfo
            {
                ServiceAddress = (string)lastCommand["address"],
                BaseName = lastCommand["base_name"].ToString(),
                CommandDate = (DateTime)lastCommand["date_command"],
                StartDate = lastCommand["date_start"] != DBNull.Value ? (DateTime)lastCommand["date_start"] : DateTime.MinValue,
                CompleteDate = lastCommand["date_complete"] != DBNull.Value ? (DateTime)lastCommand["date_complete"] : DateTime.MinValue,
                ReleaseDate = lastCommand["date_release"] != DBNull.Value ? (DateTime)lastCommand["date_release"] : DateTime.MinValue,
                ConfigurationChangeDate = lastCommand["date_change"] != DBNull.Value ? (DateTime)lastCommand["date_change"] : DateTime.MinValue,
                Pid = lastCommand["pid"] != DBNull.Value ? (int)lastCommand["pid"] : 0,
                ReportGuid = (Guid)lastCommand["report_guid"],
                LaunchGuid = lastCommand["launch_guid"] != DBNull.Value ? (Guid)lastCommand["launch_guid"] : Guid.Empty
            };
        }

        public Reporter(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }
    }
}
