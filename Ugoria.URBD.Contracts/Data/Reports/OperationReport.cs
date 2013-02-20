using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    [DataContract]
    [KnownType(typeof(ExchangeReport))]
    [KnownType(typeof(ExtDirectoriesReport))]
    public class OperationReport : Report
    {
        [DataMember]
        public DateTime dateComplete;

        [DataMember]
        public string message;
    }
}
