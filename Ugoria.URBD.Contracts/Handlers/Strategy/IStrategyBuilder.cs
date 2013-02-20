using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Context;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Handlers.Strategy
{
    public interface IStrategyBuilder
    {
        ICommandStrategy Build(IContext context);
        void PrepareSystem(RemoteConfiguration configuration);
        IDictionary<string, string> ValidateConfiguration(RemoteConfiguration configuration);
        LaunchReport GetLaunchReport(ICommandStrategy strategy);
        OperationReport GetOperationReport(ICommandStrategy strategy);
    }    
}
