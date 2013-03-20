using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    //public enum ExtDirectoriesReportStatus { Unknown, Interrupt, Success, Fail, Warning };

    [DataContract]
    [ReportHandler(CommandType = typeof(ExtDirectoriesCommand))]
    public class ExtDirectoriesReport : OperationReport
    {
        [DataMember]
        public List<ExtDirectoriesFile> files;
    }
}
