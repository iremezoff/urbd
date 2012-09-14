using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.Contracts.Service
{
    [ServiceContract]
    public interface IRemoteService
    {
        [OperationContract(IsOneWay = true)]
        void Configure (RemoteConfiguration configuration);

        [OperationContract(IsOneWay = true)]
        void CommandExecute (Command command);

        [OperationContract(IsOneWay = true)]
        void RegisterCentralService (Uri serviceAddress);

        [OperationContract]
        RemoteProcessStatus CheckProcess (CheckCommand command);
    }
}
