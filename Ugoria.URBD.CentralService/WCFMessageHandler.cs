using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Handlers;
using System.Reflection;
using Ugoria.URBD.Shared;
using Ugoria.URBD.CentralService.DataProvider;
using System.Transactions;

namespace Ugoria.URBD.CentralService
{
    class WCFMessageHandler
    {
        //private List<DataHandler> handlerStore = new List<DataHandler>();
        private Dictionary<Type, DataHandler> handlerStore = new Dictionary<Type, DataHandler>();
        private Dictionary<string, Type> components = new Dictionary<string, Type>();

        public WCFMessageHandler()
        {
            //this.handlerStore.AddRange(handlers);
        }

        public void AddHandler(DataHandler handler)
        {
            Attribute attr = Attribute.GetCustomAttribute(handler.ReportType, typeof(ReportHandlerAttribute), true);
            ReportHandlerAttribute reportAttr = (ReportHandlerAttribute)attr;
           
            attr = Attribute.GetCustomAttribute(reportAttr.CommandType, typeof(CommandHandlerAttribute), true);
            CommandHandlerAttribute commandAttr = (CommandHandlerAttribute)attr;

            int index = 0;
            /*if (reportAttr.CommandType.IsSubclassOf(typeof(ExecuteCommand)) && (index = reportAttr.CommandType.Name.IndexOf("Command")) > 0)
                components.Add(reportAttr.CommandType.Name.Substring(0, index), reportAttr.CommandType);*/
            if (handler.ReportType.IsSubclassOf(typeof(OperationReport)) && (index = handler.ReportType.Name.IndexOf("Report")) > 0)
                components.Add(handler.ReportType.Name.Substring(0, index), handler.ReportType);
            handlerStore.Add(handler.ReportType, handler);
            handlerStore.Add(reportAttr.CommandType, handler);
        }

        public void HandleReport(Report report)
        {
            try
            {
                if (report is LaunchReport)
                {
                    string componentName = ((LaunchReport)report).componentName;
                    if(string.IsNullOrEmpty(componentName) || !components.ContainsKey(componentName))
                        return;
                    Type reportType = components [componentName];
                    DataHandler dataHandler = handlerStore[reportType].Clone();
                    dataHandler.SetLaunchReport((LaunchReport)report);
                }
                else if (report is OperationReport)
                {
                    DataHandler dataHandler = handlerStore[report.GetType()].Clone();
                    dataHandler.SetReport((OperationReport)report);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Write2Log("Не найден обработчик сообщения типа " + report.GetType().FullName, LogLevel.Error);
                LogHelper.Write2Log(ex);
                throw new URBDException("Не указан обработчик", ex);
            }
        }

        public void SetCommandReport(ExecuteCommand command)
        {
            SetCommandReport(command, 1);
        }

        public void SetCommandReport(ExecuteCommand command, int userId)
        {
            DataHandler dataHandler = handlerStore[command.GetType()].Clone();

            if (dataHandler == null)
                return;
            dataHandler.SetCommandReport(command, userId);
        }

        /*private DataHandler GetHandler(Report report)
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
            return dataHandler.Clone(); // берем прототип и клонируем его, дабы кеш никак не пересекался
        }*/

        public LaunchReport GetLaunchReport(ExecuteCommand command)
        {
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.Snapshot }))
            {
                DataHandler dataHandler = handlerStore[command.GetType()].Clone();
                if (dataHandler == null)
                    return null;

                return dataHandler.GetLaunchReport(command);
            }
        }

        public ExecuteCommand PrepareCommand(ExecuteCommand command)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions() { IsolationLevel = System.Transactions.IsolationLevel.Snapshot }))
                {
                    DataHandler dataHandler = handlerStore[command.GetType()].Clone();

                    return dataHandler.GetPreparedCommand(command);
                }
            }
            catch (InvalidOperationException ex)
            {
                LogHelper.Write2Log("Не найден обработчик сообщения типа " + command.GetType().FullName, LogLevel.Error);
                LogHelper.Write2Log(ex);
                throw new URBDException("Не указан обработчик", ex);
            }
        }
    }
}
