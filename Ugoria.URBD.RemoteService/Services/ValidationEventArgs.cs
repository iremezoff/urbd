using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.RemoteService.Services
{
    public class ValidationEventArgs:EventArgs
    {
        private RemoteConfiguration configuration;

        public RemoteConfiguration Configuration
        {
            get { return configuration; }
        }

        internal ValidationEventArgs(RemoteConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }
}
