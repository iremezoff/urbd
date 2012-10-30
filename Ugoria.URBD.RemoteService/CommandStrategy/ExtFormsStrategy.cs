using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;

namespace Ugoria.URBD.RemoteService
{
    class ExtFormsStrategy : ICommandStrategy
    {
        private IConfiguration configuration;
        private DateTime commandDate;

        public ExtFormsStrategy(IConfiguration configuration, DateTime commandDate)
        {
            this.configuration = configuration;
            this.commandDate = commandDate;
        }        

        public void Launch(ReportAsyncCallback reportAsyncsCallback)
        {
            throw new NotImplementedException();
        }

        public Guid LaunchGuid
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsInterrupt
        {
            get { throw new NotImplementedException(); }
        }

        public void Interrupt()
        {
            throw new NotImplementedException();
        }
    }
}
