using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Contracts
{
    [Serializable]
    public class URBDException : Exception
    {
        public URBDException(string message) : base(message) { }
        public URBDException(string message, Exception innerException) : base(message, innerException) { }
    }
}
