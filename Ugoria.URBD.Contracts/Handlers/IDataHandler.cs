using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Handlers
{
    public enum ReportStatus { Information, Fail, Warning, Interrupt, Critical}

    public interface IDataHandler
    {
        ReportStatus SetReport(OperationReport report);
        ExecuteCommand GetPreparedCommand(ExecuteCommand command);
        LaunchReport GetLaunchReport(ExecuteCommand command);
        ReportStatus SetLaunchReport(LaunchReport report);
        ExecuteCommand SetCommandReport(ExecuteCommand command);
    }
}
