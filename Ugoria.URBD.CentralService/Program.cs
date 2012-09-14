using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts;
using System.Threading;

namespace Ugoria.URBD.CentralService
{
    class Program
    {
        static void Main(string[] args)
        {
            string remoteServiceIP = "10.86.4.57";
            Console.WriteLine("central: Стартовать службу?");
            Console.ReadLine();

            URBDCentralWorker worker = new URBDCentralWorker();
            worker.Start();

            /*Thread.Sleep(3000);

            Console.WriteLine("central: Запрос на запуск блокнота");
            wcfClient.CommandExecute(new Command { baseName = "test", commandType = CommandType.Exchange, modeType = ModeType.Normal });
            */
            Console.ReadLine();
        }
    }
}
