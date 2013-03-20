using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.Contracts.Handlers
{
    public interface IMlgCollectDataHandler:IDataHandler
    {
        MlgCollectCommand GetPreparedMlgCollectCommand(MlgCollectCommand command);
        void SetReport(MlgCollectReport report);
    }
}
