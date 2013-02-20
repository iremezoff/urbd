using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    [DataContract]
    [KnownType(typeof(OperationReport))]
    [KnownType(typeof(LaunchReport))]
    public class Report
    {
        [DataMember]
        public Guid reportGuid;

        [DataMember]
        public int baseId;

        [DataMember]
        public string baseName = "";

        [DataMember]
        public DateTime commandDate;
    }
}
