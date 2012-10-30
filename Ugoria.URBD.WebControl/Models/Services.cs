using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Services;
using System.Data.Objects;
using Ugoria.URBD.WebControl.ViewModels;

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
        string Message { get; set; }
        int UserId { get; set; }
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

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public int UserId
        {
            get { return user_id; }
            set { user_id = value; }
        }
    }

    /*public partial class ReportPacket : IReportPacket
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
        IEnumerable<IServiceLog> GetServiceLogs(int serviceId);
        IEnumerable<BaseViewModel> GetBasesByServiceId(int serviceId);
        IEnumerable<IServiceScheduleExchange> GetServiceScheduleExchangeByServiceId(int serviceId);
        IEnumerable<IServiceScheduleExchange> GetServiceScheduleExchangeByBaseId(int baseId);
        IEnumerable<IReferenceSchedule> GetReferenceByBaseId(int baseId);
        IBase GetBaseById(int baseId);
        IEnumerable<ReportViewModel> GetReportsByBaseId(int baseId);
    }

    public class ServiceRepository : IServiceRepository
    {
        private readonly URBD2Entities dataContext;

        public IService GetServiceById(int serviceId)
        {
            var query = dataContext.Service.Where(s => s.service_id == serviceId).Select(s => s);
            if (query.Count() == 0)
                return null;
            return query.Select(s => s).Single();
            //return service;
            //Include("Group").Include("ScheduleExchange").Include("ScheduleExtForms").Include("Packet")
        }

        public IEnumerable<IServiceLog> GetServiceLogs(int serviceId)
        {
            return dataContext.ServiceLog.Where(l => l.service_id == serviceId).OrderByDescending(l => l.log_id).Take(10);
        }

        public IEnumerable<BaseViewModel> GetBasesByServiceId(int serviceId)
        {
            var t = dataContext.Base.Include("ScheduleExchange").Include("ScheduleExtForms").Include("Packet").Where(b => b.service_id == serviceId).Select<Base, BaseViewModel>(b => new BaseViewModel
            {
                BaseId = b.base_id,
                ServiceId = b.service_id,
                ServiceAddress = b.Service.address,
                Name = b.base_name
            });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)t).ToTraceString());
            return t;
        }

        public IBase GetBaseById(int baseId)
        {
            var query = dataContext.Base.Where(b => b.base_id == baseId).Select(b => b);
            if (query.Count() == 0)
                return null;
            return query.Single();
        }

        public IEnumerable<IScheduleExchange> GetScheduleExchangeByBaseID(int baseId)
        {
            return dataContext.Base.Where(b => baseId == b.base_id).Single().ScheduleExchange.Select<ScheduleExchange, IScheduleExchange>(s => s);
        }

        public IEnumerable<IScheduleExtForms> GetScheduleExtFormsByBaseId(int baseId)
        {
            return dataContext.Base.Where(b => baseId == b.base_id).Single().ScheduleExtForms.Select<IScheduleExtForms, IScheduleExtForms>(s => s);
        }

        public ServiceRepository(URBD2Entities dataContext)
        {
            this.dataContext = dataContext;
        }

        public IEnumerable<IServiceScheduleExchange> GetServiceScheduleExchangeByServiceId(int serviceId)
        {
            IQueryable<IServiceScheduleExchange> q = dataContext.ScheduleExchangeOfService.Where(s => s.service_id == serviceId).OrderBy(b => b.time);
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            return q.Select(new Func<IServiceScheduleExchange, IServiceScheduleExchange>(s => s));
        }

        public IEnumerable<IServiceScheduleExchange> GetServiceScheduleExchangeByBaseId(int baseId)
        {
            IQueryable<IServiceScheduleExchange> q = dataContext.Base.Where(b => b.base_id == baseId)
                .Join(dataContext.ScheduleExchangeOfService.Select(s => s), b => b.service_id, s => s.service_id, (b, s) => s)
                .OrderBy(b => b.time);
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            return q.Select(new Func<IServiceScheduleExchange, IServiceScheduleExchange>(s => s));
        }

        public IEnumerable<IReferenceSchedule> GetReferenceByBaseId(int baseId)
        {
            IQueryable<ReferenceSchedule> q = dataContext.ReferenceSchedule.Where(s => s.current_id == baseId).OrderBy(s => s.time)/*.ThenBy(s => s.base_id)*/.Select(s => s);
            //System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            //q = q.OrderBy(s => s.time).ThenBy(s=>s.base_id);

            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            return q.Select(new Func<IReferenceSchedule, IReferenceSchedule>(s => { return s; }));
        }

        public IEnumerable<ReportViewModel> GetReportsByBaseId(int baseId)
        {
            var query = dataContext.Report.Where(r => r.base_id == baseId).Where(r => r.date_command == DateTime.Now).OrderByDescending(r => r.date_command).Select<Report, ReportViewModel>(r =>
                new ReportViewModel
                {
                    ReportId = r.report_id,
                    CommandDate = r.date_command,
                    CompleteDate = r.date_complete,
                    Status = r.ReportStatus.name,
                    Message = r.message,
                    User = r.User.user_name,
                    LaunchList = r.Launch1C.Select(l => new LaunchViewModel
                    {
                        LaunchId = l.launch_id,
                        Pid = l.pid,
                        StartDate = l.date_start
                    }),
                    PacketList = r.ReportPacket.Select(p => new ReportPacketViewModel
                    {
                        CreatedDate = p.date_created,
                        ReportPacketId = p.rp_id,
                        Size = p.size,
                        Packet = new PacketViewModel
                        {
                            FileName = p.Packet.filename,
                            PacketId = p.Packet.packet_id,
                            Type = p.Packet.type
                        }
                    })
                });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            IEnumerable<ReportViewModel> q= query.Select(new Func<ReportViewModel, ReportViewModel>(r => r));
            return q;
        }
    }
}