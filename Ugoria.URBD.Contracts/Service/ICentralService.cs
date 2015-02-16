using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.Contracts.Services
{
    [ServiceContract]
    public interface ICentralService
    {
        [OperationContract(IsOneWay = true)]
        void NoticePID1C (LaunchReport launchReport, Uri address);

        [OperationContract(IsOneWay = true)]
        void NoticeReport (OperationReport report, Uri address);

        [OperationContract]
        RemoteConfiguration RequestConfiguration (Uri address);
    }
}
