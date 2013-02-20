using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.CentralService.Alarming
{
    interface IAlarmer
    {
        //void Alarm(int baseId, string text);
        void Alarm(string text);
        void Alarm(Guid reportGuid, string text);
    }
}
