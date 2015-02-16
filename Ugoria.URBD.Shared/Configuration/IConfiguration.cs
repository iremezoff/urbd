using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Shared.Configuration
{
    public interface IConfiguration
    {
        object GetParameter(string key);
    }
}
