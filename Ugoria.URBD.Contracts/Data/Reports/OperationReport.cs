using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    [KnownType(typeof(Report))]
    [DataContract]
    public class OperationReport : Report
    {
        [DataMember]
        public DateTime dateComplete;

        [DataMember]
        public ReportStatus status;

        [DataMember]
        public string message;

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
