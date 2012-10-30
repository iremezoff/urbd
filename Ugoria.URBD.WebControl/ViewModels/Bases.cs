using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.WebControl.ViewModels
{
    public class ServiceViewModel
    {
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Path1C { get; set; }
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
        public IEnumerable<ScheduleExtFormsViewModel> ScheduleExtFormsList { get; set; }
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
    }

    public class ScheduleExtFormsViewModel
    {
        public int ScheduleId { get; set; }
    }

    public class ReportViewModel
    {
        public int ReportId { get; set; }
        public DateTime? CommandDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string User { get; set; }
        public IEnumerable<ReportPacketViewModel> PacketList { get; set;}
        public IEnumerable<LaunchViewModel> LaunchList { get; set;}
    }

    public class ReportPacketViewModel
    {
        public int ReportPacketId { get; set; }
        public PacketViewModel Packet { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? Size { get; set; }
    }

    public class LaunchViewModel
    {
        public int LaunchId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? Pid { get; set; }
    }
}