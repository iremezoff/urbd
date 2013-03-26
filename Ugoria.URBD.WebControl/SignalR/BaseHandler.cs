using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.WebControl.SignalR
{
    public abstract class BaseHandler : ICentralServiceDataHandler
    {
        public object GetPacket(ExecuteCommand command)
        {
            return new
            {
                base_id = command.baseId,
                type = command.GetType().Name,
                message = string.Format("Идет процесс с {0:dd.MM.yyyy HH:mm:ss}", command.commandDate),
                status = "busy"
            };
        }

        public abstract object GetPacket(OperationReport report);
    }
}