using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Ugoria.URBD.CentralService.DataProvider;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Services;
using System.Collections;
using System.Transactions;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public class ExchangeDataHandler : DataHandler, IExchangeDataHandler
    {
        public ExchangeDataHandler()
            : base("Exchange") { }

        public override ExecuteCommand GetPreparedCommand(ExecuteCommand command)
        {
            return GetPreparedExchangeCommand((ExchangeCommand)command);
        }

        public ExchangeCommand GetPreparedExchangeCommand(ExchangeCommand command)
        {
            ExchangeCommand preparedCommand = new ExchangeCommand()
            {
                baseId = command.baseId,
                baseName = command.baseName,
                commandDate = command.commandDate,
                modeType = command.modeType
            };
            preparedCommand = (ExchangeCommand)base.GetPreparedCommand(preparedCommand);

            DataRow dataRow = cache.Tables[0].Rows[0];
            preparedCommand.releaseUpdate = dataRow["date_release"] != DBNull.Value ? (DateTime)dataRow["date_release"] : DateTime.MinValue;
            preparedCommand.isReleaseUpdated = (bool)dataRow["is_updated"];

            return preparedCommand;
        }

        public void SetReport(ExchangeReport report)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                base.SetReport(report);
                if (!string.IsNullOrEmpty(report.mdRelease))
                {
                    dataProvider.SetReportParam(report.reportGuid, new Hashtable() { { "md_release", report.mdRelease }, { "date_release", report.dateRelease } });
                }
                // обрабокта сообщений лога работы 1С на стороне удаленного сервиса
                foreach (MlgMessage message in report.messageList)
                {
                    dataProvider.SetReportLog(report.reportGuid, message.eventDate, message.eventType, message.account, message.mode1c, message.information);
                }
                // информация о пакетах
                if (report.packetList.Count > 0)
                {
                    foreach (ReportPacket packet in report.packetList)
                    {
                        dataProvider.SetReportPacket(report.reportGuid, packet.filename, packet.datePacket, (char)packet.type, packet.fileSize);
                    }
                }
                scope.Complete();
            }
        }

        public override void SetReport(OperationReport report)
        {
            SetReport((ExchangeReport)report);
        }

        public override DataHandler Clone()
        {
            return new ExchangeDataHandler();
        }
    }
}
