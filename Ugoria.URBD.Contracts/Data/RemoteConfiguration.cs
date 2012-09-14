using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    public class RemoteConfiguration
    {
        [DataMember]
        public string ftpAddress = "10.86.1.48";

        [DataMember]
        public string ftpUsername = "";

        [DataMember]
        public string ftpPassword = "";

        [DataMember]
        public string file1CPath = @"c:\1cv7\1cv7.exe";

        [DataMember]
        public int threadsCount = 3;

        [DataMember]
        public string extFormsPath = "";

        [DataMember]
        public string cpPath = "";

        [DataMember]
        public string pcPath = "";

        [DataMember]
        public int waitTime = 10;

        [DataMember]
        public int packetExchangeAttempts = 3;

        [DataMember]
        public int packetExchangeWaitTime = 10;

        [DataMember]
        public List<Base> baseList = new List<Base>();
    }
}
