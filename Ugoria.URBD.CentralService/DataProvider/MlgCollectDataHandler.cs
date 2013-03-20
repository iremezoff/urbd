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
    public class MlgCollectDataHandler : DataHandler, IMlgCollectDataHandler
    {
        public MlgCollectDataHandler()
            : base("MlgCollect") { }

        public override ExecuteCommand GetPreparedCommand(ExecuteCommand command)
        {
            return GetPreparedMlgCollectCommand((MlgCollectCommand)command);
        }

        public MlgCollectCommand GetPreparedMlgCollectCommand(MlgCollectCommand command)
        {
            MlgCollectCommand preparedCommand = new MlgCollectCommand()
            {
                baseId = command.baseId,
                baseName = command.baseName,
                commandDate = command.commandDate
            };
            preparedCommand = (MlgCollectCommand)base.GetPreparedCommand(preparedCommand);

            DataRow dataRow = cache.Tables[0].Rows[0];
            preparedCommand.prevDateLog = dataRow["mlg_date_log"] != DBNull.Value ? (DateTime)dataRow["mlg_date_log"] : DateTime.MinValue;

            return preparedCommand;
        }

        public virtual void SetReport(MlgCollectReport report)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                base.SetReport(report);
                if (report.messageList.Count > 0)
                    dataProvider.SetReportParam(report.reportGuid, new Hashtable() { { "mlg_date_log", report.messageList.Last().eventDate } });

                // обрабокта сообщений лога работы 1С на стороне удаленного сервиса
                foreach (MlgMessage message in report.messageList)
                {
                    dataProvider.SetReportLog(report.reportGuid, message.eventDate, message.eventType, message.account,message.mode1c, message.information, message.objectTypeCode, message.objectTypeNumber, message.baseCode, message.objectIdentifier, message.additional);
                }
                scope.Complete();
            }
        }

        public override void SetReport(OperationReport report)
        {
            SetReport((MlgCollectReport)report);
        }

        public override DataHandler Clone()
        {
            return new MlgCollectDataHandler();
        }
    }
}
