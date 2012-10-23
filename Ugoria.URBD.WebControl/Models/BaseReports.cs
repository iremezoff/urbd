using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.IO;
using System.Xml.Linq;

namespace Ugoria.URBD.WebControl.Models
{
    /*public interface IBase
    {
        public int BaseId { get; set; }
        public int GroupId { get; set; }
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Path { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SqlDatabase { get; set; }
    }

    public interface IService
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Path1C { get; set; }
    }

    public interface IGroup
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
    }

    public interface IReport
    {
        public int ReportId { get; set; }
        public int BaseId { get; set; }
        public int StatusId { get; set; }
        public DateTime? DateCommand { get; set; }
        public DateTime? DateComplete { get; set; }
        public string Mesasge { get; set; }
    }

    public interface IReportPacket
    {
        public int ReportPacketId { get; set; }
        public int ReportId { get; set; }
        public long? Size { get; set; }
        public DateTime? DatePacket { get; set; }
        public string FileHash { get; set; }
    }

    public interface IPacket
    {
        public int PacketId { get; set; }
        public int BaseId { get; set; }
        public char Type { get; set; }
        public string FileName { get; set; }
    }*/
    /*
    public partial class Group : IGroup
    {
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int GroupId
        {
            get { return group_id; }
            set { group_id = value; }
        }
    }

    public partial class Base : IBase
    {
        public string Name
        {
            get { return base_name; }
            set { base_name = value; }
        }

        public string Path
        {
            get { return C1c_database; }
            set { C1c_database = value; }
        }

        public string Username
        {
            get { return Username; }
            set { Username = value; }
        }

        public string Password
        {
            get { return C1c_password; }
            set { C1c_password = value; }
        }

        public string SqlDatabase
        {
            get { return sql_database; }
            set { sql_database = value; }
        }

        public int ServiceId
        {
            get { return service_id; }
            set { service_id = value; }
        }

        public int BaseId
        {
            get { return base_id; }
            set { base_id = value; }
        }


        public int GroupId
        {
            get { return group_id; }
            set { group_id = value; }
        }
    }

    public partial class Service : IService
    {
        public int ServiceId
        {
            get { return service_id; }
            set { service_id = value; }
        }

        public string Name
        {
            get { return service_name; }
            set { service_name = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public string Path1C
        {
            get { return C1c_path; }
            set { C1c_path = value; }
        }
    }

    public partial class Report : IReport
    {
        public int ReportId
        {
            get { return report_id; }
            set { report_id = value; }
        }

        public int BaseId
        {
            get { return base_id; }
            set { base_id = value; }
        }

        public int StatusId
        {
            get { return status_id; }
            set { status_id = value; }
        }

        public DateTime? DateCommand
        {
            get { return date_command; }
            set { date_command = value; }
        }

        public DateTime? DateComplete
        {
            get { return date_complete; }
            set { date_complete = value; }
        }


        public string Mesasge
        {
            get { return message; }
            set { message = value; }
        }
    }

    public partial class ReportPacket : IReportPacket
    {
        public int ReportPacketId
        {
            get { return rp_id; }
            set { rp_id = value; }
        }

        public int ReportId
        {
            get { return report_id; }
            set { report_id = value; }
        }

        public long? Size
        {
            get { return filesize; }
            set { filesize = value; }
        }

        public DateTime? DatePacket
        {
            get { return date_packet; }
            set { date_packet = value; }
        }

        public string FileHash
        {
            get { return filehash; }
            set { filehash = value; }
        }
    }

    public partial class Packet : IPacket
    {

        public int PacketId
        {
            get { return packet_id; }
            set { packet_id = value; }
        }

        public int BaseId
        {
            get { return base_id; }
            set { base_id = value; }
        }

        public char Type
        {
            get { return type[0]; }
            set { type = value.ToString(); }
        }

        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }
    }*/

    public interface IReportPacketView
    {
        string Filename { get; }
        DateTime? DateCreated { get; }
        long? Size { get; }
    }

    public class ReportPacketView : IReportPacketView
    {
        private string filename;

        public string Filename
        {
            get { return filename; }
            set { filename = value; }
        }
        private DateTime? dateCreated;

        public DateTime? DateCreated
        {
            get { return dateCreated; }
            set { dateCreated = value; }
        }
        private long? size;

        public long? Size
        {
            get { return size; }
            set { size = value; }
        }
    }

    public class BaseReportView : IBaseReportView
    {
        private int baseId;

        public int BaseId
        {
            get { return baseId; }
            set { baseId = value; }
        }
        private string baseName;

        public string BaseName
        {
            get { return baseName; }
            set { baseName = value; }
        }
        private string groupName;

        public string GroupName
        {
            get { return groupName; }
            set { groupName = value; }
        }



        private string serviceName;

        public string ServiceName
        {
            get { return serviceName; }
            set { serviceName = value; }
        }
        private string serviceAddress;

        public string ServiceAddress
        {
            get { return serviceAddress; }
            set { serviceAddress = value; }
        }
        private string status;

        public string Status
        {
            get { return status; }
            set { status = value; }
        }
        private DateTime? dateComplete;

        public DateTime? DateComplete
        {
            get { return dateComplete; }
            set { dateComplete = value; }
        }
        private DateTime? dateCommand;

        public DateTime? DateCommand
        {
            get { return dateCommand; }
            set { dateCommand = value; }
        }
        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        private IEnumerable<IReportPacketView> loadPackets;

        public IEnumerable<IReportPacketView> LoadPackets
        {
            get { return loadPackets; }
            set { loadPackets = value; }
        }
        private IEnumerable<IReportPacketView> unloadPackets;

        public IEnumerable<IReportPacketView> UnloadPackets
        {
            get { return unloadPackets; }
            set { unloadPackets = value; }
        }

        private int? groupId;
        public int? GroupId
        {
            get { return groupId; }
            set { groupId = value; }
        }

        private int? serviceId;
        public int? ServiceId
        {
            get { return serviceId; }
            set { serviceId = value; }
        }

        private string mdRelease;

        public string MDRelease
        {
            get { return mdRelease; }
            set { mdRelease = value; }
        }
    }

    public interface IBaseReportView
    {
        int BaseId { get; }
        int? GroupId { get; }
        string BaseName { get; }
        string MDRelease { get; }
        string GroupName { get; }
        int? ServiceId { get; }
        string ServiceName { get; }
        string ServiceAddress { get; }
        string Status { get; }
        DateTime? DateComplete { get; }
        DateTime? DateCommand { get; }
        string Message { get; }
        IEnumerable<IReportPacketView> LoadPackets { get; }
        IEnumerable<IReportPacketView> UnloadPackets { get; }
    }

    public enum TableGrouper { group, service }
    public interface IBaseRepository
    {
        IEnumerable<IBaseReportView> GetBases(TableGrouper grouper);
        IBaseReportView GetBaseById(int baseId);
    }

    public class BaseRepository : IBaseRepository
    {
        private readonly URBD2Entities dataContext;
        private int userId;

        private IEnumerable<IReportPacketView> ParsePackets(string xmlSource)
        {
            List<IReportPacketView> list = new List<IReportPacketView>();
            if (string.IsNullOrEmpty(xmlSource))
                return list;
            XDocument xDoc = XDocument.Load(new StringReader(xmlSource));
            foreach (XElement elem in xDoc.Root.Elements())
            {
                IReportPacketView reportPacketView = new ReportPacketView
                {
                    Filename = elem.Element("filename").Value,
                    DateCreated = DateTime.Parse(elem.Element("date_created").Value),
                    Size = long.Parse(elem.Element("size").Value)
                };
                list.Add(reportPacketView);
            }
            return list;
        }

        private IBaseReportView ParseBaseReport(BaseReportList baseReport)
        {
            IBaseReportView baseReportView = new BaseReportView()
            {
                BaseId = baseReport.base_id,
                GroupId = baseReport.group_id,
                MDRelease = baseReport.md_release,
                ServiceId = baseReport.service_id,
                BaseName = baseReport.base_name,
                DateCommand = baseReport.date_command,
                DateComplete = baseReport.date_complete,
                GroupName = baseReport.group_name,
                Message = baseReport.message,
                Status = baseReport.status,
                ServiceName = baseReport.service_name,
                ServiceAddress = baseReport.service_address,
                LoadPackets = ParsePackets(baseReport.load_packets),
                UnloadPackets = ParsePackets(baseReport.unload_packets)
            };
            return baseReportView;
        }

        public IBaseReportView GetBaseById(int baseId)
        {
            return dataContext.BaseReportList.Where(b => b.base_id == baseId && b.user_id == userId).Select<BaseReportList, IBaseReportView>(new Func<BaseReportList, IBaseReportView>(x =>
            {
                return ParseBaseReport(x);
            })).Single();
        }

        public IEnumerable<IBaseReportView> GetBases(TableGrouper grouper)
        {
            return dataContext.BaseReportList.Where(x => x.user_id == userId).OrderBy(x => grouper == TableGrouper.group ? x.group_id : x.service_id).ThenBy(x=>x.base_id).Select<BaseReportList, IBaseReportView>(new Func<BaseReportList, IBaseReportView>(x =>
            {
                return ParseBaseReport(x);
            }));
        }

        public BaseRepository(URBD2Entities dataContext, int userId)
        {
            this.userId = userId;
            this.dataContext = dataContext;
        }
    }
}