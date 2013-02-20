using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Handlers.Strategy.Exchange.Mode;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Context;

namespace Ugoria.URBD.Contracts.Handlers.Strategy
{
    public interface IExchangeStrategyBuilder : IStrategyBuilder
    {
        IMode CreateMode(IContext exchangeContext);
    }
}
