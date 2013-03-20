using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Context;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.RemoteService.Strategy.Builders
{
    public class MlgCollectStrategyBuilder:IMlgCollectStrategyBuilder
    {
        public ICommandStrategy Build(IContext context)
        {
            MlgCollectContext collectContext = new MlgCollectContext();
            MlgCollectCommand command = (MlgCollectCommand)context.Command;
            if (context.Configuration == null)
                return null;
            collectContext.Command = command;
            collectContext.BasePath = (string)context.Configuration.GetParameter("base.1c_database");
            collectContext.Messages = new Stack<MlgMessage>();
            collectContext.Username = (string)context.Configuration.GetParameter("main.username");
            collectContext.Password = (string)context.Configuration.GetParameter("main.password");
            collectContext.PrevDateLog = command.prevDateLog;

            ICommandStrategy commandStrategy = new MlgCollectStrategy(collectContext);
            return commandStrategy;
        }

        public void PrepareSystem(RemoteConfiguration configuration)
        {
            return;
        }

        public IDictionary<string, string> ValidateConfiguration(RemoteConfiguration configuration)
        {
            return null;
        }

        public LaunchReport GetLaunchReport(MlgCollectStrategy strategy)
        {
            return new LaunchReport
            {
                baseId = strategy.Context.Command.baseId,
                baseName = strategy.Context.Command.baseName,
                commandDate = strategy.Context.Command.commandDate,
                launchGuid = strategy.Context.LaunchGuid,
                pid = Process.GetCurrentProcess().Id,
                reportGuid = strategy.Context.Command.reportGuid,
                startDate = strategy.Context.StartTime
            };
        }

        public LaunchReport GetLaunchReport(ICommandStrategy strategy)
        {
            return GetLaunchReport((MlgCollectStrategy)strategy);
        }

        public OperationReport GetOperationReport(MlgCollectStrategy strategy)
        {
            MlgCollectReport report = new MlgCollectReport
            {
                baseId = strategy.Context.Command.baseId,
                baseName = strategy.Context.Command.baseName,
                commandDate = strategy.Context.Command.commandDate,
                dateComplete = DateTime.Now,
                reportGuid = strategy.Context.Command.reportGuid,
                messageList = new List<MlgMessage>(),
            };

            report.messageList.AddRange(strategy.Context.Messages);
            if (strategy.IsInterrupt)
            {
                report.status = ReportStatus.Interrupt;
                report.message = "Процесс прерван вручную. Считано записей: " + strategy.Context.Messages.Count;
            }
            else
            {                
                if (!strategy.IsComplete)
                {
                    report.status = ReportStatus.Fail;
                }
                else if (report.messageList.Count > 0)
                {
                    report.status = ReportStatus.Success;
                    report.message = "Сбор данных файла логирования прошёл успешно. Считано записей: " + strategy.Context.Messages.Count;
                }
                else
                {
                    report.status = ReportStatus.Warning;
                    report.message = "Новые данные в файле логирования отсутствуют";
                }
            }
            BinaryFormatter bFormatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bFormatter.Serialize(ms, report);
                LogHelper.Write2Log(string.Format("Размер пакета для ИБ {0}: {1:0.00} KiB", report.baseName, ms.Length / 1024), LogLevel.Information);
            }
            return report;
        }

        public OperationReport GetOperationReport(ICommandStrategy strategy)
        {
            return GetOperationReport((MlgCollectStrategy)strategy);
        }
    }
}
