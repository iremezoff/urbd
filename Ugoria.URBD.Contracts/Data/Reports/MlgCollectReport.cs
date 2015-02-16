using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    //public enum MlgCollectReportStatus { Unknown, Interrupt, Success, Fail, Warning };

    [Serializable]
    [DataContract]
    [ReportHandler(HandlerType = typeof(IMlgCollectDataHandler), CommandType = typeof(MlgCollectCommand))]
    public class MlgCollectReport:OperationReport
    {
        [DataMember]
        public List<MlgMessage> messageList = new List<MlgMessage>();
    }
}
