using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Handlers;
using System.Reflection;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Context;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.RemoteService
{
    class MessageHandler
    {
        private List<IStrategyBuilder> buildersStore = new List<IStrategyBuilder>();

        public List<IStrategyBuilder> BuildersStore
        {
            get { return buildersStore; }
            set { buildersStore = value; }
        }

        public MessageHandler(List<IStrategyBuilder> builders)
        {
            buildersStore.AddRange(builders);
        }

        private IStrategyBuilder GetBuilder(Type commandType)
        {
            Attribute attr = Attribute.GetCustomAttribute(commandType, typeof(CommandHandlerAttribute), true);
            if (attr == null)
                throw new URBDException("Не указан обработчик");
            CommandHandlerAttribute commandAttr = (CommandHandlerAttribute)attr;

            IStrategyBuilder strategyBuilder = null;

            strategyBuilder = buildersStore.FirstOrDefault(h => commandAttr.StrategyBuilder.IsAssignableFrom(h.GetType())); // <------ проверить

            if (strategyBuilder == null)
            {
                LogHelper.Write2Log("Не найден обработчик сообщения типа " + commandType, LogLevel.Error);
                return null;
            }
            return strategyBuilder;
        }

        public ICommandStrategy GetStrategy(IConfiguration configuration, Command command)
        {
            IStrategyBuilder builder = GetBuilder(command.GetType());
            if (builder == null)
                return null;

            return builder.Build(new StrategyContext(command, configuration));
        }

        public LaunchReport GetLaunchReport(Command command, ICommandStrategy strategy)
        {
            IStrategyBuilder builder = GetBuilder(command.GetType());
            if (builder == null)
                return null;
            return builder.GetLaunchReport(strategy);
        }

        public OperationReport GetOperationReport(Command command, ICommandStrategy strategy)
        {
            IStrategyBuilder builder = GetBuilder(command.GetType());
            if (builder == null)
                return null;
            return builder.GetOperationReport(strategy);
        }
    }
}
