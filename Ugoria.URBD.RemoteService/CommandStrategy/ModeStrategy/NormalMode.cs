using System;
using System.Threading;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class NormalMode : ModeStrategy
    {
        private bool attempt = false;
        private int waitTime = 10; // 10 минут
        private Verifier verifier;

        public NormalMode(Verifier verifier, int waitTime)
            : base(verifier)
        {
            this.waitTime = waitTime;
        }

        public override bool CompleteExchange()
        {
            // ошибок при обмене не было либо ранее попытка была проведена
            if (base.CompleteExchange() || attempt)
                return true;

            LogHelper.Write2Log("Режим Normal. Повтор запуска через (мин):" + waitTime, LogLevel.Information);
            attempt = true; // попытались раз
            Thread.Sleep(new TimeSpan(0, waitTime, 0)); // спать 10 минут до следующей попытки
            return false;
        }
    }
}
