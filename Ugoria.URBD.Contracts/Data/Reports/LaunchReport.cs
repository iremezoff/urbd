﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Ugoria.URBD.Contracts.Data.Reports
{
    [DataContract]
    public class LaunchReport : Report
    {
        [DataMember]
        public Guid launchGuid;

        [DataMember]
        public int pid;

        [DataMember]
        public DateTime startDate;

        [DataMember]
        public string componentName;
    }
}
