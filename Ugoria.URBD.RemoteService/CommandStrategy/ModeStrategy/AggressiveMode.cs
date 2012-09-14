using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class AggresiveMode : ModeStrategy
    {
        private bool attention = false;
        private string basepath = "";
        private int waitTime = 10;

        public new string Status
        {
            get { return "Aggresive mode: " + status; }
        }

        public AggresiveMode (string logfilename, string basepath, int waitTime)
            : base(logfilename)
        {
            this.basepath = basepath;
            this.waitTime = waitTime;
        }

        public override bool Verification ()
        {
            // проблем при обмене не было либо уже просили о выходе, 2 раза повторять не будем, забиваем на обмен
            if (base.Verification() || attention)
            {
                // Удаление файла с просьбой для обмена
                if (File.Exists(String.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath)))
                    File.Delete(String.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath));
                return true;
            }

            // создание файла с просьбой
            using (StreamWriter sw = new StreamWriter(new FileStream(String.Format(@"{0}\ExtForms\!md_message_urbd.txt", basepath), FileMode.Create),
                Encoding.GetEncoding(1251)))
            {
                sw.WriteLine("Требуется выполнить автообмен, закройте программу 1С");
            }
            attention = true; // оповещение было
            Thread.Sleep(new TimeSpan(0, waitTime, 0)); // спать до следующей попытки
            return false; // верификация не пройдена
        }
    }
}
