using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.Contracts.Services
{
    [ServiceContract]
    public interface IRemoteService
    {
        [OperationContract(IsOneWay = true)]
        void ResetConfiguration ();

        [OperationContract(IsOneWay = true)]
        void CommandExecute (Command command);

        [OperationContract]
        RemoteProcessStatus CheckProcess (CheckCommand command);

        [OperationContract(IsOneWay = true)]
        void InterruptProcess(Guid commandGuid);
    }
}
