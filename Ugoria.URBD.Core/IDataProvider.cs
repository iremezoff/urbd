using System;
using System.Data;
namespace Ugoria.URBD.Core
{
    public interface IDataProvider
    {
        //DataTable GetBaseData(int? idBase = null);
        DataSet GetScheduleData(string baseName = null);
        DataTable GetScheduleExchangeData(string baseName = null);
        DataTable GetScheduleExtForms(string baseName = null);
        DataTable GetSettings(string key = null);
        DataSet GetServiceSettings(string address);
        void SetPID1C(Guid reportGuid, Guid launchGuid, DateTime startDate, int pid, string transactionName = null);
        void SetReport(Guid guid, DateTime completeDate, string status, string message, string transactionName = null);
        void SetCommandReport(Guid guid, string baseName, DateTime commandDate, string transactionName = null);
        void SetReportLog(Guid guid, DateTime messageDate, string type, string text, string transactionName = null);
        void Commit(string transactionName);
        void Rollback(string transactionName);
        void BeginTransaction(string transactionName);
        void SetReportPacket(Guid guid, string filename, DateTime packetDate, long filesize, string filehash, string transactionName = null);
        void SetLog(string address, DateTime messageDate, char status, string message, string transactionName = null);
        DataTable GetReport(Guid reportGuid);
    }
}
