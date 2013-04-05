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
using System.Security.Principal;
using System.Collections;

namespace Ugoria.URBD.RemoteService.Strategy.Exchange
{
    public class ExchangeStrategyBuilder : IExchangeStrategyBuilder
    {
        public static readonly string[] CRITICAL_ERRORS = new string[] {
            "Загрузка измененной конфигурации не может быть произведена при доступе к Информационной Базе в разделенном режиме",
            "Настройки счетов были изменены. Для выполнения выгрузки данных итоги должны быть пересчитаны", 
            "Изменения конфигурации  не загружались в ИБ из которой прибыл файл переноса",
            "Ошибка загрузки конфигурации!"
        };
        public ICommandStrategy Build(IContext context)
        {
            ExchangeContext exchangeContext = null;
            ExchangeCommand command = (ExchangeCommand)context.Command;
            if (context.Configuration == null)
                return null;
            exchangeContext = new ExchangeContext();
            exchangeContext.Command = command;
            exchangeContext.DateRelease = command.releaseUpdate;
            exchangeContext.Path1C = (string)context.Configuration.GetParameter("service.1c_path");
            exchangeContext.BasePath = new FileInfo((string)context.Configuration.GetParameter("base.1c_database")).FullName;
            exchangeContext.Username = (string)context.Configuration.GetParameter("main.username");
            exchangeContext.Password = (string)context.Configuration.GetParameter("main.password");
            exchangeContext.User1C = (string)context.Configuration.GetParameter("base.1c_username");
            exchangeContext.Password1C = (string)context.Configuration.GetParameter("base.1c_password");
            exchangeContext.AttemptDelay = int.Parse((string)context.Configuration.GetParameter("exchange.attempt_delay"));
            exchangeContext.FtpAddress = (string)context.Configuration.GetParameter("main.ftp_address");
            exchangeContext.FtpCenterDir = (string)context.Configuration.GetParameter("exchange.ftp_center_dir");
            exchangeContext.FtpPeripheryDir = (string)context.Configuration.GetParameter("exchange.ftp_periphery_dir");
            exchangeContext.FtpAttemptCount = int.Parse((string)context.Configuration.GetParameter("main.ftp_attempt_count"));
            exchangeContext.FtpDelayTime = int.Parse((string)context.Configuration.GetParameter("main.ftp_attempt_delay"));
            exchangeContext.HasDeletePacket = bool.Parse((string)context.Configuration.GetParameter("exchange.delete_packet"));
            exchangeContext.Packets = ((List<Hashtable>)context.Configuration.GetParameter("base.packet_list")).Select<Hashtable, ReportPacket>(p => new ReportPacket { type = (PacketType)p["packet.type"], filename = (string)p["packet.filename"] }).ToList();
            exchangeContext.PrmFile = String.Format(@"{0}\PrmURBD\configuration.prm", (string)context.Configuration.GetParameter("base.1c_database"));
            exchangeContext.Mode = (DefaultMode)CreateMode(context);
            exchangeContext.Messages = new List<string>();

            ICommandStrategy commandStrategy = new ExchangeStrategy(exchangeContext);
            return commandStrategy;
        }

        public IMode CreateMode(IContext context)
        {
            string basePath = (string)context.Configuration.GetParameter("base.1c_database");
            int attemptDelay = int.Parse((string)context.Configuration.GetParameter("exchange.attempt_delay"));
            Verifier verifier = new Verifier(String.Format(@"{0}\PrmURBD\report.log", basePath));
            DefaultMode mode = null;
            switch (((ExchangeCommand)context.Command).modeType)
            {
                case ModeType.Passive: mode = new PassiveMode(verifier, basePath, attemptDelay);
                    break;
                case ModeType.Normal: mode = new NormalMode(verifier, basePath, attemptDelay);
                    break;
            }
            return mode;
        }

        public IDictionary<string, string> ValidateConfiguration(RemoteConfiguration configuration)
        {
            return null;
        }

        public void PrepareSystem(RemoteConfiguration configuration)
        {
            LogHelper.Write2Log("Обновление реестра и prm", LogLevel.Information);
            LogHelper.Write2Log("Путь до 1С: " + configuration.configuration["service.1c_path"], LogLevel.Information);
            NetworkConnection netConn = null;

            WindowsIdentity identity = new WindowsIdentity(SecureHelper.ConvertUserdomainToClassic((string)configuration.configuration["main.username"]));
            //RegistryKey userRegistryKey = Registry.Users.CreateSubKey(identity.User.Value);
            RegistryKey userRegistryKey = Registry.Users.CreateSubKey(WindowsIdentity.GetCurrent().User.Value);
            // Создание ветки 1С

            if (userRegistryKey.OpenSubKey(@"Software\1C\1Cv7\7.7\Titles") == null)
            {
                userRegistryKey.CreateSubKey(@"Software\1C\1Cv7\7.7\Titles");
            }
            userRegistryKey.CreateSubKey(@"Software\1C\1Cv7\7.7\Options\TIPOTHDAYGLB").SetValue("TipOfTheDayGlobal", "0", RegistryValueKind.String);
            RegistryKey titleKey = userRegistryKey.OpenSubKey(@"Software\1C\1Cv7\7.7\Titles", true);
            string[] titlesArr = titleKey.GetValueNames();

            foreach (KeyValuePair<int, Hashtable> base1C in configuration.bases)
            {
                Uri basePathUri = new Uri((string)base1C.Value["base.1c_database"]);
                try
                {
                    // Заведение ключа в реестре для предовтращения появления подсказок при запуске конфигуратора в пакетном режиме                    
                    if (!titlesArr.Any(k => basePathUri.OriginalString.Equals(k)))
                    {
                        titleKey.SetValue(basePathUri.OriginalString, (string)base1C.Value["base.base_name"]);
                        LogHelper.Write2Log(string.Format("Добавлена в реестр ИБ 1C: {0}", (string)base1C.Value["base.base_name"]), LogLevel.Information);
                    }

                    if (basePathUri.IsUnc)
                    {
                        //netConn = new NetworkConnection(basePathUri.OriginalString, new NetworkCredential((string)configuration.configuration["main.username"], (string)configuration.configuration["main.password"]));
                    }
                    string md5HashBaseName = ServiceUtil.GetMd5Hash((string)base1C.Value["base.base_name"]);
                    PrmBuilder prmBuilder = PrmBuilder.Create();
                    prmBuilder.FileName = String.Format(@"{0}\PrmURBD\configuration.prm", basePathUri.OriginalString);
                    prmBuilder.LogFile = String.Format(@"{0}\PrmURBD\report.log", basePathUri.OriginalString);
                    FileInfo prmFileInfo = prmBuilder.Build();

                    List<Hashtable> packetList = (List<Hashtable>)base1C.Value["base.packet_list"];
                    if (packetList != null)
                    {
                        foreach (Hashtable packet in packetList)
                        {
                            FileInfo packetFileInfo = new FileInfo(String.Format(@"{0}\{1}", basePathUri.OriginalString, packet["packet.filename"]));
                            if (!packetFileInfo.Directory.Exists)
                                packetFileInfo.Directory.Create();
                        }
                    }
                }
                finally
                {
                    //if (netConn != null)
                        //netConn.Dispose();
                }
            }
        }

        public ExchangeReport GetOperationReport(ExchangeStrategy strategy)
        {
            ExchangeReport report = new ExchangeReport
            {
                baseId = strategy.Context.Command.baseId,
                baseName = strategy.Context.Command.baseName,
                commandDate = strategy.Context.Command.commandDate,
                dateComplete = DateTime.Now,
                dateRelease = strategy.Context.DateRelease,
                mdRelease = strategy.Context.MDRelease,
                reportGuid = strategy.Context.Command.reportGuid
            };
            if (strategy.IsInterrupt)
            {
                report.status = ReportStatus.Interrupt;
                report.message = "Процесс был прерван вручную";
            }
            else if (!strategy.IsComplete)
            {
                // Критические проблемы
                if ((strategy.Context.Command.isReleaseUpdated && string.IsNullOrEmpty(strategy.Context.MDRelease))
                    || CRITICAL_ERRORS.Any(e => strategy.Context.Mode.Message.IndexOf(e) >= 0))
                    report.status = ReportStatus.Critical;
                else
                    report.status = ReportStatus.Fail;
                
                if(!strategy.Context.Mode.IsSuccess)
                    strategy.Context.Messages.Insert(0, strategy.Context.Mode.Message);
                report.message = string.Join(".\r\n", strategy.Context.Messages);
            }
            else
            {
                if (strategy.Context.Mode.IsWarning)
                    report.status = ReportStatus.Warning;
                else if (strategy.Context.Mode.IsSuccess)
                    report.status = ReportStatus.Success;
                else
                    report.status = ReportStatus.Fail;

                if (report.status!=ReportStatus.Success // добавляем ещё одну ошибку к существующим
                    || (strategy.Context.Messages.Count == 0 && report.status == ReportStatus.Success)) // всё успешно, добавляем хороший статус
                {
                    strategy.Context.Messages.Insert(0, strategy.Context.Mode.Message);
                }
                
                report.message = string.Join(".\r\n",strategy.Context.Mode.Message);
            }
            report.packetList.AddRange(strategy.Context.Packets.Where(p => p.fileSize > 0 && p.datePacket != DateTime.MinValue));
            return report;
        }

        public LaunchReport GetLaunchReport(ExchangeStrategy strategy)
        {
            return new LaunchReport
            {
                baseId = strategy.Context.Command.baseId,
                baseName = strategy.Context.Command.baseName,
                commandDate = strategy.Context.Command.commandDate,
                launchGuid = strategy.Context.LaunchGuid,
                pid = strategy.Context.Pid,
                reportGuid = strategy.Context.Command.reportGuid,
                startDate = strategy.Context.StartTime,
                componentName = "Exchange"
            };
        }

        public LaunchReport GetLaunchReport(ICommandStrategy strategy)
        {
            return GetLaunchReport((ExchangeStrategy)strategy);
        }

        public OperationReport GetOperationReport(ICommandStrategy strategy)
        {
            return GetOperationReport((ExchangeStrategy)strategy);
        }
    }
}
