using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Handlers.Strategy.Exchange.Mode;

namespace Ugoria.URBD.RemoteService.Strategy
{
    public class ExchangeContext
    {
        public ExchangeCommand Command { get; set; }
        public string BasePath { get; set; }
        public string Path1C { get; set; }
        public string User1C { get; set; }
        public string Password1C { get; set; }
        public string PrmFile { get; set; }
        public string FtpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public IEnumerable<ReportPacket> Packets { get; set; }
        public DateTime DateRelease { get; set; }
        public string MDRelease { get; set; }
        public bool HasDeletePacket { get; set; }
        public int FtpDelayTime { get; set; }
        public int AttemptDelay { get; set; }
        public int FtpAttemptCount { get; set; }
        public string FtpCenterDir { get; set; }
        public string FtpPeripheryDir { get; set; }
        public bool HaveMD { get; set; }
        public int Pid { get; set; }
        public Guid LaunchGuid { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime CompleteTime { get; set; }
        public IMode Mode { get; set; }
    }
}
