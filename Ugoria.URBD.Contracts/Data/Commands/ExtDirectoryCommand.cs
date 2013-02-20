using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Handlers.Strategy;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [Serializable]
    [DataContract]
    [CommandHandler(HandlerType = typeof(IExtDirectoriesDataHandler), StrategyBuilder = typeof(IExtDirectoryStrategyBuilder))]
    public class ExtDirectoriesCommand : ExecuteCommand
    {
    }
}
