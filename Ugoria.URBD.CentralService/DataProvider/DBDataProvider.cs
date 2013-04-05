using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data.Commands;
using System.Collections;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public class DBDataProvider : IDisposable
    {
        private SqlConnection conn;

        public SqlConnection Connection
        {
            get { return conn; }
        }

        public DBDataProvider()
        {
            conn = DB.Instance.Connection;
            //conn.Open();
        }

        public DBDataProvider(bool isOzekiSending)
        {
            SqlConnection conn = DB.Instance.OzekiConnection;
            //conn.Open();
        }

        private bool disposed = false;

        ~DBDataProvider()
        {
            if (!disposed)
                Dispose();
        }

        public DataSet GetLaunchReport(int baseId, string componentName)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectLastLaunchReport]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            cmd.Parameters.AddWithValue("@component", componentName);
            //DataRow dataRow = null;
            return ExecuteDataSqlCommand(cmd);
            /*if (dataSet.Tables[0].Rows.Count != 0)
                dataRow = dataSet.Tables[0].Rows[0];
            return dataRow;*/
        }

        public DataSet GetPreparedCommand(int baseId, string componentName)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectPreparedCommand]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            cmd.Parameters.AddWithValue("@component", componentName);
            return ExecuteDataSqlCommand(cmd);
            //return dataSet;
        }

        public DataRow GetBase(int baseId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectBase]";
            cmd.Parameters.AddWithValue("@base_id", baseId == 0 ? null : (object)baseId);
            cmd.Parameters.AddWithValue("@is_get_schedule", false);
            DataRow dataRow = null;
            DataSet dataSet = ExecuteDataSqlCommand(cmd);
            if (dataSet.Tables[0].Rows.Count != 0)
                dataRow = dataSet.Tables[0].Rows[0];
            return dataRow;
        }

        public DataTable GetSettings(string key = null)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_Settings]";
            cmd.Parameters.AddWithValue("@key", key);
            return ExecuteDataSqlCommand(cmd).Tables[0];
        }

        public DataTable GetServiceBases(int serviceId)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectBases]";
            cmd.Parameters.AddWithValue("@service_id", serviceId);
            DataSet dataSet = ExecuteDataSqlCommand(cmd);
            return dataSet.Tables[0];
        }

        public DataSet GetServiceSettings(string address)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_ServiceSettings]";
            cmd.Parameters.AddWithValue("@address", address);

            DataSet data = ExecuteDataSqlCommand(cmd);
            data.Tables[0].TableName = "Base";
            data.Tables[1].TableName = "Packet";
            data.Tables[2].TableName = "ExtDirectoryBase";
            data.Relations.Add(data.Tables["Base"].Columns["base_id"], data.Tables["Packet"].Columns["base_id"]).RelationName = "BasePacketRelation";
            data.Relations.Add(data.Tables["Base"].Columns["base_id"], data.Tables["ExtDirectoryBase"].Columns["base_id"]).RelationName = "ExtDirectoriesBaseRelation";
            return data;
        }

        public DataSet GetScheduleData(int baseId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectBase]";
            cmd.Parameters.AddWithValue("@base_id", baseId == 0 ? null : (object)baseId);
            cmd.Parameters.AddWithValue("@is_get_schedule", true);
            DataSet baseData = ExecuteDataSqlCommand(cmd);
            baseData.Tables[0].TableName = "Base";
            baseData.Tables[1].TableName = "ScheduleExtDirectories";
            baseData.Tables[2].TableName = "ScheduleExchange";
            baseData.Tables[3].TableName = "ScheduleMlgCollect";

            baseData.Relations.Add(baseData.Tables["Base"].Columns["base_id"], baseData.Tables["ScheduleExchange"].Columns["base_id"]).RelationName = "BaseScheduleExchange";
            baseData.Relations.Add(baseData.Tables["Base"].Columns["base_id"], baseData.Tables["ScheduleExtDirectories"].Columns["base_id"]).RelationName = "BaseScheduleExtDirectories";
            baseData.Relations.Add(baseData.Tables["Base"].Columns["base_id"], baseData.Tables["ScheduleMlgCollect"].Columns["base_id"]).RelationName = "BaseScheduleMlgCollect";
            //scheduleCache = baseData;
            return baseData;
        }

        public DataTable GetScheduleExchangeData(int baseId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectScheduleExchange]";
            cmd.Parameters.AddWithValue("@base_id", baseId == 0 ? null : (object)baseId);

            DataTable schExchData = ExecuteDataSqlCommand(cmd).Tables[0];
            schExchData.TableName = "ScheduleExchange";
            return schExchData;
        }

        public DataTable GetScheduleExtDirectories(int baseId = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "[urbd_SelectScheduleExtDirectories]";
            cmd.Parameters.AddWithValue("@base_id", baseId);

            DataTable schEFData = ExecuteDataSqlCommand(cmd).Tables[0];
            schEFData.TableName = "ScheduleExtDirectories";
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

        public DataTable GetAlarmList(Guid reportGuid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SelectAlarmReceive]";
            cmd.Parameters.AddWithValue("@report_guid", reportGuid);

            DataTable reportData = ExecuteDataSqlCommand(cmd).Tables[0];
            reportData.TableName = "AlarmReceivers";
            return reportData;
        }

        public DataSet ExecuteDataSqlCommand(SqlCommand command)
        {
            DataSet dataSet = new DataSet();
            try
            {
                command.Connection = conn;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataSet);
                if (conn.State == ConnectionState.Open)
                    conn.Close();
                return dataSet;
            }
            catch (SqlException)
            {
                throw;
            }
        }

        public void ExecuteVoidSqlCommand(SqlCommand command)
        {
            try
            {
                command.Connection = conn;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                command.ExecuteNonQuery();
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }
            catch (SqlException)
            {
                throw;
            }
        }

        public void SetPID1C(Guid reportGuid, Guid launchGuid, DateTime startDate, int pid)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetPID1C]";
            cmd.Parameters.AddWithValue("@report_guid", reportGuid);
            cmd.Parameters.AddWithValue("@launch_guid", launchGuid);
            cmd.Parameters.AddWithValue("@date_start", startDate != DateTime.MinValue ? (object)startDate : DBNull.Value);
            cmd.Parameters.AddWithValue("@pid", pid != 0 ? (object)pid : DBNull.Value);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetReport(Guid guid, DateTime completeDate, string status, string message)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReport]";
            cmd.Parameters.AddWithValue("@report_guid", guid);
            cmd.Parameters.AddWithValue("@date_complete", completeDate != DateTime.MinValue ? (object)completeDate : DBNull.Value);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@message", message);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetReportParam(Guid guid, Hashtable hashtable)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReportParam]";
            cmd.Parameters.AddWithValue("@report_guid", guid);
            foreach (DictionaryEntry entry in hashtable)
            {
                cmd.Parameters.AddWithValue("@" + entry.Key, entry.Value);
            }
            ExecuteVoidSqlCommand(cmd);
        }

        public void SetCommandReport(int userId, Guid guid, int baseId, DateTime commandDate, string componentName)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetCommandReport]";
            cmd.Parameters.AddWithValue("@base_id", baseId);
            cmd.Parameters.AddWithValue("@date_command", commandDate);
            cmd.Parameters.AddWithValue("@report_guid", guid);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@component", componentName);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetReportLog(Guid guid, DateTime messageDate, string eventType, string account, string mode1c, string information, string objectTypeCode = "", string objectTypeNumber = "", string baseCode = "", string objectIdentifier = "", string additional = "")
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReportLog]";
            cmd.Parameters.AddWithValue("@report_guid", guid);
            cmd.Parameters.AddWithValue("@date_message", messageDate);
            cmd.Parameters.AddWithValue("@event_type", eventType);
            cmd.Parameters.AddWithValue("@account", account);
            cmd.Parameters.AddWithValue("@mode1c", mode1c);
            cmd.Parameters.AddWithValue("@object_type_code", objectTypeCode);
            cmd.Parameters.AddWithValue("@object_type_number", objectTypeNumber);
            cmd.Parameters.AddWithValue("@base_code", baseCode);
            cmd.Parameters.AddWithValue("@object_identifier", objectIdentifier);
            cmd.Parameters.AddWithValue("@information", information);
            cmd.Parameters.AddWithValue("@additional", additional);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetReportFile(Guid guid, string filename, DateTime createdDate, long size)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReportFile]";
            cmd.Parameters.AddWithValue("@guid", guid);
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@date_file", createdDate);
            cmd.Parameters.AddWithValue("@filesize", size);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetReportPacket(Guid guid, string filename, DateTime packetDate, char type, long filesize)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetReportPacket]";
            cmd.Parameters.AddWithValue("@guid", guid);
            cmd.Parameters.AddWithValue("@filename", filename);
            cmd.Parameters.AddWithValue("@type", type);
            cmd.Parameters.AddWithValue("@date_file", packetDate);
            cmd.Parameters.AddWithValue("@filesize", filesize);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetLog(string address, DateTime messageDate, char status, string message)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[urbd_SetServiceLog]";
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@date", messageDate);
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@message", message);

            ExecuteVoidSqlCommand(cmd);
        }

        public void SetPhoneMessage(List<string> phones, string message)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.AddWithValue("@msg", message);
            StringBuilder strBuilder = new StringBuilder("insert into ozekimessageout(receiver, msg, status) ");
            IEnumerator<string> enumerator = phones.GetEnumerator();
            int i = 0;
            foreach (string phone in phones)
            {
                if (i > 0) strBuilder.Append(",");
                strBuilder.AppendFormat(" values(@phone{0}, @msg, 'send')", i);
                cmd.Parameters.AddWithValue("@phone" + i, phones[i++]);
            }
            ExecuteVoidSqlCommand(cmd);
        }

        public void Dispose()
        {
            /*if (!disposed && conn != null && conn.State != ConnectionState.Closed)
                conn.Close();*/
            disposed = true;
        }
    }
}