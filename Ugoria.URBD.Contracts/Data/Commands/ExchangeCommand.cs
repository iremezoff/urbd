using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [DataContract]
    public class ExchangeCommand : Command
    {
        [DataMember]
        public ModeType modeType;
    }
}
