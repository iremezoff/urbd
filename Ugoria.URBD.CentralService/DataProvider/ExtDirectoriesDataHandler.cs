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
using System.Transactions;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public class ExtDirectoriesDataHandler : DataHandler, IExtDirectoriesDataHandler
    {
        public ExtDirectoriesDataHandler()
            : base("ExtDirectories") { }

        public ExtDirectoriesCommand GetPreparedExtDirectoriesCommand(ExtDirectoriesCommand command)
        {
                ExtDirectoriesCommand preparedCommand = new ExtDirectoriesCommand()
                {
                    baseId = command.baseId,
                    baseName = command.baseName,
                    commandDate = command.commandDate
                };
                preparedCommand = (ExtDirectoriesCommand)base.GetPreparedCommand(preparedCommand);

                return preparedCommand;            
        }

        public virtual void SetReport(ExtDirectoriesReport report)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted }))
            {
                base.SetReport(report);
                // обрабокта сообщений лога работы 1С на стороне удаленного сервиса
                foreach (ExtDirectoriesFile file in report.files)
                {
                    dataProvider.SetReportFile(report.reportGuid, file.fileName, file.createdDate, file.fileSize);
                }
                scope.Complete();
            }
        }

        public override void SetReport(OperationReport report)
        {
            SetReport((ExtDirectoriesReport)report);
        }

        public override DataHandler Clone()
        {
            return new ExtDirectoriesDataHandler();
        }
    }
}
