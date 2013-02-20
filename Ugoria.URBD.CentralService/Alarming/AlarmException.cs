using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.CentralService.Alarming
{
    class AlarmException : Exception
    {
        public AlarmException(string message) : base(message) { }
        public AlarmException(string message, Exception innerException) : base(message, innerException) { }
    }
}
