using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;

namespace Ugoria.URBD.RemoteService
{
    interface ICommandStrategy
    {
        Guid LaunchGuid {get;}
        void Launch(ReportAsyncCallback reportAsyncsCallback);
    }
}
