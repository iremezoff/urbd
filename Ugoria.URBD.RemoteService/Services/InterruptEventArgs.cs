using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService.Services
{
    public class InterruptEventArgs : EventArgs
    {
        private Guid guid;

        public Guid CommandGuid
        {
            get { return guid; }
        }

        internal InterruptEventArgs(Guid guid)
        {
            this.guid = guid;
        }
    }
}
