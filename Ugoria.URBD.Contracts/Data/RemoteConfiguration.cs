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
        public string ftpAddress = "";

        [DataMember]
        public string ftpUsername = "";

        [DataMember]
        public string ftpPassword = "";

        [DataMember]
        public string file1CPath = "";

        [DataMember]
        public int threadsCount = 3;

        [DataMember]
        public string extFormsPath = "";

        [DataMember]
        public string ftpPC = "";

        [DataMember]
        public string ftpCP = "";

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
