using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.Contracts.Context
{
    public interface IContext
    {
        IConfiguration Configuration { get; }
        Command Command { get; }
    }
}
