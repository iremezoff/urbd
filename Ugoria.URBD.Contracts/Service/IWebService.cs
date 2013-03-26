using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Service
{
    [ServiceContract]
    public interface IWebService
    {
        [OperationContract(IsOneWay = true)]
        void NotifyCommand(ExecuteCommand command);

        [OperationContract(IsOneWay = true)]
        void NotifyReport(Report report);
    }
}
