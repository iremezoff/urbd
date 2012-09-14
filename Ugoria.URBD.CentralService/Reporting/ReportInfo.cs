using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Core.Reporting
{
    class ReportInfo
    {
        public string serviceAddress;
        public string baseName;
        public DateTime commandDate;
        public DateTime startDate;
        public DateTime completeDate;
        public int pid;
        public Guid reportGuid;
        public Guid launchGuid;
    }
}
