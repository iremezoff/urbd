using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Ugoria.URBD.Contracts.Services
{
    [ServiceContract]
    public interface IControlService
    {
        [OperationContract]
        void RunTask(int userId, int baseId, ModeType modeType);

        [OperationContract]
        void InterruptTask(int baseId);
    }
}
