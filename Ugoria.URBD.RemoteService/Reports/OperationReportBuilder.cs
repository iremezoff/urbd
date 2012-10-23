using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using System.IO;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.RemoteService
{
    class OperationReportBuilder:IReportBuilder
    {
        private ReportStatus status;

        public ReportStatus Status
        {
            get { return status; }
            set { status = value; }
        }
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        private DateTime completeDate;

        public DateTime CompleteDate
        {
            get { return completeDate; }
            set { completeDate = value; }
        }

        private List<ReportPacket> packetList = new List<ReportPacket>();

        public List<ReportPacket> PacketList
        {
            get { return packetList; }
        }

        private string mdRelease = "";

        public string MDRelease
        {
            get { return mdRelease; }
            set { mdRelease = value; }
        }

        private DateTime releaseDate;

        public DateTime ReleaseDate
        {
            get { return releaseDate; }
            set { releaseDate = value; }
        }

        private OperationReportBuilder()
        {
        }

        /*public OperationReportBuilder WithPacket(string filepath)
        {
            FileInfo fileInfo = new FileInfo(filepath);
            this.packetList.Add(new ReportPacket
            {
                datePacket = fileInfo.CreationTime,
                fileHash = ServiceUtil.GetFileMd5Hash(fileInfo.FullName),
                filename = fileInfo.Name,
                fileSize = fileInfo.Length
            });
            return this;
        }

        public OperationReportBuilder WithMlgMesasge(MLGMessage mlgMessage)
        {
            this.messageList.Add(mlgMessage);

            return this;
        }*/

        public static OperationReportBuilder Create()
        {
            return new OperationReportBuilder();
        }

        public Report Build()
        {
            OperationReport report = new OperationReport
            {
                message = message,
                status = status,
                dateComplete = completeDate,
                mdRelease = mdRelease,
                dateRelease = releaseDate
            };
            report.packetList.AddRange(packetList);
            return report;
        }
    }

    class ReportPacketCollection
    {
        //private List[]
    }
}
