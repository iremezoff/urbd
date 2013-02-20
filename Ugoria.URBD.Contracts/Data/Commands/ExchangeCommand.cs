using System;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [Serializable]
    [DataContract]
    [CommandHandler(HandlerType = typeof(IExchangeDataHandler), StrategyBuilder = typeof(IExchangeStrategyBuilder))]
    public class ExchangeCommand : ExecuteCommand
    {
        [DataMember]
        public ModeType modeType;

        [DataMember]
        public DateTime releaseUpdate;

        [DataMember]
        public bool isReleaseUpdated = false;
    }
}
