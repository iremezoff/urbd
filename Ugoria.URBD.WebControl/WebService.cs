using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Service;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.WebControl
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    public class WebService:IWebService
    {
        public event Action<ExecuteCommand> CommandSended;
        public event Action<Report> ReportReceived;
        public void NotifyCommand(ExecuteCommand command)
        {
            if (CommandSended != null)
                CommandSended(command);
        }

        public void NotifyReport(Report report)
        {
            if (ReportReceived != null)
                ReportReceived(report);
        }
    }
}