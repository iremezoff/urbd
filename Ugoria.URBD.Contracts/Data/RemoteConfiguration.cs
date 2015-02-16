using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Shared.Configuration;
using System.Security;
using System.Collections;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    [KnownType(typeof(List<Hashtable>))]
    [KnownType(typeof(PacketType))]
    [KnownType(typeof(Dictionary<string, string>))]
    public class RemoteConfiguration
    {
        [DataMember]
        public Hashtable configuration;

        [DataMember]
        public Dictionary<int, Hashtable> bases;
    }
}
