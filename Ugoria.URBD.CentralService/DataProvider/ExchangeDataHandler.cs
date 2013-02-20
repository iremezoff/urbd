using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Ugoria.URBD.CentralService.DataProvider;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared.DataProvider;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public class ExchangeDataHandler : DataHandler, IExchangeDataHandler
    {
        private static readonly string COMPONENT_NAME = "Exchange";

        public override ExecuteCommand GetPreparedCommand(ExecuteCommand command)
        {
            return GetPreparedExchangeCommand((ExchangeCommand)command);
        }

        public ExchangeCommand GetPreparedExchangeCommand(ExchangeCommand command)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                DataTable dataTable = dataProvider.GetPreparedCommand(command.baseId, COMPONENT_NAME);

                if (dataTable == null)
                    return null;

                ExchangeCommand preparedCommand = null;
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    if (preparedCommand == null)
                    {
                        preparedCommand = new ExchangeCommand
                        {
                            baseId = command.baseId,
                            modeType = command.modeType,
                            commandDate = dataRow["date_complete"] == DBNull.Value ? DateTime.MinValue : DateTime.Now, // не закончена предыдущая операция, поэтому новая команда отправлена быть не может
                            reportGuid = dataRow["date_complete"] == DBNull.Value ? (Guid)dataRow["guid"] : Guid.NewGuid(), // не закончена предыдущая команда, значит пресваиваем guid этой команды, иначе новый
                            baseName = (string)dataRow["base_name"],
                            releaseUpdate = dataRow["date_release"] != DBNull.Value ? (DateTime)dataRow["date_release"] : DateTime.MinValue,
                            configurationChangeDate = dataRow["date_change"] != DBNull.Value ? (DateTime)dataRow["date_change"] : DateTime.MinValue,
                            isReleaseUpdated = (bool)dataRow["is_updated"],
                            pools = new List<int>()
                        };
                    }
                    // если список принадлежности к пулам уже имелся
                    if (command.pools != null && command.pools.Count != 0)
                    {
                        preparedCommand.pools = new List<int>(command.pools);
                        break;
                    }
                    // добавляем принадлежность к пулу
                    preparedCommand.pools.Add((int)dataRow["pool_id"]);
                }
                return preparedCommand;
            }
        }

        public virtual ReportStatus SetReport(ExchangeReport report)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                dataProvider.SetReport(report.reportGuid, report.dateComplete, report.status.ToString(), report.message, report.mdRelease, report.dateRelease);
                // обрабокта сообщений лога работы 1С на стороне удаленного сервиса
                foreach (MLGMessage message in report.messageList)
                {
                    dataProvider.SetReportLog(report.reportGuid, message.eventDate, message.eventType, message.information);
                }

                // информация о пакетах
                if (report.packetList.Count > 0)
                {
                    foreach (ReportPacket packet in report.packetList)
                    {
                        dataProvider.SetReportPacket(report.reportGuid, packet.filename, packet.datePacket, (char)packet.type, packet.fileSize);
                    }
                }
            }
            switch (report.status)
            {
                case ExchangeReportStatus.Interrupt:
                case ExchangeReportStatus.Warning: return ReportStatus.Warning;
                case ExchangeReportStatus.Success: return ReportStatus.Information;
                case ExchangeReportStatus.Fail: return ReportStatus.Fail;
                case ExchangeReportStatus.Critical: return ReportStatus.Critical;
            }
            return ReportStatus.Fail;
        }

        public override ReportStatus SetReport(OperationReport report)
        {
            return SetReport((ExchangeReport)report);
        }

        public override LaunchReport GetLaunchReport(ExecuteCommand command)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                DataRow dataRow = dataProvider.GetLaunchReport(command.baseId, COMPONENT_NAME);
                if (dataRow == null)
                    return null;
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
        }

        public override ExecuteCommand SetCommandReport(ExecuteCommand command, int userId)
        {
            return base.SetCommandReport(command, COMPONENT_NAME, userId);
        }

        public override ExecuteCommand SetCommandReport(ExecuteCommand command)
        {
            return SetCommandReport(command, 1);
        }
    }
}
