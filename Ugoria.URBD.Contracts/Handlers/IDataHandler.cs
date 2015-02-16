using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Handlers
{
    //public enum ReportStatusType { Information, Fail, Warning, Interrupt, Critical}

    public interface IDataHandler
    {
        void SetReport(OperationReport report);
        ExecuteCommand GetPreparedCommand(ExecuteCommand command);
        LaunchReport GetLaunchReport(ExecuteCommand command);
        void SetLaunchReport(LaunchReport report);
        void SetCommandReport(ExecuteCommand command);
        Type ReportType { get; }
    }
}
