using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.RemoteService.Strategy
{
    public class MlgCollectContext
    {
        public MlgCollectCommand Command { get; set; }
        public string BasePath { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Stack<MlgMessage> Messages { get; set; }
        public DateTime PrevDateLog { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompleteTime { get; set; }
        public Guid LaunchGuid { get; set; }
    }
}
