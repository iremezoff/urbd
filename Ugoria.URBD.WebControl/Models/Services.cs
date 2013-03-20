using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Services;
using System.Data.Objects;
using Ugoria.URBD.WebControl.ViewModels;
using System.Data.Objects.DataClasses;
using System.Data.Linq;
using System.Data.Entity;

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
        IEnumerable<IScheduleExtDirectories> ScheduleExtDirectoriesList { get; }
        IEnumerable<IScheduleMlgCollect> ScheduleMlgCollectList { get; }
        IEnumerable<IExtDirectory> ExtDirectoriesList { get; }
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
        int ComponentStatusId { get; set; }
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
        int? BaseId { get; set; }
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

    public interface IScheduleExtDirectories
    {
        int ScheduleId { get; }
        string Time { get; set; }
        bool? IsActive { get; set; }
    }

    public interface IScheduleMlgCollect
    {
        int ScheduleId { get; }
        string Time { get; set; }
        bool? IsActive { get; set; }
    }

    public interface IExtDirectory
    {
        int DirId { get; }
        string LocalPath { get; }
        string FtpPath { get; }
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

    public partial class ExtDirectory : IExtDirectory
    {
        public string LocalPath
        {
            get { return local_path; }
            set { local_path = value; }
        }

        public string FtpPath
        {
            get { return ftp_path; }
            set { ftp_path = value; }
        }

        public int DirId
        {
            get { return dir_id; }
            set { dir_id = value; }
        }
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

        public IEnumerable<IScheduleExtDirectories> ScheduleExtDirectoriesList
        {
            get { return ScheduleExtDirectories; }
        }

        private IEnumerable<ExtDirectory> extDirectories;
        public IEnumerable<ExtDirectory> ExtDirectories
        {
            get
            {
                if (extDirectories == null)
                {
                    extDirectories = ExtDirectoryBase.Select(c => c.ExtDirectory);
                }
                return extDirectories;
            }
        }

        public IEnumerable<IExtDirectory> ExtDirectoriesList
        {
            get { return extDirectories; }
        }


        public IEnumerable<IScheduleMlgCollect> ScheduleMlgCollectList
        {
            get { return ScheduleMlgCollect; }
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

    public partial class ScheduleExtDirectories : IScheduleExtDirectories
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

    public partial class ScheduleMlgCollect : IScheduleMlgCollect
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

        public int ComponentStatusId
        {
            get { return component_status_id; }
            set { component_status_id = value; }
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

    public interface IComponentReportStatus
    {
        int ComponentStatusId { get; }
        IReportStatus Status { get; }
        IComponent Component { get; }
    }



    public interface IReportStatus
    {
        int StatusId { get; }
        string Name { get; }
    }

    public partial class ReportStatus : IReportStatus
    {
        public int StatusId { get { return status_id; } }
        public string Name { get { return name; } }
    }



    public partial class ComponentReportStatus : IComponentReportStatus
    {
        public int ComponentStatusId { get { return component_status_id; } }

        public IReportStatus Status { get { return ReportStatus; } }

        IComponent IComponentReportStatus.Component { get { return this.Component; } }
    }

    public interface IReportNotification
    {

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

        public int? BaseId
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
        IEnumerable<ServiceViewModel> GetServices();
        ServiceViewModel GetServiceById(int serviceId);
        IEnumerable<ServiceLogViewModel> GetServiceLogs(int serviceId, DateTime startDate, DateTime endDate);
        IEnumerable<BaseViewModel> GetBasesByServiceId(int serviceId, int userId, bool isAdmin);
        IEnumerable<IServiceScheduleExchange> GetServiceScheduleExchangeByServiceId(int serviceId);
        IEnumerable<IServiceScheduleExchange> GetServiceScheduleExchangeByBaseId(int baseId);
        IEnumerable<BaseTreeSchedule> GetReferenceByBaseId(int baseId);
        BaseViewModel GetBaseById(int baseId);
        IEnumerable<ExtDirectoryReportView> GetExtDirectoriesReportsByBaseId(int baseId, DateTime startDate, DateTime endDate);
        IEnumerable<ExchangeReportView> GetExchangeReportsByBaseId(int baseId, DateTime startDate, DateTime endDate);
        IEnumerable<MlgCollectReportView> GetMlgCollectReportsByBaseId(int baseId, DateTime startDate, DateTime endDate);
        IEnumerable<ComponentReportStatusView> GetComponentReportStatusByUserId(int userId, int baseId);
        IEnumerable<IExtDirectory> GetExtDirectories();
        IEnumerable<UserBasesPermission> GetBasePermissions(int baseId);
        void SaveBase(BaseViewModel baseVM);
        void SaveService(ServiceViewModel serviceVM);
        void SaveNotification(ReportNotificationViewModel notify, int baseId, int userId);
        void SavePermission(PermissionViewModel permission, int baseId);
    }

    public class ServiceRepository : IServiceRepository
    {
        private readonly URBD2Entities dataContext;

        public ServiceViewModel GetServiceById(int serviceId)
        {
            var query = dataContext.Service.Where(s => s.service_id == serviceId).Select(s => s);
            if (query.Count() == 0)
                return null;
            return query.Select(s => new ServiceViewModel
            {
                Address = s.address,
                Name = s.service_name,
                Path1C = s.C1c_path,
                ServiceId = s.service_id
            }).Single();
        }

        public IEnumerable<ServiceLogViewModel> GetServiceLogs(int serviceId, DateTime startDate, DateTime endDate)
        {
            return dataContext.ServiceLog.Where(s => s.service_id == serviceId && s.date > EntityFunctions.TruncateTime(startDate) && EntityFunctions.TruncateTime(EntityFunctions.AddDays(endDate, 1)) >= s.date)
                .OrderByDescending(s => s.date).Select<ServiceLog, ServiceLogViewModel>(l => new ServiceLogViewModel
                {
                    LogId = l.log_id,
                    Date = l.date.Value,
                    ServiceId = l.service_id,
                    Status = l.status,
                    Text = l.text
                });
        }

        public IEnumerable<BaseViewModel> GetBasesByServiceId(int serviceId, int userId, bool isAdmin)
        {
            var query = dataContext.Base.Include(b => b.Packet).Where(b => b.service_id == serviceId);

            if (!isAdmin)
                query = query.Join(dataContext.UserBasesPermission, b => b.base_id, p => p.base_id, (b, p) => new { Base = b, Permission = p }).Where(bp => bp.Permission.user_id == userId).Select(bp => bp.Base);

            var query2 = query.OrderBy(b => b.base_name).Select<Base, BaseViewModel>(b => new BaseViewModel
            {
                BaseId = b.base_id,
                ServiceId = b.service_id,
                ServiceAddress = b.Service.address,
                Name = b.base_name
            });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query2).ToTraceString());
            return query2;
        }

        public void SaveBase(BaseViewModel baseVM)
        {
            Base @base = dataContext.Base.Include("ScheduleExchange").Include("ScheduleExtDirectories").Include("Packet").Where(b => b.base_id == baseVM.BaseId).Single();
            @base.C1c_database = baseVM.Path;
            @base.Username = baseVM.Username;
            @base.Password = baseVM.Password;
            @base.Service.date_change = DateTime.Now; // изменение текущего сервиса
            if (baseVM.ServiceId != @base.ServiceId)
            {
                @base.ServiceId = baseVM.ServiceId;
                @base.Service.date_change = DateTime.Now; // изменение нового сервиса
            }

            if (baseVM.PacketList != null)
            {
                @base.Packet.Where(p => !baseVM.PacketList.Any(pvm => pvm.PacketId == p.PacketId)).ToList().ForEach(p => p.Base = null);
                foreach (PacketViewModel packetVM in baseVM.PacketList)
                {
                    if (packetVM.PacketId == 0)
                    {
                        dataContext.Packet.AddObject(new Packet
                        {
                            Base = @base,
                            filename = packetVM.FileName,
                            type = packetVM.Type
                        });
                        continue;
                    }
                    Packet packet = @base.Packet.FirstOrDefault(p => p.PacketId == packetVM.PacketId);
                    if (packet == null)
                        continue;
                    packet.FileName = packetVM.FileName;
                    packet.type = packetVM.Type;
                }
            }

            if (baseVM.ScheduleExchangeList != null)
            {
                @base.ScheduleExchange.Where(s => !baseVM.ScheduleExchangeList.Any(svm => svm.ScheduleId == s.ScheduleId)).ToList().ForEach(s => dataContext.ScheduleExchange.DeleteObject(s));
                foreach (ScheduleExchangeViewModel scheduleVM in baseVM.ScheduleExchangeList)
                {
                    if (scheduleVM.ScheduleId == 0)
                    {
                        dataContext.ScheduleExchange.AddObject(new ScheduleExchange
                        {
                            Base = @base,
                            time = scheduleVM.Time,
                            mode = scheduleVM.Mode,
                            is_active = true
                        });
                        continue;
                    }
                    ScheduleExchange schedule = @base.ScheduleExchange.FirstOrDefault(s => s.ScheduleId == scheduleVM.ScheduleId);
                    if (schedule == null)
                        continue;
                    schedule.time = scheduleVM.Time;
                    schedule.mode = scheduleVM.Mode;
                }
            }

            if (baseVM.ScheduleExtDirectoriesList != null)
            {
                @base.ScheduleExtDirectories.Where(s => !baseVM.ScheduleExtDirectoriesList.Any(svm => svm.ScheduleId == s.ScheduleId)).ToList().ForEach(s => dataContext.ScheduleExtDirectories.DeleteObject(s));
                foreach (ScheduleExtDirectoriesViewModel scheduleVM in baseVM.ScheduleExtDirectoriesList)
                {
                    if (scheduleVM.ScheduleId == 0)
                    {
                        dataContext.ScheduleExtDirectories.AddObject(new ScheduleExtDirectories
                        {
                            Base = @base,
                            time = scheduleVM.Time,
                            is_active = true
                        });
                        continue;
                    }
                    ScheduleExtDirectories schedule = @base.ScheduleExtDirectories.FirstOrDefault(s => s.ScheduleId == scheduleVM.ScheduleId);
                    if (schedule == null)
                        continue;
                    schedule.Time = scheduleVM.Time;
                }
            }

            if (baseVM.ScheduleMlgCollectList != null)
            {
                @base.ScheduleMlgCollect.Where(s => !baseVM.ScheduleMlgCollectList.Any(svm => svm.ScheduleId == s.ScheduleId)).ToList().ForEach(s => dataContext.ScheduleMlgCollect.DeleteObject(s));
                foreach (ScheduleMlgCollectViewModel scheduleVM in baseVM.ScheduleMlgCollectList)
                {
                    if (scheduleVM.ScheduleId == 0)
                    {
                        dataContext.ScheduleMlgCollect.AddObject(new ScheduleMlgCollect
                        {
                            Base = @base,
                            time = scheduleVM.Time,
                            is_active = true
                        });
                        continue;
                    }
                    ScheduleMlgCollect schedule = @base.ScheduleMlgCollect.FirstOrDefault(s => s.ScheduleId == scheduleVM.ScheduleId);
                    if (schedule == null)
                        continue;
                    schedule.Time = scheduleVM.Time;
                }
            }

            if (@baseVM.ExtDirectoriesList != null)
            {
                @base.ExtDirectoryBase.Where(s => !baseVM.ExtDirectoriesList.Any(svm => svm.DirId == s.dir_id)).ToList().ForEach(s => dataContext.ExtDirectoryBase.DeleteObject(s));
                IEnumerable<ExtDirectory> extDirs = dataContext.ExtDirectory.Select(d => d);
                foreach (ExtDirectoryViewModel extDirectoryVM in @baseVM.ExtDirectoriesList.GroupBy(d => d.DirId, (d, c) => c.First()))
                {
                    ExtDirectory extDirectory = @base.ExtDirectories.FirstOrDefault(d => d.DirId == extDirectoryVM.DirId);
                    if (extDirectory == null)
                    {
                        //dataContext.ExtDirectoryBase.AddObject()
                        dataContext.ExtDirectoryBase.AddObject(new ExtDirectoryBase { Base = @base, ExtDirectory = extDirs.First(d => d.DirId == extDirectoryVM.DirId) });
                    }
                }
            }
        }

        public void SaveService(ServiceViewModel serviceVM)
        {
            Service service = dataContext.Service.Where(s => s.service_id == serviceVM.ServiceId).SingleOrDefault();
            service.service_name = serviceVM.Name;
            service.C1c_path = serviceVM.Path1C;
            service.address = serviceVM.Address;
            service.date_change = DateTime.Now;
            dataContext.SaveChanges();
        }

        public BaseViewModel GetBaseById(int baseId)
        {
            var query = dataContext.Base.Where(b => b.base_id == baseId).Include(b => b.ScheduleExchange).Include(b => b.ScheduleExtDirectories).Include(b => b.ExtDirectoryBase).Select(b => new BaseViewModel
            {
                BaseId = b.base_id,
                Name = b.base_name,
                SqlDatabase = b.sql_database,
                Path = b.C1c_database,
                Username = b.C1c_username,
                Password = b.C1c_password,
                ServiceId = b.service_id,
                ServiceAddress = b.Service.address,
                PacketList = b.Packet.Select(p => new PacketViewModel { PacketId = p.packet_id, FileName = p.filename, Type = p.type }),
                ScheduleExchangeList = b.ScheduleExchange.OrderBy(s => s.time).Select(s => new ScheduleExchangeViewModel { ScheduleId = s.schedule_id, Mode = s.mode, Time = s.time }),
                ScheduleExtDirectoriesList = b.ScheduleExtDirectories.OrderBy(s => s.time).Select(s => new ScheduleExtDirectoriesViewModel { ScheduleId = s.schedule_id, Time = s.time }),
                ScheduleMlgCollectList = b.ScheduleMlgCollect.OrderBy(s => s.time).Select(s => new ScheduleMlgCollectViewModel { ScheduleId = s.schedule_id, Time = s.time }),
                ExtDirectoriesList = b.ExtDirectoryBase.Select(d => new ExtDirectoryViewModel { DirId = d.dir_id, FtpPath = d.ExtDirectory.ftp_path, LocalPath = d.ExtDirectory.local_path })
            });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());

            return query.SingleOrDefault();
        }

        public IEnumerable<IScheduleExchange> GetScheduleExchangeByBaseID(int baseId)
        {
            return dataContext.Base.Where(b => baseId == b.base_id).Single().ScheduleExchange.Select<ScheduleExchange, IScheduleExchange>(s => s);
        }

        public IEnumerable<IScheduleExtDirectories> GetScheduleExtFormsByBaseId(int baseId)
        {
            return dataContext.Base.Where(b => baseId == b.base_id).Single().ScheduleExtDirectories.Select<IScheduleExtDirectories, IScheduleExtDirectories>(s => s);
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

        public IEnumerable<BaseTreeSchedule> GetReferenceByBaseId(int baseId)
        {
            IQueryable<BaseTreeSchedule> q = dataContext.ReferenceOfBase.Where(s => s.current_id == baseId)
                .Join(dataContext.ScheduleExchange, r => r.base_id, s => s.base_id, (r, s) => new { Reference = r, Schedule = s })
                .OrderBy(rs => rs.Schedule.time)/*.ThenBy(s => s.base_id)*/
                .Select(rs => new BaseTreeSchedule { BaseId = rs.Reference.base_id, BaseName = rs.Reference.base_name, CurrentId = rs.Reference.current_id.Value, Time = rs.Schedule.time });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            //q = q.OrderBy(s => s.time).ThenBy(s=>s.base_id);

            System.Diagnostics.Trace.WriteLine(((ObjectQuery)q).ToTraceString());

            return q.Select(s => s);
        }

        public IEnumerable<ExchangeReportView> GetExchangeReportsByBaseId(int baseId, DateTime startDate, DateTime endDate)
        {
            var query = dataContext.Report.Include("Launch1C").Include("ReportPacket.Packet")
              .Where(r => r.ComponentReportStatus.Component.name.Equals("Exchange") && r.base_id == baseId && r.date_command >= EntityFunctions.TruncateTime(startDate) && EntityFunctions.TruncateTime(EntityFunctions.AddDays(endDate, 1)) >= r.date_command)
              .OrderByDescending(r => r.date_command)
              .Select<Report, ExchangeReportView>(r =>
                  new ExchangeReportView
                  {
                      ReportId = r.report_id,
                      CommandDate = r.date_command,
                      CompleteDate = r.date_complete,
                      Status = r.ComponentReportStatus.ReportStatus.name,
                      Message = r.message,
                      User = r.User.user_name,
                      LaunchList = r.Launch1C,
                      PacketList = r.ReportPacket                      
                  });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            //IEnumerable<ReportViewModel> q = query.Select(new Func<ReportViewModel, ReportViewModel>(r => r));
            return query.ToList();
        }

        public IEnumerable<ExtDirectoryReportView> GetExtDirectoriesReportsByBaseId(int baseId, DateTime startDate, DateTime endDate)
        {
            var query = dataContext.Report.Include(r=>r.Launch1C).Include("ReportPacket.Packet")
                .Where(r => r.ComponentReportStatus.Component.name.Equals("ExtDirectories") && r.base_id == baseId && r.date_command >= EntityFunctions.TruncateTime(startDate) && EntityFunctions.TruncateTime(EntityFunctions.AddDays(endDate, 1)) >= r.date_command)
                .OrderByDescending(r => r.date_command)
                .Select<Report, ExtDirectoryReportView>(r =>
                    new ExtDirectoryReportView
                    {
                        ReportId = r.report_id,
                        CommandDate = r.date_command,
                        CompleteDate = r.date_complete,
                        Status = r.ComponentReportStatus.ReportStatus.name,
                        Message = r.message,
                        User = r.User.user_name,
                        Files = r.ExtDirectoryFile
                    });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            return query.ToList();
        }

        public IEnumerable<MlgCollectReportView> GetMlgCollectReportsByBaseId(int baseId, DateTime startDate, DateTime endDate)
        {
            var query = dataContext.Report.Include(r=>r.Launch1C)
                .Where(r => r.ComponentReportStatus.Component.name.Equals("MlgCollect") && r.base_id == baseId && r.date_command >= EntityFunctions.TruncateTime(startDate) && EntityFunctions.TruncateTime(EntityFunctions.AddDays(endDate, 1)) >= r.date_command)
                .OrderByDescending(r => r.date_command)
                .Select<Report, MlgCollectReportView>(r =>
                    new MlgCollectReportView
                    {
                        ReportId = r.report_id,
                        CommandDate = r.date_command,
                        CompleteDate = r.date_complete,
                        Status = r.ComponentReportStatus.ReportStatus.name,
                        Message = r.message,
                        User = r.User.user_name,
                        DateMlgDate = r.ReportLog.Max(l => l.date_message)
                    });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            return query.ToList();
        }

        public IEnumerable<ComponentReportStatusView> GetComponentReportStatusByUserId(int userId, int baseId)
        {
            var query = dataContext.ComponentReportStatus
                .GroupJoin(dataContext.ReportNotification.Where(rn => rn.UserBasesPermission.user_id == userId && rn.UserBasesPermission.base_id == baseId), crs => crs.component_status_id, rn => rn.component_status_id, (crs, rn) => new { crs, rn })
                .SelectMany(x => x.rn.DefaultIfEmpty(), (crs, rn) => new ComponentReportStatusView { Component = crs.crs.Component, ComponentReportStatusId = crs.crs.component_status_id, Notification = crs.rn.FirstOrDefault(), ReportStatus = crs.crs.ReportStatus });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            return query;
        }

        public IEnumerable<IExtDirectory> GetExtDirectories()
        {
            return dataContext.ExtDirectory.Select(d => d).ToList();
        }

        IEnumerable<UserBasesPermission> ubpCache;

        public void SavePermission(PermissionViewModel permissionVM, int baseId)
        {
            if (ubpCache == null)
                ubpCache = GetBasePermissions(baseId);
            if (permissionVM.PermissionId == 0)
            {
                if (permissionVM.UserId != 0)
                    dataContext.UserBasesPermission.AddObject(new UserBasesPermission { base_id = baseId, user_id = permissionVM.UserId, allow_configure = permissionVM.AllowConfigure });
                return;
            }
            UserBasesPermission permission = ubpCache.Where(p => p.permission_id == permissionVM.PermissionId).SingleOrDefault();
            if (permission == null)
                return;
            else if (permissionVM.UserId == 0)
                dataContext.UserBasesPermission.DeleteObject(permission);
            else
            {
                permission.allow_configure = permissionVM.AllowConfigure;
                permission.user_id = permissionVM.UserId;
            }
        }

        private UserBasesPermission cacheNotification;
        public void SaveNotification(ReportNotificationViewModel notify, int baseId, int userId)
        {
            // условие: если кеш пуст либо в кеше хранятся записи оповещения другого разрешения (baseId и userId различаются), то кеш надо обновить
            if (cacheNotification == null || cacheNotification.base_id != baseId || cacheNotification.user_id != userId)
            {
                //cacheNotification = dataContext.ReportNotification.Where(rn => rn.UserBasesPermission.user_id == userId && rn.UserBasesPermission.base_id == baseId).ToList();
                cacheNotification = dataContext.UserBasesPermission.Include(ubp => ubp.ReportNotification).Where(ubp => ubp.base_id == baseId && ubp.user_id == userId).SingleOrDefault();
            }
            // если отсутствует разрешение на базу, то нотификация редактируется админом, т.к. этот код не может быть вызван в контексте пользователя с отсутствующим разрешением
            if (cacheNotification == null)
            {
                cacheNotification = new UserBasesPermission { base_id = baseId, user_id = userId, allow_configure = true, ReportNotification = new EntityCollection<ReportNotification>() };
                dataContext.UserBasesPermission.AddObject(cacheNotification);
            }
            // добавляем новое оповещение
            if (notify.NotificationId == 0)
            {
                if (notify.OnMail || notify.OnPhone)
                    dataContext.ReportNotification.AddObject(new ReportNotification { UserBasesPermission = cacheNotification, on_mail = notify.OnMail, on_phone = notify.OnPhone, component_status_id = notify.ComponentStatusId });
                return;
            }
            // поиск в кеше
            ReportNotification reportNotification = cacheNotification.ReportNotification.Where(rn => rn.notification_id == notify.NotificationId).SingleOrDefault();
            if (reportNotification == null)
                return;
            else if (!notify.OnPhone && !notify.OnMail)
                dataContext.ReportNotification.DeleteObject(reportNotification);
            else
            {
                reportNotification.on_mail = notify.OnMail;
                reportNotification.on_phone = notify.OnPhone;
            }
        }

        public IEnumerable<ServiceViewModel> GetServices()
        {
            return dataContext.Service.OrderBy(s => s.service_name).Select(s => new ServiceViewModel { Address = s.address, ServiceId = s.service_id, Name = s.service_name, Path1C = s.C1c_path }).ToList();
        }

        public IEnumerable<UserBasesPermission> GetBasePermissions(int baseId)
        {
            return dataContext.UserBasesPermission.Include(ubp => ubp.User).Where(p => p.base_id == baseId && p.user_id!=1).Select(p => p).ToList();
        }
    }

    public class ComponentReportStatusView
    {
        public int ComponentReportStatusId { get; set; }
        public ReportStatus ReportStatus { get; set; }
        public Component Component { get; set; }
        public ReportNotification Notification { get; set; }
    }
}