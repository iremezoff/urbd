using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.CentralService;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.Shared.Reporting
{
    interface IReporter
    {
        void SetPID1C (LaunchReport launchReport);
        void SetReport (OperationReport report);
        void SetCommandReport (Report commandReport, int userId);
        ReportInfo GetLastCommand(int baseId);
    }
}
