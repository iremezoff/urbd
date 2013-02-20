using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Handlers
{
    public interface IExchangeDataHandler : IDataHandler
    {
        ExchangeCommand GetPreparedExchangeCommand(ExchangeCommand command);
        ReportStatus SetReport(ExchangeReport report);
    }
}
