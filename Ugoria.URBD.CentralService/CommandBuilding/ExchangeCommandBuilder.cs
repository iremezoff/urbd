using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService.CommandBuilding
{
    class ExchangeCommandBuilder : ICommandBuilder
    {
        private string description = "обмен пакетами";

        public string Description
        {
            get { return description; }
        }

        private ModeType modeType;

        public ModeType ModeType
        {
            get { return modeType; }
            set { modeType = value; }
        }

        private DateTime releaseUpdate;

        public DateTime ReleaseUpdate
        {
            get { return releaseUpdate; }
            set { releaseUpdate = value; }
        }

        private ExchangeCommandBuilder() { }

        public static ExchangeCommandBuilder Create()
        {
            return new ExchangeCommandBuilder();
        }

        public Command Build()
        {
            return new ExchangeCommand { modeType = modeType, releaseUpdate = releaseUpdate };
        }
    }
}
