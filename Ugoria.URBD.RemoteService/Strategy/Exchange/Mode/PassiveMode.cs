using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService.Strategy.Exchange.Mode
{
    class PassiveMode : DefaultMode
    {
        private bool attempt = false;
        public PassiveMode(Verifier verifier, string basepath, int waitTime)
            : base(verifier, basepath, waitTime) { }

        public override bool CompleteExchange(bool haveMD)
        {
            // если MD нет и попыток не было, то пассивный режим во всей красе - отрабатываем операцию, независимо от результата проверки
            bool isComplete = base.CompleteExchange(haveMD);
            if (!haveMD || attempt)
                return true;
            // MD есть и попыток загрузки не было, значит попытка засчитывается и ожидается результат проверки обмена
            attempt = true;
            return isComplete;
        }
    }
}
