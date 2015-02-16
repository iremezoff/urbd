using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Contracts.Handlers
{
    public class CommandHandlerAttribute : Attribute
    {
        private Type handlerType;
        public Type HandlerType
        {
            get { return handlerType; }
            set { handlerType = value; }
        }

        private Type reportType;
        public Type ReportType
        {
            get { return reportType; }
            set { reportType = value; }
        }

        private Type strategyBuilder;
        public Type StrategyBuilder
        {
            get { return strategyBuilder; }
            set { strategyBuilder = value; }
        }
    }
}
