using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    [KnownType(typeof(OperationReport))]
    [KnownType(typeof(LaunchReport))]
    [DataContract]
    public class Report
    {
        [DataMember]
        public Guid reportGuid;

        [DataMember]
        public string baseName = "";

        [DataMember]
        public DateTime dateCommand;
    }
}
