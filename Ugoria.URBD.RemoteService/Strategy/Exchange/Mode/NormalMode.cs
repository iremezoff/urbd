using System;
using System.Threading;
using Ugoria.URBD.Shared;
using System.IO;
using System.Text;

namespace Ugoria.URBD.RemoteService.Strategy.Exchange.Mode
{
    class NormalMode : DefaultMode
    {
        private bool attempt = false;

        public NormalMode(Verifier verifier, string basepath, int waitTime)
            : base(verifier, basepath, waitTime) { }

        public override bool CompleteExchange(bool haveMD)
        {
            // ошибок при обмене не было либо ранее попытка была проведена            
            if (base.CompleteExchange(haveMD) || attempt)
                return true;

            // при haveMD пауза устанавливается в ModeStrategy
            if (!haveMD)
            {
                LogHelper.Write2Log(String.Format("Режим Normal. Повтор запуска через (мин): {0}", waitTime), LogLevel.Information);
                Thread.Sleep(new TimeSpan(0, waitTime, 0)); // спать 10 минут до следующей попытки
            }
            attempt = true; // попытались раз

            return false;
        }
    }
}
