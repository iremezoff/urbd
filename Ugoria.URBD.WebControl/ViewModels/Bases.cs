using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.WebControl.Models;

namespace Ugoria.URBD.WebControl.ViewModels
{
    public class ServiceViewModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Path1C { get; set; }
    }

    public class ServiceLogViewModel
    {
        public int LogId { get; set; }
        public int ServiceId { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public String Text { get; set; }
    }

    public class BaseViewModel
    {
        public int BaseId { get; set; }
        public int ServiceId { get; set; }
        public int GroupId { get; set; }
        public string ServiceAddress { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SqlDatabase { get; set; }
        public IEnumerable<PacketViewModel> PacketList { get; set; }
        public IEnumerable<ScheduleExchangeViewModel> ScheduleExchangeList { get; set; }
        public IEnumerable<ScheduleExtDirectoriesViewModel> ScheduleExtDirectoriesList { get; set; }
        public IEnumerable<ScheduleMlgCollectViewModel> ScheduleMlgCollectList { get; set; }
        public IEnumerable<ExtDirectoryViewModel> ExtDirectoriesList { get; set; }
    }

    public class UserPermissionViewModel
    {
        public int PermissionId { get; set; }
        public int BaseId { get; set; }
        public int UserId { get; set; }
        public bool AllowConfigure { get; set; }
    }

    public class ExtDirectoryViewModel : IExtDirectory
    {
        public int DirId { get; set; }
        public string LocalPath { get; set; }
        public string FtpPath { get; set; }
    }

    public class PacketViewModel
    {
        public int PacketId { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
    }

    public class ScheduleExchangeViewModel
    {
        public int ScheduleId { get; set; }
        public string Mode { get; set; }
        public string Time { get; set; }
    }

    public class ScheduleExtDirectoriesViewModel
    {
        public int ScheduleId { get; set; }
        public string Time { get; set; }
    }

    public class ScheduleMlgCollectViewModel
    {
        public int ScheduleId { get; set; }
        public string Time { get; set; }
    }

    public class ReportViewModel
    {
        public int ReportId { get; set; }
        public DateTime? CommandDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public IEnumerable<ReportPacketViewModel> PacketList { get; set; }
        public IEnumerable<LaunchViewModel> LaunchList { get; set; }
        public IEnumerable<ExtDirectoriesFileViewModel> Files { get; set; }
        public DateTime? DateMlgDate { get; set; }
    }

    public class ExchangeReportView
    {
        public int ReportId { get; set; }
        public DateTime? CommandDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public IEnumerable<ReportPacket> PacketList { get; set; }
        public IEnumerable<Launch1C> LaunchList { get; set; }
    }

    public class ExtDirectoryReportView
    {
        public int ReportId { get; set; }
        public DateTime? CommandDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public IEnumerable<Launch1C> LaunchList { get; set; }
        public IEnumerable<ExtDirectoryFile> Files { get; set; }
    }

    public class MlgCollectReportView
    {
        public int ReportId { get; set; }
        public DateTime? CommandDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public IEnumerable<Launch1C> LaunchList { get; set; }
        public DateTime? DateMlgDate { get; set; }
    }

    public class BaseReportViewModel
    {
        public int baseId;
        public int serviceId;
        public string component;
        public string status;
    }
    

    public class ExtDirectoriesFileViewModel
    {
        public int FileId { get; set; }
        public string Filename { get; set; }
        public DateTime? DateCopied { get; set; }
        public long? Size { get; set; }
    }

    public class ReportPacketViewModel
    {
        public int ReportPacketId { get; set; }
        public PacketViewModel Packet { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? Size { get; set; }
    }

    public class ReportLogViewModel
    {

    }

    public class LaunchViewModel
    {
        public int LaunchId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? Pid { get; set; }
    }

    public class BaseTreeSchedule
    {
        public int BaseId { get; set; }
        public int CurrentId { get; set; }
        public string BaseName { get; set; }
        public string Time { get; set; }
    }

    public class ReportNotificationViewModel
    {
        public int NotificationId { get; set; }
        public int ComponentStatusId { get; set; }
        public bool OnMail { get; set; }
        public bool OnPhone { get; set; }
    }
}