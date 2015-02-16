using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Handlers;
using Ugoria.URBD.Contracts.Data;
using System.Data;

namespace Ugoria.URBD.Contracts.Context
{
    public class StrategyContext : IContext
    {
        public IConfiguration Configuration
        {
            get { return configuration; }
        }

        public Command Command
        {
            get { return command; }
        }

        private IConfiguration configuration;
        private Command command;

        public StrategyContext(Command command, IConfiguration configuration)
        {
            this.command = command;
            this.configuration = configuration;
        }
    }
}
