using System;
using System.Threading;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class NormalMode : ModeStrategy
    {
        private bool attempt = false;
        private int waitTime = 10; // 10 минут

        public new string Status
        {
            get { return "Normal mode: " + status; }
        }

        public NormalMode (string logFilename, int waitTime)
            : base(logFilename)
        {
            this.waitTime = waitTime;
        }

        public override bool Verification ()
        {
            // ошибок при обмене не было либо ранее попытка была проведена
            if (base.Verification() || attempt)
                return true;

            attempt = true; // попытались раз
            Thread.Sleep(new TimeSpan(0, waitTime, 0)); // спать 10 минут до следующей попытки
            return false;
        }
    }
}
