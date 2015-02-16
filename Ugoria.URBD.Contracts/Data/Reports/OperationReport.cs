using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    public enum ReportStatus { Unknown, Interrupt, Success, Fail, Warning, Critical };

    [Serializable]
    [DataContract]
    [KnownType(typeof(ExchangeReport))]
    [KnownType(typeof(ExtDirectoriesReport))]
    [KnownType(typeof(MlgCollectReport))]
    public class OperationReport : Report
    {
        [DataMember]
        public ReportStatus status;

        [DataMember]
        public DateTime dateComplete;

        [DataMember]
        public string message;
    }
}
