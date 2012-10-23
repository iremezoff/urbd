using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService.Services
{
    public delegate void CommandSendedHandler(RemoteServiceProxy sender, SendCommandEventArgs e);
    public class SendCommandEventArgs : EventArgs
    {
        private Command command;

        public Command Command
        {
            get { return command; }
        }

        internal SendCommandEventArgs(Command command)
        {
            this.command = command;
        }
    }
}
