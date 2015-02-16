using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.CentralService.DataProvider;
using System.Data;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Services;
using System.Transactions;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public abstract class DataHandler : IDataHandler
    {
        protected DBDataProvider dataProvider = new DBDataProvider();
        protected DataSet cache;
        protected readonly string COMPONENT_NAME;

        protected DataHandler(string componentName)
        {
            COMPONENT_NAME = componentName;
        }

        public abstract DataHandler Clone();

        public abstract Type ReportType { get; }

        public virtual LaunchReport GetLaunchReport(ExecuteCommand command)
        {
            cache = dataProvider.GetLaunchReport(command.baseId, COMPONENT_NAME);
            if (cache == null || cache.Tables.Count == 0 || cache.Tables[0].Rows.Count == 0)
                return null;
            DataRow dataRow = cache.Tables[0].Rows[0];
            return new LaunchReport
            {
                baseId = (int)dataRow["base_id"],
                baseName = (string)dataRow["base_name"],
                commandDate = dataRow["date_command"] != DBNull.Value ? (DateTime)dataRow["date_command"] : DateTime.MinValue,
                startDate = dataRow["date_start"] != DBNull.Value ? (DateTime)dataRow["date_start"] : DateTime.MinValue,
                launchGuid = dataRow["launch_guid"] != DBNull.Value ? (Guid)dataRow["launch_guid"] : Guid.Empty,
                reportGuid = dataRow["report_guid"] != DBNull.Value ? (Guid)dataRow["report_guid"] : Guid.Empty,
                pid = dataRow["pid"] != DBNull.Value ? (int)dataRow["pid"] : 0,
            };
        }

        public virtual ExecuteCommand GetPreparedCommand(ExecuteCommand command)
        {
            cache = dataProvider.GetPreparedCommand(command.baseId, COMPONENT_NAME);
            if (cache.Tables.Count == 0 || cache.Tables[0].Rows.Count == 0)
                return null;
            bool isFullRequire = true;
            foreach (DataRow dataRow in cache.Tables[0].Rows)
            {
                if (isFullRequire)
                {
                    command.baseId = command.baseId;
                    command.commandDate = dataRow["date_complete"] == DBNull.Value ? DateTime.MinValue : DateTime.Now; // не закончена предыдущая операция, поэтому новая команда отправлена быть не может
                    command.reportGuid = dataRow["date_complete"] == DBNull.Value ? (Guid)dataRow["guid"] : Guid.NewGuid(); // не закончена предыдущая команда, значит пресваиваем guid этой команды, иначе новый
                    command.baseName = (string)dataRow["base_name"];
                    command.configurationChangeDate = dataRow["date_change"] != DBNull.Value ? (DateTime)dataRow["date_change"] : DateTime.MinValue;
                    command.pools = new List<int>();
                    isFullRequire = false;
                }
                // добавляем принадлежность к пулу
                command.pools.Add((int)dataRow["pool_id"]);
            }
            return command;
        }

        public virtual void SetReport(OperationReport report)
        {
            dataProvider.SetReport(report.reportGuid, report.dateComplete, report.status.ToString(), report.message);
        }

        public void SetLaunchReport(LaunchReport launchReport)
        {
            dataProvider.SetPID1C(launchReport.reportGuid, launchReport.launchGuid, launchReport.startDate, launchReport.pid);
        }

        public virtual void SetCommandReport(ExecuteCommand command)
        {
            this.SetCommandReport(command, 1);
        }

        public virtual void SetCommandReport(ExecuteCommand command, int userId)
        {
            dataProvider.SetCommandReport(userId, command.reportGuid, command.baseId, command.commandDate, COMPONENT_NAME);
        }
    }
}
