using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Service;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    public class Packet
    {
        [DataMember]
        public string filePath = "";

        [DataMember]
        public PacketType packetType = PacketType.Load;
    }
}
