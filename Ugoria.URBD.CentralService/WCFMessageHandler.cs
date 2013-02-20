using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Shared.DataProvider;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Handlers;
using System.Reflection;
using Ugoria.URBD.Shared;
using Ugoria.URBD.CentralService.DataProvider;

namespace Ugoria.URBD.CentralService
{
    class WCFMessageHandler
    {
        private List<DataHandler> handlerStore = new List<DataHandler>();

        public WCFMessageHandler(List<DataHandler> handlers)
        {
            this.handlerStore.AddRange(handlers);
        }

        public ReportStatus HandleReport(Report report)
        {
            try
            {
                if (report is LaunchReport)
                {
                    DataHandler dataHandler = handlerStore.First();
                    return dataHandler.SetLaunchReport((LaunchReport)report);
                }
                else if (report is OperationReport)
                {
                    DataHandler dataHandler = GetHandler(report);
                    return dataHandler.SetReport((OperationReport)report);
                }
                return ReportStatus.Fail;
            }
            catch (InvalidOperationException ex)
            {
                LogHelper.Write2Log("Не найден обработчик сообщения типа " + report.GetType().FullName, LogLevel.Error);
                throw new URBDException("Не указан обработчик", ex);
            }
        }

        public Command SetCommandReport(ExecuteCommand command)
        {
            return SetCommandReport(command, 1);
        }

        public Command SetCommandReport(ExecuteCommand command, int userId)
        {
            DataHandler dataHandler = GetHandler(command.GetType());

            if (dataHandler == null)
                return null;

            return dataHandler.SetCommandReport(command, userId);
        }

        private DataHandler GetHandler(Report report)
        {
            Attribute attr = Attribute.GetCustomAttribute(report.GetType(), typeof(ReportHandlerAttribute), true);
            ReportHandlerAttribute reportAttr = (ReportHandlerAttribute)attr;
            return GetHandler(reportAttr.CommandType);
        }

        private DataHandler GetHandler(Type commandType)
        {
            Attribute attr = Attribute.GetCustomAttribute(commandType, typeof(CommandHandlerAttribute), true);
            CommandHandlerAttribute commandAttr = (CommandHandlerAttribute)attr;

            DataHandler dataHandler = null;
            dataHandler = handlerStore.First(h => commandAttr.HandlerType.IsAssignableFrom(h.GetType())); // <------ проверить
            return dataHandler;
        }

        public LaunchReport GetLaunchReport(ExecuteCommand command)
        {
            DataHandler dataHandler = GetHandler(command.GetType());
            if (dataHandler == null)
                return null;

            return dataHandler.GetLaunchReport(command);
        }

        public ExecuteCommand PrepareCommand(ExecuteCommand command)
        {
            try
            {
                DataHandler dataHandler = GetHandler(command.GetType());

                return dataHandler.GetPreparedCommand(command);
            }
            catch (InvalidOperationException ex)
            {
                LogHelper.Write2Log("Не найден обработчик сообщения типа " + command.GetType().FullName, LogLevel.Error);
                throw new URBDException("Не указан обработчик", ex);
            }
        }
    }
}
