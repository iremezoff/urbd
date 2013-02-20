using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    public enum ExchangeReportStatus { Unknown, Interrupt, Success, Fail, Warning, Critical };

    [DataContract]
    [ReportHandler(HandlerType = typeof(IExchangeDataHandler), CommandType = typeof(ExchangeCommand))]
    public class ExchangeReport : OperationReport
    {
        [DataMember]
        public ExchangeReportStatus status;

        [DataMember]
        public List<MLGMessage> messageList = new List<MLGMessage>();

        [DataMember]
        public List<ReportPacket> packetList = new List<ReportPacket>();

        [DataMember]
        public string mdRelease = "";

        [DataMember]
        public DateTime dateRelease;
    }
}
