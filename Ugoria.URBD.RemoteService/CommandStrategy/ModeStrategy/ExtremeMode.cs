using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    class ExtremeMode : AggresiveMode
    {
        private bool attempt = false;

        public ExtremeMode(Verifier verifier, string basepath, int waitTime)
            : base(verifier, basepath, waitTime)
        { }

        public override bool CompleteExchange()
        {
            // если обмен прошел или Aggresive-попытка не удалась
            if (base.CompleteExchange() || attempt)
                return true;

            Uri basepathUri = new Uri(basepath);

            Process process = new Process();
            process.StartInfo.FileName = "openfiles.exe";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.Arguments = String.Format("/query /s {0} /fo csv /nh", basepathUri.IsLoopback ? "localhost" : basepathUri.Host);
            process.Start();
            string[] lines = process.StandardOutput.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            // логика вырубания 1cv7.LCK и 1cv7.MD

            attempt = true; // последняя попытка исчерпана
            return false;
        }
    }
}
