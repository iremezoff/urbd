using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.RemoteService.Services
{
    public class CommandEventArgs : EventArgs
    {
        private ExecuteCommand command;

        public ExecuteCommand Command
        {
            get { return command; }
        }

        internal CommandEventArgs(ExecuteCommand command)
        {
            this.command = command;
        }
    }
}
