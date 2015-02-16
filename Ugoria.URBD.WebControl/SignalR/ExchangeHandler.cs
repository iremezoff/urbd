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
    public class ExchangeHandler:BaseHandler
    {
        public override object GetPacket(OperationReport report)
        {
            IBaseRepository baseRepo = new BaseRepository(new URBD2Entities());

            IBaseReportView baseReportView = baseRepo.GetBaseById(report.baseId, "Exchange");
            return new
            {
                base_id = report.baseId,
                date_complete = report.dateComplete,
                message = report.message,
                status = report.status.ToString().ToLower(),
                md_release = baseReportView.MDRelease,
                load_packets = string.Join("<br/>", baseReportView.Packets.Where(p => p.Packet.Type[0] == (char)PacketType.Load).Select(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Packet.FileName, c.Size / 1024f, c.CreatedDate))),
                unload_packets = string.Join("<br/>", baseReportView.Packets.Where(p => p.Packet.Type[0] == (char)PacketType.Unload).Select(c => string.Format("<b>{0}</b> ({1:0.00} Kb) - {2:dd.MM.yyyy HH:mm:ss}", c.Packet.FileName, c.Size / 1024f, c.CreatedDate)))
            };
        }
    }
}