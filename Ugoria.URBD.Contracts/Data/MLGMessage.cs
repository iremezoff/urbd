using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data
{
    [Serializable]
    [DataContract]
    public class MlgMessage
    {
        [DataMember]
        public DateTime eventDate;

        [DataMember]
        public string eventType;

        [DataMember]
        public string information;

        [DataMember]
        public string account;

        [DataMember]
        public string mode1c;

        [DataMember]
        public string objectTypeCode = "";

        [DataMember]
        public string objectTypeNumber = "";

        [DataMember]
        public string baseCode = "";

        [DataMember]
        public string objectIdentifier = "";

        [DataMember]
        public string additional = "";

        [DataMember]
        public string eventGroup = "";
    }
}
