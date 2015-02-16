using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    public class Packet
    {
        [DataMember]
        public string filename = "";

        [DataMember]
        public PacketType type = PacketType.Load;
    }
}
