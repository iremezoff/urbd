using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [DataContract]
    public class Command
    {
        [DataMember]
        public Guid guid;

        [DataMember]
        public int baseId;

        [DataMember]
        public DateTime commandDate;

        [DataMember]
        public CommandType commandType;

        [DataMember]
        public ModeType modeType;

        [DataMember]
        public DateTime releaseUpdate;
    }
}
