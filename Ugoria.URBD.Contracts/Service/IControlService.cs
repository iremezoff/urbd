using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.Contracts.Services
{
    [ServiceContract]
    public interface IControlService
    {
        [OperationContract]
        void RunTask(int userId, int baseId, ModeType modeType);

        [OperationContract]
        void InterruptTask(int baseId);

        [OperationContract]
        IDictionary<string, string> ValidateConfiguration(Uri remoteUri, RemoteConfiguration configuration);
    }
}
