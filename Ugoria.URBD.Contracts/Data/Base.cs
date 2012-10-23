using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    public class Base
    {
        [DataMember]
        public int baseId;

        [DataMember]
        public string baseName="";

        [DataMember]
        public string basePath = "";

        [DataMember]
        public string username = "";

        [DataMember]
        public string password = "";

        [DataMember]
        public List<Packet> packetList = new List<Packet>();
    }
}
