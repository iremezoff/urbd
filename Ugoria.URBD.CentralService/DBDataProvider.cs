using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;
using Ugoria.URBD.Core;

namespace Ugoria.URBD.CentralService
{
    class DBDataProvider : IDataProvider
    {
        private DB db = DB.Instance;
        private ConcurrentDictionary<string, SqlTransaction> transactions = new ConcurrentDictionary<string, SqlTransaction>();
        private object objLock = new object();

        public DBDataProvider() { }

        public DataRow GetLastCommand(int baseId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectLastCommand]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            DataRow dataRow = null;
            DataSet dataSet = ExecuteDataSqlCommand(cmd);
            if (dataSet.Tables[0].Rows.Count != 0)
                dataRow = dataSet.Tables[0].Rows[0];
            return dataRow;
        }

        public DataRow GetBase(int baseId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectBase]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            cmd.Parameters.AddWithValue("@is_get_schedule", false);
            DataRow dataRow = null;
            DataSet dataSet = ExecuteDataSqlCommand(cmd);
            if (dataSet.Tables[0].Rows.Count != 0)
                dataRow = dataSet.Tables[0].Rows[0];
            return dataRow;
        }

        public DataTable GetSettings(string key)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_Settings]";
            cmd.Parameters.AddWithValue("@key", key);
            return ExecuteDataSqlCommand(cmd).Tables[0];
        }

        public DataSet GetServiceSettings(string address)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_ServiceSettings]";
            cmd.Parameters.AddWithValue("@address", address);

            DataSet packetData = ExecuteDataSqlCommand(cmd);
            packetData.Tables[0].TableName = "Base";
            packetData.Tables[1].TableName = "Packet";
            packetData.Relations.Add(packetData.Tables["Base"].Columns["base_id"], packetData.Tables["Packet"].Columns["base_id"]).RelationName = "BasePacket";
            return packetData;
        }

        public DataSet GetScheduleData(string baseId = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectBase]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            cmd.Parameters.AddWithValue("@is_get_schedule", true);
            DataSet baseData = ExecuteDataSqlCommand(cmd);
            baseData.Tables[0].TableName = "Base";
            baseData.Tables[1].TableName = "ScheduleExtForms";
            baseData.Tables[2].TableName = "ScheduleExchange";

            baseData.Relations.Add(baseData.Tables["Base"].Columns["base_id"], baseData.Tables["ScheduleExchange"].Columns["base_id"]).RelationName = "BaseScheduleExchange";
            baseData.Relations.Add(baseData.Tables["Base"].Columns["base_id"], baseData.Tables["ScheduleExtForms"].Columns["base_id"]).RelationName = "BaseScheduleExtForms";
            return baseData;
        }

        public DataTable GetScheduleExchangeData(string baseName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectScheduleExchange]";
            cmd.Parameters.AddWithValue("@base_name", baseName);

            DataTable schExchData = ExecuteDataSqlCommand(cmd).Tables[0];
            schExchData.TableName = "ScheduleExchange";
            return schExchData;
        }

        public DataTable GetScheduleExtForms(string baseName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "[urbd_SelectScheduleExtForms]";
            cmd.Parameters.AddWithValue("@base_name", baseName);

            DataTable schEFData = ExecuteDataSqlCommand(cmd).Tables[0];
            schEFData.TableName = "ScheduleExtForms";
            return schEFData;
        }

        public DataTable GetReport(Guid reportGuid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectReport]";
            cmd.Parameters.AddWithValue("@report_guid", reportGuid);

            DataTable reportData = ExecuteDataSqlCommand(cmd).Tables[0];
            reportData.TableName = "Report";
            return reportData;
        }

        public DataTable GetAlarmList(string address)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectAlarmReceive]";
            cmd.Parameters.AddWithValue("@address", address);

            DataTable reportData = ExecuteDataSqlCommand(cmd).Tables[0];
            reportData.TableName = "AlarmReceiver";
            return reportData;
        }

        public DataSet ExecuteDataSqlCommand(SqlCommand command, string transactionName = null)
        {
            DataSet dataSet = new DataSet();
            try
            {
                SqlConnection conn = null;
                if (transactionName != null)
                {
                    SqlTransaction trans = transactions[transactionName];
                    command.Transaction = trans;
                    conn = trans.Connection;
                }
                else
                {
                    conn = db.Connection;
                    conn.Open();
                }
                command.Connection = conn;

                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataSet);

                if (transactionName == null)
                    conn.Close();

                return dataSet;
            }
            catch (SqlException ex)
            {
                throw;
            }
        }

        public void ExecuteVoidSqlCommand(SqlCommand command, string transactionName = null)
        {
            try
            {
                SqlConnection conn = null;
                if (transactionName != null)
                {
                    SqlTransaction trans = transactions[transactionName];
                    command.Transaction = trans;
                    conn = trans.Connection;
                }
                else
                {
                    conn = db.Connection;
                    conn.Open();
                }
                command.Connection = conn;
                command.ExecuteNonQuery();

                if (transactionName == null)
                    conn.Close();
            }
            catch (SqlException)
            {
                throw;
            }
        }

        public void SetPID1C(Guid reportGuid, Guid launchGuid, DateTime startDate, int pid, string transactionName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetPID1C]";
            cmd.Parameters.AddWithValue("@report_guid", reportGuid);
            cmd.Parameters.AddWithValue("@launch_guid", launchGuid);
            cmd.Parameters.AddWithValue("@date_start", startDate != DateTime.MinValue ? (object)startDate : DBNull.Value);
            cmd.Parameters.AddWithValue("@pid", pid != 0 ? (object)pid : DBNull.Value);

            ExecuteVoidSqlCommand(cmd, transactionName);
        }

        public void SetReport(Guid guid, DateTime completeDate, string status, string message, string mdRelease, DateTime releaseDate, string transactionName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReport]";
            cmd.Parameters.AddWithValue("@report_guid", guid);
            cmd.Parameters.AddWithValue("@date_complete", completeDate != DateTime.MinValue ? (object)completeDate : DBNull.Value);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@message", message);
            cmd.Parameters.AddWithValue("@md_release", mdRelease);
            cmd.Parameters.AddWithValue("@date_release", releaseDate != DateTime.MinValue ? (object)releaseDate : DBNull.Value);

            ExecuteVoidSqlCommand(cmd, transactionName);
        }

        public void SetCommandReport(int userId, Guid guid, int baseId, DateTime commandDate, string transactionName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetCommandReport]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            cmd.Parameters.AddWithValue("@date_command", commandDate);
            cmd.Parameters.AddWithValue("@report_guid", guid);
            cmd.Parameters.AddWithValue("@user_id", userId);

            ExecuteVoidSqlCommand(cmd, transactionName);
        }

        public void SetReportLog(Guid guid, DateTime messageDate, string type, string text, string transactionName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReportLog]";
            cmd.Parameters.AddWithValue("@report_guid", guid);
            cmd.Parameters.AddWithValue("@date_message", messageDate);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@text", text);

            ExecuteVoidSqlCommand(cmd, transactionName);
        }

        public void SetReportPacket(Guid guid, string filename, DateTime packetDate, char type, long filesize, string filehash, string transactionName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReportPacket]";
            cmd.Parameters.AddWithValue("@guid", guid);
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@date_file", packetDate);
            cmd.Parameters.AddWithValue("@filesize", filesize);
            cmd.Parameters.AddWithValue("@filehash", filehash);

            ExecuteVoidSqlCommand(cmd, transactionName);
        }

        public void SetLog(string address, DateTime messageDate, char status, string message, string transactionName = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetServiceLog]";
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@date", messageDate);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@message", message);

            ExecuteVoidSqlCommand(cmd, transactionName);
        }

        public void Commit(string transactionName)
        {
            SqlTransaction transaction = transactions[transactionName];
            SqlConnection conn = transaction.Connection;
            transaction.Commit();
            conn.Close();
            transactions.TryRemove(transactionName, out transaction);
        }

        public void Rollback(string transactionName)
        {
            SqlTransaction transaction = transactions[transactionName];
            SqlConnection conn = transaction.Connection;
            transaction.Rollback();
            conn.Close();
            transactions.TryRemove(transactionName, out transaction);
        }

        public void BeginTransaction(string transactionName)
        {
            SqlConnection conn = db.Connection;
            conn.Open();
            //  уровень изоляции "грязных чтений", для претовращения взаимоблокировок при записи отчетов одновременно от не нескольких сервисов
            SqlTransaction transaction = conn.BeginTransaction(IsolationLevel.ReadUncommitted, transactionName);
            transactions.TryAdd(transactionName, transaction);
        }
    }
}