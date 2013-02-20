using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data
{
    [DataContract]
    public class ExtDirectoriesFile
    {
        [DataMember]
        public string fileName = "";

        [DataMember]
        public long fileSize = 0;

        [DataMember]
        public DateTime createdDate;
    }
}
