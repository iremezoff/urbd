using System;
using System.Collections.Generic;
using System.Text;
using Ugoria.URBD.Contracts;
using System.IO;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.Contracts.Handlers.Strategy.Exchange.Mode
{
    public interface IMode
    {
        string Message { get; }
        bool IsSuccess { get; }
        bool IsWarning { get; }
        bool IsAborted { get; }
        bool CompleteExchange(bool haveMD);
    }    
}
