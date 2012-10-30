using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService.CommandBuilding
{
    public interface ICommandBuilder
    {
        string Description { get; }
        Command Build();
    }
}
