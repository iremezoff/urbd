using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Handlers.Strategy;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [DataContract]
    [CommandHandler(HandlerType = typeof(IMlgCollectDataHandler), StrategyBuilder = typeof(IMlgCollectStrategyBuilder))]
    public class MlgCollectCommand : ExecuteCommand
    {
        [DataMember]
        public DateTime prevDateLog;
    }
}
