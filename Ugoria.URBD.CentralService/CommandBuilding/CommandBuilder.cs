using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService.CommandBuilding
{
    public class CommandBuilder : ICommandBuilder
    {
        private string description = "команда";

        public string Description
        {
            get { return particularBuilder != null ? particularBuilder.Description : description; }
        }

        private int baseId;

        public int BaseId
        {
            get { return baseId; }
            set { baseId = value; }
        }

        private DateTime configurationChangeDate = DateTime.MinValue;

        public DateTime ConfigurationChangeDate
        {
            get { return configurationChangeDate; }
            set { configurationChangeDate = value; }
        }

        private DateTime releaseUpdate = DateTime.MinValue;

        public DateTime ReleaseUpdate
        {
            get { return releaseUpdate; }
            set { releaseUpdate = value; }
        }

        private ICommandBuilder particularBuilder;

        public ICommandBuilder ParticularBuilder
        {
            get { return particularBuilder; }
            set { particularBuilder = value; }
        }

        private CommandBuilder() { }

        public static CommandBuilder Create()
        {
            return new CommandBuilder();
        }

        public Command Build()
        {
            Command command = particularBuilder != null ? particularBuilder.Build() : new Command();
            command.baseId = baseId;
            command.commandDate = DateTime.Now;
            command.reportGuid = Guid.NewGuid();
            command.configurationChangeDate = configurationChangeDate;
            command.releaseUpdate = releaseUpdate;
            return command;
        }
    }
}
