using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Service;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [DataContract]
    public class Command
    {
        [DataMember]
        public Guid guid;

        [DataMember]
        public string baseName = "";

        [DataMember]
        public DateTime commandDate;

        [DataMember]
        public CommandType commandType;

        [DataMember]
        public ModeType modeType;

        [DataMember]
        public bool withMD;
    }
}
