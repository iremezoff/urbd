using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [DataContract]
    [KnownType(typeof(ExchangeCommand))]
    [KnownType(typeof(ExtFormsCommand))]
    [KnownType(typeof(CheckCommand))]
    public class Command
    {
        [DataMember]
        public Guid reportGuid;

        [DataMember]
        public int baseId;

        [DataMember]
        public DateTime commandDate;

        [DataMember]
        public DateTime configurationChangeDate;

        [DataMember]
        public DateTime releaseUpdate;
    }
}
