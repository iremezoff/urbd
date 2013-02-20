using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.Contracts.Services
{
    [ServiceContract]
    public interface IControlService
    {
        [OperationContract]
        void RunTask(int userId, ExecuteCommand command);

        [OperationContract]
        void InterruptTask(ExecuteCommand command);

        [OperationContract]
        void ReconfigureBaseOfService(int baseId);

        [OperationContract]
        void ReconfigureRemoteService(int serviceId);

        [OperationContract]
        void ReconfigureCentralService();
    }
}
