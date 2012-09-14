using System.Collections.Generic;
using System.Diagnostics;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class ExtremeMode : AggresiveMode
    {
        private bool attempt = false;
        private SynchronizedCollection<TaskExecute> executedProcess;

        public new string Status
        {
            get { return "Extreme mode: " + status; }
        }

        public ExtremeMode (string logfilename, string basepath, int waitTime, SynchronizedCollection<TaskExecute> executedProcess)
            : base(logfilename, basepath, waitTime)
        {
            this.executedProcess = executedProcess;
        }

        public override bool Verification ()
        {
            // если обмен прошел или Aggresive-попытка не удалась
            if (base.Verification() || attempt)
                return true;

            // убивать процессы только не выполнящиеся от имени сервиса УРБД (имеются в списке executedProcess)
            foreach (Process process in Process.GetProcesses())
            {
                if ("1cv7s.exe".Equals(process) /*&& executedProcess.FirstOrDefault(p => p.CommandStrategy.Pid == process.Id) == null*/)
                    process.Kill(); // потом pskill.exe \\адрес_терминального_сервера [-u от имени какого-то пользователя] 1cv7s.exe
            }
            attempt = true; // последняя попытка исчерпана
            return false;
        }
    }
}
