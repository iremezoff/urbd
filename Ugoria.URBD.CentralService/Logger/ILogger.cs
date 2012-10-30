using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.CentralService.Logging
{
    public interface ILogger
    {
        bool IsFailEnabled { get; set; }
        bool IsInformationEnabled { get; set; }
        bool IsWarningEnabled { get; set; }

        void Fail(Uri uri, string message);
        void Fail(string address, string message);
        void Information(Uri uri, string message);
        void Information(string address, string message);
        void Warning(Uri uri, string message);
        void Warning(string address, string message);
    }
}
