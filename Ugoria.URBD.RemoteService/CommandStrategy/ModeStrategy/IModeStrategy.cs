using System;
using System.Collections.Generic;
using System.Text;
using Ugoria.URBD.Contracts;
using System.IO;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy;

namespace Ugoria.URBD.RemoteService
{
    interface IModeStrategy
    {
        string Message { get; }
        bool IsSuccess { get; }
        bool IsWarning { get; }
        bool IsAborted { get; }
        Verifier Verifier { get; }
        bool CompleteExchange();
    }    
}
