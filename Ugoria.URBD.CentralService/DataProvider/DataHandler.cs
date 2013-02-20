using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.CentralService.DataProvider;
using System.Data;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public abstract class DataHandler : IDataHandler
    {
        public abstract LaunchReport GetLaunchReport(ExecuteCommand command);
        public abstract ExecuteCommand GetPreparedCommand(ExecuteCommand command);

        public abstract ReportStatus SetReport(OperationReport report);
        /*{
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                dataProvider.SetReport(report.reportGuid, report.dateComplete, ReportStatus.Interrupt.ToString(), report.message, null, null, null);
            }
            return ReportStatus.Fail;
        }*/

        public ReportStatus SetLaunchReport(LaunchReport launchReport)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                dataProvider.SetPID1C(launchReport.reportGuid, launchReport.launchGuid, launchReport.startDate, launchReport.pid);
            }
            return ReportStatus.Information;
        }

        public virtual ExecuteCommand SetCommandReport(ExecuteCommand command)
        {
            this.SetCommandReport(command, string.Empty, 1);
            return command;
        }

        public virtual ExecuteCommand SetCommandReport(ExecuteCommand command, int userId)
        {
            this.SetCommandReport(command, string.Empty, userId);
            return command;
        }

        public virtual ExecuteCommand SetCommandReport(ExecuteCommand command, string component, int userId)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                dataProvider.SetCommandReport(userId, command.reportGuid, command.baseId, command.commandDate, component);
            }
            return command;
        }
    }
}
