using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Services;
using System.Data.Objects;

namespace Ugoria.URBD.WebControl.Models
{
    public interface IBase
    {
        int BaseId { get; }
        int GroupId { get; set; }
        int ServiceId { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        string SqlDatabase { get; set; }
        IEnumerable<IPacket> PacketList { get; }
        IEnumerable<IScheduleExchange> ScheduleExchangeList { get; }
        IEnumerable<IScheduleExtForms> ScheduleExtFormsList { get; }
    }

    public interface IReferenceSchedule
    {
        int? CurrentId { get; }
        int? BaseId { get; }
        string BaseName { get; }
        string Time { get; }
    }

    public interface IService
    {
        int ServiceId { get; }
        string Name { get; set; }
        string Address { get; set; }
        string Path1C { get; set; }
        IEnumerable<IServiceLog> LogList { get; }
        IEnumerable<IBase> BaseList { get; }
    }

    public interface IGroup
    {
        int GroupId { get; }
        string Name { get; set; }
    }

    public interface IReport
    {
        int ReportId { get; }
        int BaseId { get; set; }
        int StatusId { get; set; }
        DateTime? DateCommand { get; set; }
        DateTime? DateComplete { get; set; }
        string Mesasge { get; set; }
    }

    public interface IReportPacket
    {
        int ReportPacketId { get; }
        int ReportId { get; set; }
        long? Size { get; set; }
        DateTime? DatePacket { get; set; }
        string FileHash { get; set; }
    }

    public interface IPacket
    {
        int PacketId { get; }
        int BaseId { get; set; }
        PacketType Type { get; set; }
        string FileName { get; set; }
    }

    public interface IMail
    {
        int MailId { get; }
        string Address { get; set; }
    }

    public interface IServiceLog
    {
        int LogId { get; }
        DateTime? Date { get; set; }
        string Status { get; set; }
        string Text { get; set; }
    }

    public interface IScheduleExchange
    {
        int ScheduleId { get; }
        int BaseId { get; }
        string Time { get; set; }
        ModeType Mode { get; set; }
        bool? IsActive { get; set; }
        IBase ParentBase { get; }
    }

    public interface IScheduleExtForms
    {
        int ScheduleId { get; }
        string Time { get; set; }
        bool? IsActive { get; set; }
    }

    public interface IServiceScheduleExchange
    {
        int ScheduleId { get; }
        int BaseId { get; }
        int ServiceId { get; }
        string BaseName { get; }
        string Time { get; }
        string Address { get; }
        string Mode { get; }
    }

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
            get { return C1c_username; }
            set { C1c_username = value; }
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
        }

        public int GroupId
        {
            get { return group_id; }
            set { group_id = value; }
        }

        public IEnumerable<IPacket> PacketList
        {
            get { return Packet; }
        }

        public IEnumerable<IScheduleExchange> ScheduleExchangeList
        {
            get { return ScheduleExchange; }
        }

        public IEnumerable<IScheduleExtForms> ScheduleExtFormsList
        {
            get { return ScheduleExtForms; }
        }
    }

    public partial class ReferenceSchedule : IReferenceSchedule
    {
        public int? CurrentId
        {
            get { return current_id; }
        }

        public int? BaseId
        {
            get { return base_id; }
        }

        public string BaseName
        {
            get { return base_name; }
        }

        public string Time
        {
            get { return time; }
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

        public IEnumerable<IServiceLog> LogList
        {
            get { return ServiceLog; }
        }

        public IEnumerable<IBase> BaseList
        {
            get { return Base; }
        }
    }

    public partial class ServiceLog : IServiceLog
    {

        public int LogId { get { return log_id; } }

        public DateTime? Date
        {
            get { return date; }
            set { date = value; }
        }

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }
    }

    public partial class ScheduleExchange : IScheduleExchange
    {
        private IBase parentBase = null;

        public int ScheduleId
        {
            get { return schedule_id; }
        }

        public string Time
        {
            get { return time; }
            set { time = value; }
        }

        public ModeType Mode
        {
            get { return (ModeType)mode[0]; }
            set { mode = ((char)value).ToString(); }
        }

        public bool? IsActive
        {
            get { return is_active; }
            set { is_active = value; }
        }

        public IBase ParentBase
        {
            get { return parentBase; }
            set { parentBase = value; }
        }


        public int BaseId
        {
            get { return base_id; }
        }
    }

    public partial class ScheduleExtForms : IScheduleExtForms
    {
        public int ScheduleId
        {
            get { return schedule_id; }
        }

        public string Time
        {
            get { return time; }
            set { time = value; }
        }

        public bool? IsActive
        {
            get { return is_active; }
            set { is_active = value; }
        }
    }

    public partial class ScheduleExchangeOfService : IServiceScheduleExchange
    {
        public int ScheduleId { get { return schedule_id; } }
        public int BaseId { get { return base_id; } }
        public int ServiceId { get { return service_id; } }
        public string BaseName { get { return base_name; } }
        public string Time { get { return time; } }
        public string Address { get { return address; } }
        public string Mode { get { return mode; } }
    }

    /*
    public partial class Report : IReport
    {
        public int ReportId
        {
            get { return report_id; }
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
    }*/

    public partial class Packet : IPacket
    {
        public int PacketId
        {
            get { return packet_id; }
        }

        public int BaseId
        {
            get { return base_id; }
            set { base_id = value; }
        }

        public PacketType Type
        {
            get { return (PacketType)type[0]; }
            set { type = ((char)value).ToString(); }
        }

        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }
    }

    public interface IServiceRepository
    {
        IService GetServiceById(int serviceId);
        IEnumerable<IBase> GetBasesByServiceId(int serviceId);
        IEnumerable<IServiceScheduleExchange> GetScheduleByServiceId(int serviceId);
        IEnumerable<IReferenceSchedule> GetReferenceByBaseId(int baseId);
    }

    public class ServiceRepository : IServiceRepository
    {
        private readonly URBD2Entities dataContext;
        private Service service;

        public IService GetServiceById(int serviceId)
        {
            service = dataContext.Service.Where<Service>(s => s.service_id == serviceId)
                .Select(new Func<Service, Service>(s =>
                {
                    s.ServiceLog.Load();
                    return s;
                })).Single();
            return service;
            //Include("Group").Include("ScheduleExchange").Include("ScheduleExtForms").Include("Packet")
        }

        public IEnumerable<IBase> GetBasesByServiceId(int serviceId)
        {
            var t = dataContext.Base.Where(b => b.service_id == serviceId).Select<Base, IBase>(new Func<Base, IBase>(b =>
            {
                b.Packet.Load();
                b.ScheduleExchange.Load();
                b.ScheduleExtForms.Load();
                return b;
            }));
            //System.Diagnostics.Trace.WriteLine(((ObjectQuery)t).ToTraceString());
            return t;
        }

        public IEnumerable<IScheduleExchange> GetScheduleExchangeByBaseID(int baseId)
        {
            return service.Base.Where(b => baseId == b.base_id).Single().ScheduleExchange.Select<ScheduleExchange, IScheduleExchange>(s => s);
        }

        public IEnumerable<IScheduleExtForms> GetScheduleExtFormsByBaseId(int baseId)
        {
            return service.Base.Where(b => baseId == b.base_id).Single().ScheduleExtForms.Select<IScheduleExtForms, IScheduleExtForms>(s => s);
        }

        public IEnumerable<IServiceLog> GetLogsByServuceId(int serviceId)
        {
            return service.ServiceLog.Select(s => s);
        }

        public ServiceRepository(URBD2Entities dataContext)
        {
            this.dataContext = dataContext;
        }

        public IEnumerable<IServiceScheduleExchange> GetScheduleByServiceId(int serviceId)
        {
            IQueryable<IServiceScheduleExchange> q = dataContext.ScheduleExchangeOfService.Where(s => s.service_id == serviceId).OrderBy(b => b.time);
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            return q.Select(new Func<IServiceScheduleExchange, IServiceScheduleExchange>(s => { return s; }));
        }

        public IEnumerable<IReferenceSchedule> GetReferenceByBaseId(int baseId)
        {
            IQueryable<ReferenceSchedule> q = dataContext.ReferenceSchedule.Where(s => s.current_id == baseId).OrderBy(s => s.time)/*.ThenBy(s => s.base_id)*/.Select(s=>s);
            //System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            //q = q.OrderBy(s => s.time).ThenBy(s=>s.base_id);

            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            return q.Select(new Func<IReferenceSchedule, IReferenceSchedule>(s => { return s; }));
        }
    }
}