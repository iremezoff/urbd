using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.CentralService
{
    interface IAlarmer
    {
        void Alarm(string serviceAddr, string text);
        void Alarm (Uri serviceUri, string text);
    }
}
