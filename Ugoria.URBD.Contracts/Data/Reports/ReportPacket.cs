﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Services;

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
        public long fileSize;

        [DataMember]
        public PacketType type;
    }
}
