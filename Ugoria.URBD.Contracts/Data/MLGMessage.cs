using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    public class MLGMessage
    {
        [DataMember]
        public DateTime eventDate;

        [DataMember]
        public string eventType;

        [DataMember]
        public string information;
    }
}
