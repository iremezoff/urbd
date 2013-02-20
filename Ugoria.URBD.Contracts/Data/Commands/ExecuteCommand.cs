using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(ExchangeCommand))]
    [KnownType(typeof(ExtDirectoriesCommand))]
    public class ExecuteCommand : Command
    {
        [DataMember]
        public List<int> pools;
    }
}
