using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Shared.Reporting
{
    class ReportInfo
    {
        public string ServiceAddress { get; set; }
        public string BaseName { get; set; }
        public DateTime CommandDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime CompleteDate { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DateTime ConfigurationChangeDate { get; set; }
        public int Pid { get; set; }
        public Guid ReportGuid { get; set; }
        public Guid LaunchGuid { get; set; }
    }
}
