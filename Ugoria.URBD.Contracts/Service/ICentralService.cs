using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Service
{
    [ServiceContract]
    public interface ICentralService
    {
        [OperationContract(IsOneWay = true)]
        void NoticePID1C (LaunchReport launchReport, Uri address);

        [OperationContract(IsOneWay = true)]
        void NoticeReport (OperationReport report, Uri address);

        [OperationContract(IsOneWay = true)]
        void RequestConfiguration (Uri address);
    }
}
