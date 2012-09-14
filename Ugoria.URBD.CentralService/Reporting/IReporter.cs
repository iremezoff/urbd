using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.CentralService;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Core.Reporting
{
    interface IReporter
    {
        void SetPID1C (LaunchReport launchReport);
        void SetReport (OperationReport report);
        void SetCommandReport (Report commandReport);
        ReportInfo CheckReport (Guid launchGuid);
    }
}
