using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.RemoteService.Services
{
    public class CheckEventArgs : EventArgs
    {
        private CheckCommand checkCommand;

        public CheckCommand CheckCommand
        {
            get { return checkCommand; }
        }

        internal CheckEventArgs(CheckCommand checkCommand)
        {
            this.checkCommand = checkCommand;
        }
    }
}
