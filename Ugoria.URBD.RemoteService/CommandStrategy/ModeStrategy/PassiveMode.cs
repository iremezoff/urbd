using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class PassiveMode : ModeStrategy
    {
        public new string Status
        {
            get { return "Passive mode: " + status; }
        }

        public PassiveMode (string logFilename)
            : base(logFilename) { }

        public override bool Verification ()
        {
            return base.Verification();
        }
    }
}
