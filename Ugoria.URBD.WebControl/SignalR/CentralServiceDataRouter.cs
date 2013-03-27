using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Data.Commands;
using Microsoft.AspNet.SignalR;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Service;

namespace Ugoria.URBD.WebControl.SignalR
{
    public class CentralServiceDataRouter
    {
        private Dictionary<Type, ICentralServiceDataHandler> handlers = new Dictionary<Type, ICentralServiceDataHandler>();
        private Dictionary<Type, string> components = new Dictionary<Type, string>();
        private IHubContext hubContext;

        public void Add(Type commandContractType, Type reportContractType, ICentralServiceDataHandler handler)
        {
            int index = 0;
            if (commandContractType.IsSubclassOf(typeof(ExecuteCommand)) && (index = commandContractType.Name.IndexOf("Command")) > 0)
                components.Add(commandContractType, commandContractType.Name.Substring(0, index));
            if (reportContractType.IsSubclassOf(typeof(OperationReport)) && (index = reportContractType.Name.IndexOf("Report")) > 0)
                components.Add(reportContractType, reportContractType.Name.Substring(0, index));

            handlers.Add(commandContractType, handler);
            handlers.Add(reportContractType, handler);
        }

        public void Routing(ExecuteCommand command)
        {
            string componentName = components[command.GetType()];
            ICentralServiceDataHandler handler = handlers[command.GetType()];
            hubContext.Clients.Group(string.Format("{0}.{1}", componentName, command.baseId)).sendCommand(handler.GetPacket(command));
            hubContext.Clients.Group(string.Format("{0}.All", componentName)).sendCommand(handler.GetPacket(command));
        }

        public void Routing(Report report)
        {
            if (report is LaunchReport)
            {
                string componentName = ((LaunchReport)report).componentName;
                object packet = new
                {
                    base_id = report.baseId,
                    type = "launch",
                    date_complete = ((LaunchReport)report).startDate,
                    message = string.Format("Идет процесс с {0:dd.MM.yyyy HH:mm:ss}. PID: {1} ({2:dd.MM.yyyy HH:mm:ss})", report.commandDate, ((LaunchReport)report).pid, ((LaunchReport)report).startDate),
                    status = "busy",
                };
                hubContext.Clients.Group(string.Format("{0}.{1}", componentName, report.baseId)).sendReport(packet);
                hubContext.Clients.Group(string.Format("{0}.All", componentName)).sendReport(packet);
            }
            else
            {
                string componentName = components[report.GetType()];
                ICentralServiceDataHandler handler = handlers[report.GetType()];
                object packet = handler.GetPacket((OperationReport)report);
                hubContext.Clients.Group(string.Format("{0}.{1}", componentName, report.baseId)).sendReport(packet);
                hubContext.Clients.Group(string.Format("{0}.All", componentName)).sendReport(packet);
            }
        }

        public CentralServiceDataRouter(IHubContext hubContext, WebService service)
        {
            this.hubContext = hubContext;
            service.CommandSended += Routing;
            service.ReportReceived += Routing;
        }
    }
}