using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.WebControl.Models;
using Ugoria.URBD.WebControl.Helpers;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.WebControl.SignalR
{
    public class MlgCollectHandler : BaseHandler
    {
        public override object GetPacket(OperationReport report)
        {
            IBaseRepository baseRepo = new BaseRepository(new URBD2Entities());

            IBaseReportView baseReportView = baseRepo.GetBaseById(report.baseId, "MlgCollect");
            return new
            {
                base_id = report.baseId,
                date_complete = report.dateComplete,
                message = report.message,
                status = report.status.ToString().ToLower(),
                date_last_log = baseReportView.DateMlgLog
            };
        }
    }
}