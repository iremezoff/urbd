using System;
using System.Collections.Generic;
using System.Text;
using Ugoria.URBD.Contracts;
using System.IO;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.RemoteService
{
    interface IModeStrategy
    {
        string Status { get; }
        bool IsVerified { get; }
        List<MLGMessage> Messages { get; }
        bool Verification();
    }

    
}
