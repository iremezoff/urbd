using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class PassiveMode : ModeStrategy
    {
        public PassiveMode (Verifier verifier)
            : base(verifier) { }

        public override bool CompleteExchange ()
        {
            base.CompleteExchange();
            return true;
        }
    }
}
