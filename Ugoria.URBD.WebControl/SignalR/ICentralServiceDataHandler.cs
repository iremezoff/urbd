using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.WebControl.SignalR
{
    public interface ICentralServiceDataHandler
    {
        object GetPacket(ExecuteCommand command);
        object GetPacket(OperationReport report);
    }
}
