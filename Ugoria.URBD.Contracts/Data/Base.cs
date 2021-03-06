﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Shared.Configuration;
using System.Collections;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    [KnownType(typeof(List<Packet>))]
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
        public Hashtable extendedConfiguration;
    }
}
