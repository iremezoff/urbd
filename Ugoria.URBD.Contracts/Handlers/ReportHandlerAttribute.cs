using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Contracts.Handlers
{
    public class ReportHandlerAttribute : Attribute
    {
        private Type handlerType;

        public Type HandlerType
        {
            get { return handlerType; }
            set { handlerType = value; }
        }

        private Type commandType;

        public Type CommandType
        {
            get { return commandType; }
            set { commandType = value; }
        }
    }
}