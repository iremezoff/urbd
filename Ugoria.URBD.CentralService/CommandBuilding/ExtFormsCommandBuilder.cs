using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService.CommandBuilding
{
    class ExtFormsCommandBuilder : ICommandBuilder
    {
        private string description = "обновление ExtForms";

        public string Description
        {
            get { return description; }
        }

        private ExtFormsCommandBuilder() { }

        public static ExtFormsCommandBuilder Create()
        {
            return new ExtFormsCommandBuilder();
        }

        public Command Build()
        {
            return new ExtFormsCommand();
        }
    }
}
