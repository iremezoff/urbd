using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;
using System.Security;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.RemoteService.Strategy
{
    public class ExtDirectoriesContext
    {
        public ExtDirectoriesCommand Command { get; set; }
        public string BasePath { get; set; }
        public string FtpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int FtpDelayTime { get; set; }
        public int FtpAttemptCount { get; set; }
        public Guid LaunchGuid { get; set; }
        public DateTime StartTime { get; set; }
        public Dictionary<string, string> Directories { get; set; }
        public List<ExtDirectoriesFile> Files { get; set; }
    }
}
