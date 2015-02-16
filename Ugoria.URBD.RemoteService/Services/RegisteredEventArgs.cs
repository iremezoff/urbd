using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService.Services
{
    public class RegisteredEventArgs : EventArgs
    {
        private Uri centralUri;
        private Uri localUri;

        public Uri CentralUri
        {
            get { return centralUri; }
        }

        public Uri LocalUri
        {
            get { return localUri; }
        }

        internal RegisteredEventArgs(Uri centralUri, Uri localUri)
        {
            this.centralUri = centralUri;
            this.localUri = localUri;
        }
    }
}
