using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using Microsoft.Win32;
using Ugoria.URBD.Contracts.Context;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.RemoteService.Strategy.Exchange.Mode;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Handlers.Strategy.Exchange.Mode;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.RemoteService.Kit;
using System.Net;
using System.Collections;
using System.Diagnostics;

namespace Ugoria.URBD.RemoteService.Strategy.ExtDirectory
{
    public class ExtDirectoriesStrategyBuilder : IExtDirectoryStrategyBuilder
    {
        public ICommandStrategy Build(IContext context)
        {
            ExtDirectoriesContext extFormsContext = null;
            ExtDirectoriesCommand command = (ExtDirectoriesCommand)context.Command;
            if (context.Configuration == null)
                return null;
            extFormsContext = new ExtDirectoriesContext
            {
                BasePath = (string)context.Configuration.GetParameter("base.1c_database"),
                Command = command,
                Username = (string)context.Configuration.GetParameter("main.username"),
                Password = (string)context.Configuration.GetParameter("main.password"),
                FtpAddress = (string)context.Configuration.GetParameter("main.ftp_address"),
                FtpAttemptCount = int.Parse((string)context.Configuration.GetParameter("main.ftp_attempt_count")),
                FtpDelayTime = int.Parse((string)context.Configuration.GetParameter("main.ftp_attempt_delay")),
                Files = new List<ExtDirectoriesFile>(),
                Directories = new Dictionary<string, string>((Dictionary<string, string>)context.Configuration.GetParameter("base.extdir_table"))
            };

            ICommandStrategy commandStrategy = new ExtDirectoriesStrategy(extFormsContext);
            return commandStrategy;
        }

        public IDictionary<string, string> ValidateConfiguration(RemoteConfiguration configuration)
        {
            return null;
        }

        public void PrepareSystem(RemoteConfiguration configuration)
        {
            LogHelper.Write2Log("Проверка наличия расширенных директорий", LogLevel.Information);

            NetworkConnection netConn = null;
            foreach (KeyValuePair<int, Hashtable> basePair in configuration.bases)
            {
                string basePath = (string)basePair.Value["base.1c_database"];
                string baseName = (string)basePair.Value["base.base_name"];
                string username = (string)basePair.Value["base.1c_username"];
                string password = (string)basePair.Value["base.1c_password"];
                Uri basePathUri = new Uri(basePath);
                try
                {
                    if (basePathUri.IsUnc)
                        netConn = new NetworkConnection(basePathUri.OriginalString, new NetworkCredential(username, password));
                    foreach (KeyValuePair<string, string> dirPair in (Dictionary<string, string>)basePair.Value["base.extdir_table"])
                    {
                        DirectoryInfo extFormsDirInfo = new DirectoryInfo(string.Format(@"{0}\{1}", basePath, dirPair.Value));
                        if (!extFormsDirInfo.Exists)
                        {
                            extFormsDirInfo.Create();
                            LogHelper.Write2Log(string.Format("Создана директория {0} ИБ {1}", dirPair.Value, baseName), LogLevel.Information);
                        }
                    }
                }
                finally
                {
                    if (netConn != null)
                        netConn.Dispose();
                }
            }
        }

        public LaunchReport GetLaunchReport(ExtDirectoriesStrategy strategy)
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

        public ExtDirectoriesReport GetOperationReport(ExtDirectoriesStrategy strategy)
        {
            ExtDirectoriesReport report = new ExtDirectoriesReport
            {
                baseId = strategy.Context.Command.baseId,
                baseName = strategy.Context.Command.baseName,
                commandDate = strategy.Context.Command.commandDate,
                dateComplete = DateTime.Now,
                reportGuid = strategy.Context.Command.reportGuid,
                files = new List<ExtDirectoriesFile>(),
                status = ExtDirectoriesReportStatus.Unknown
            };

            if (strategy.IsInterrupt)
            {
                report.status = ExtDirectoriesReportStatus.Interrupt;
                report.message = "Процесс прерван вручную. Добавлено файлов: " + strategy.Context.Files.Count(f => f.fileSize > 0 && f.createdDate != DateTime.MinValue);
            }
            else
            {
                List<ExtDirectoriesFile> notCopiedFiles = new List<ExtDirectoriesFile>();
                foreach (ExtDirectoriesFile efFile in strategy.Context.Files)
                {
                    if (efFile.fileSize == 0 && efFile.createdDate == DateTime.MinValue)
                        notCopiedFiles.Add(efFile);
                    else
                        report.files.Add(efFile);
                }
                if (!strategy.IsComplete)
                {
                    report.status = ExtDirectoriesReportStatus.Fail;
                }
                else if (notCopiedFiles.Count > 0)
                {
                    report.status = ExtDirectoriesReportStatus.Warning;
                    report.message = String.Format("Добавлено файлов: {0}. Не удалось скопировать следующие файлы:\r\n{1}", report.files.Count, string.Join(",\r\n", notCopiedFiles.Select(f => f.fileName)));
                }
                else if (report.files.Count > 0)
                {
                    report.status = ExtDirectoriesReportStatus.Success;
                    report.message = "Обновление расширенных директорий прошло успешно";
                }
                else
                {
                    report.status = ExtDirectoriesReportStatus.Success;
                    report.message = "Обновления отсутствуют";
                }
            }
            return report;
        }

        LaunchReport IStrategyBuilder.GetLaunchReport(ICommandStrategy strategy)
        {
            return GetLaunchReport((ExtDirectoriesStrategy)strategy);
        }

        OperationReport IStrategyBuilder.GetOperationReport(ICommandStrategy strategy)
        {
            return GetOperationReport((ExtDirectoriesStrategy)strategy);
        }
    }
}
