using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects;
using System.IO;
using System.Xml.Linq;
using System.Data.Entity;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.WebControl.ViewModels;

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

    public class ReportPacketViewModel2
    {
        public string Filename { get; set; }
        public DateTime DateCreated { get; set; }
        public long Size { get; set; }
        public PacketType Type { get; set; }
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
        public int BaseId { get; set; }
        public string BaseName { get; set; }
        public string GroupName { get; set; }
        public string ServiceName { get; set; }
        public string ServiceAddress { get; set; }
        public string Status { get; set; }
        public DateTime? DateComplete { get; set; }
        public DateTime? DateCommand { get; set; }
        public string Message { get; set; }
        public IEnumerable<ReportPacketViewModel> Packets { get; set; }
        public IEnumerable<ExtDirectoriesFileViewModel> Files { get; set; }
        public int GroupId { get; set; }
        public int ServiceId { get; set; }
        public string MDRelease { get; set; }
        public int ParentBaseId { get; set; }
        public string ParentBaseName { get; set; }
        public DateTime? DateMlgLog { get; set; }
    }

    public interface IBaseReportView
    {
        int BaseId { get; }
        int GroupId { get; }
        string BaseName { get; }
        int ParentBaseId { get; }
        string ParentBaseName { get; }
        string MDRelease { get; }
        string GroupName { get; }
        int ServiceId { get; }
        string ServiceName { get; }
        string ServiceAddress { get; }
        string Status { get; }
        DateTime? DateComplete { get; }
        DateTime? DateCommand { get; }
        string Message { get; }
        DateTime? DateMlgLog { get; }
        IEnumerable<ReportPacketViewModel> Packets { get; }
        IEnumerable<ExtDirectoriesFileViewModel> Files { get; }
    }

    public interface IObjectType
    {
        int TypeId { get; }
        string Type { get; }
        string Number { get; }
        string Name { get; }
    }

    public partial class ObjectType : IObjectType
    {
        public int TypeId { get { return type_id; } }
        public string Type { get { return type; } }
        public string Number { get { return number; } }
        public string Name { get { return name; } }
    }

    public interface IObject
    {
        int ObjectId { get; }
        int TypeId { get; }
        string BaseCode { get; }
        string Identifier { get; }
    }

    public partial class Object : IObject
    {

        public int ObjectId
        {
            get { return object_id; }
        }

        public int TypeId
        {
            get { return type_id.Value; }
        }

        public string BaseCode
        {
            get { return base_code; }
        }

        public string Identifier
        {
            get { return identifier; }
        }
    }

    public enum TableGrouper { group, service, filial }
    public interface IBaseRepository
    {
        IEnumerable<IBaseReportView> GetBases(TableGrouper grouper, string component);
        IBaseReportView GetBaseById(int baseId, string component);
        IEnumerable<IObjectType> GetObjectTypes();
        IEnumerable<string> GetBaseCodes();
        IEnumerable<ReportLog> GetReportLogByObject(int typeId, string baseCode, string identifier, DateTime startDate, DateTime endDate);
    }

    public class BaseRepository : IBaseRepository
    {
        private readonly URBD2Entities dataContext;
        private int userId;
        private bool isAdminAccess = false;

        public IEnumerable<ReportLog> GetReportLogByObject(int typeId, string baseCode, string identifier, DateTime startDate, DateTime endDate)
        {
            var query = dataContext.ReportLog.Where(o=>o.date_message >= startDate && o.date_message <= endDate);
            if (!string.IsNullOrEmpty(identifier))
                query = query.Where(o => o.Object.identifier.Equals(identifier));
            if (typeId > 0)
                query = query.Where(o => o.Object.type_id == typeId);
            if (!string.IsNullOrEmpty(baseCode))
                query = query.Where(o => o.Object.base_code.Equals(baseCode));
            return query.Include(l => l.Object).Include(l => l.EventType.EventGroup).Include(l=>l.Report.Base).OrderByDescending(l=>l.date_message).ToList();
        }

        public IEnumerable<IObjectType> GetObjectTypes()
        {
            return dataContext.ObjectType.Select(x => x).ToList();
        }
        public IEnumerable<string> GetBaseCodes()
        {
            return dataContext.Object.GroupBy(o => o.base_code).Select(x => x.Key);
        }

        public IBaseReportView GetBaseById(int baseId, string componentName)
        {
            var query = (isAdminAccess
                ? dataContext.BaseReportList
                    .Where(x => x.component_name.Equals(componentName))
                : dataContext.UserBasesPermission
                    .Where(p => p.user_id == userId)
                    .Join(dataContext.BaseReportList.Where(x => x.component_name.Equals(componentName)).Select(b => b), p => p.base_id, b => b.base_id, (p, b) => b))
                    .Where(b => b.base_id == baseId);

            var query2 = query.Select<BaseReportList, IBaseReportView>(new Func<BaseReportList, BaseReportView>(x => new BaseReportView
             {
                 BaseId = x.base_id,
                 GroupId = x.group_id,
                 BaseName = x.base_name,
                 DateCommand = x.date_command,
                 DateComplete = x.date_complete,
                 Message = x.message,
                 ParentBaseId = x.parent_base_id,
                 ParentBaseName = x.parent_base_name,
                 ServiceId = x.service_id,
                 ServiceAddress = x.service_address,
                 GroupName = x.group_name,
                 MDRelease = x.md_release,
                 ServiceName = x.service_name,
                 Status = x.status
             }));
            //System.Diagnostics.Trace.WriteLine(((ObjectQuery)query2).ToTraceString() + " " + query2.Count());
            System.Diagnostics.Trace.WriteLine(query.Count());
            if (query2.Count() == 0)
                return null;
            return query2.Single();
        }

        public IEnumerable<IBaseReportView> GetBases(TableGrouper grouper, string componentName)
        {
            var query = isAdminAccess
                ? dataContext.BaseReportList
                    .Where(x => x.component_name.Equals(componentName))
                : dataContext.UserBasesPermission
                    .Where(x => x.user_id == userId)
                    .Join(dataContext.BaseReportList.Where(x => x.component_name.Equals(componentName))
                    .Select(b => b), p => p.base_id, b => b.base_id, (p, b) => b);
            if (grouper == TableGrouper.group)
                query = query.OrderBy(x => x.group_id).ThenBy(x => x.base_name);
            else if (grouper == TableGrouper.service)
                query = query.OrderBy(x => x.service_id).ThenBy(x => x.base_name);
            else
                query = query.OrderBy(x => x.parent_base_name);
            //System.Diagnostics.Trace.Write(((ObjectQuery)query.OrderBy(x => x.base_name)).ToTraceString());

            if ("Exchange".Equals(componentName))
            {
                return query.Select(x => new BaseReportView
                {
                    BaseId = x.base_id,
                    GroupId = x.group_id,
                    BaseName = x.base_name,
                    DateCommand = x.date_command,
                    DateComplete = x.date_complete,
                    Message = x.message,
                    ParentBaseId = x.parent_base_id,
                    ParentBaseName = x.parent_base_name,
                    ServiceId = x.service_id,
                    ServiceAddress = x.service_address,
                    GroupName = x.group_name,
                    MDRelease = x.md_release,
                    ServiceName = x.service_name,
                    Status = x.status,
                    Packets = dataContext.Packet.Join(dataContext.ReportPacket,
                        p => p.packet_id,
                        rp => rp.packet_id,
                        (p, rp) =>
                            new
                            {
                                packet = p,
                                reportPacket = rp
                            }).Where(rp => rp.packet.base_id == x.base_id && rp.reportPacket.rp_id == dataContext.ReportPacket.Where(rp2 => rp2.packet_id == rp.reportPacket.packet_id).Max(m => m.rp_id)).Select(rp => new ReportPacketViewModel
                            {
                                CreatedDate = rp.reportPacket.date_created.Value,
                                Packet = new PacketViewModel { FileName = rp.packet.filename, PacketId = rp.packet.packet_id, Type = rp.packet.type },
                                Size = rp.reportPacket.size.Value
                            })
                });
            }
            else if ("MlgCollect".Equals(componentName))
            {
                return query.Select(x => new BaseReportView
                {
                    BaseId = x.base_id,
                    GroupId = x.group_id,
                    BaseName = x.base_name,
                    DateCommand = x.date_command,
                    DateComplete = x.date_complete,
                    Message = x.message,
                    ParentBaseId = x.parent_base_id,
                    ParentBaseName = x.parent_base_name,
                    ServiceId = x.service_id,
                    ServiceAddress = x.service_address,
                    GroupName = x.group_name,
                    MDRelease = x.md_release,
                    ServiceName = x.service_name,
                    Status = x.status,
                    DateMlgLog = x.mlg_date_log
                });
            }
            else
            {
                return query.Select(x => new BaseReportView
                {
                    BaseId = x.base_id,
                    GroupId = x.group_id,
                    BaseName = x.base_name,
                    DateCommand = x.date_command,
                    DateComplete = x.date_complete,
                    Message = x.message,
                    ParentBaseId = x.parent_base_id,
                    ParentBaseName = x.parent_base_name,
                    ServiceId = x.service_id,
                    ServiceAddress = x.service_address,
                    GroupName = x.group_name,
                    MDRelease = x.md_release,
                    ServiceName = x.service_name,
                    Status = x.status,
                    Files = dataContext.ExtDirectoryFile.Where(f => f.report_id == dataContext.ExtDirectoryFile.Where(f2 => f2.Report.base_id == x.base_id).Max(f2 => f2.report_id)).Select(f =>
                        new ExtDirectoriesFileViewModel
                            {
                                FileId = f.file_id,
                                Filename = f.filename,
                                Size = f.size,
                                DateCopied = f.date_copied
                            })
                });
            }
        }

        public BaseRepository(URBD2Entities dataContext, int userId, bool isAdminAccess = false)
        {
            this.userId = userId;
            this.dataContext = dataContext;
            this.isAdminAccess = isAdminAccess;
        }
    }
}