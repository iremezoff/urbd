using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(ExecuteCommand))]
    [KnownType(typeof(CheckCommand))]
    public class Command
    {
        [DataMember]
        public Guid reportGuid;

        [DataMember]
        public int baseId;

        [DataMember]
        public string baseName;

        [DataMember]
        public DateTime commandDate;

        [DataMember]
        public DateTime configurationChangeDate;
    }
}
