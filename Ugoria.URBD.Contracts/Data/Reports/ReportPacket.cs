using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    [DataContract]
    public class ReportPacket
    {
        [DataMember]
        public string filename;

        [DataMember]
        public DateTime datePacket;

        [DataMember]
        public string fileHash;

        [DataMember]
        public long fileSize;
    }
}
