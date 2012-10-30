using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.RemoteService.Services
{
    public class CommandEventArgs : EventArgs
    {
        private Command command;

        public Command Command
        {
            get { return command; }
        }

        internal CommandEventArgs(Command command)
        {
            this.command = command;
        }
    }
}
