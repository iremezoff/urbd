using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class AggresiveMode : ModeStrategy
    {
        private bool attention = false;
        protected string basepath = "";
        protected int waitTime = 10;

        public AggresiveMode(Verifier verifier, string basepath, int waitTime)
            : base(verifier)
        {
            this.basepath = basepath;
            this.waitTime = waitTime;
        }

        public override bool CompleteExchange()
        {
            // проблем при обмене не было либо уже просили о выходе, 2 раза повторять не будем, забиваем на обмен
            if (base.CompleteExchange() || attention)
            {
                // Удаление файла с просьбой для обмена
                FileInfo fi = new FileInfo(String.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath));
                if (fi.Exists)
                    fi.Delete();
                return true;
            }

            LogHelper.Write2Log("Режим Aggressive. Повтор запуска через (мин):" + waitTime, LogLevel.Information);
            // создание файла с просьбой
            using (StreamWriter sw = new StreamWriter(String.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath), false, Encoding.GetEncoding(1251)))
            {
                sw.WriteLine("Требуется выполнить автообмен, закройте программу 1С");
            }
            attention = true; // попытка с оповещением была
            Thread.Sleep(new TimeSpan(0, waitTime, 0)); // спать до следующей попытки
            return false; // верификация не пройдена
        }
    }
}
