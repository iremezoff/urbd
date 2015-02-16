using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ugoria.URBD.CentralService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("central: Стартовать службу?");
            Console.ReadLine();

            URBDCentralWorker worker = new URBDCentralWorker();
            worker.Start();

            Console.ReadLine();
        }
    }
}
