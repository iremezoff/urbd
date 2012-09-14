using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Commands
{
    [DataContract]
    public class CheckCommand
    {
        [DataMember]
        public Guid reportGuid;

        [DataMember]
        public Guid launchGuid;
    }
}
