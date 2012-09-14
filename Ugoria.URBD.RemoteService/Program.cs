using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts;
using System.Diagnostics;

namespace Ugoria.URBD.RemoteService
{
    class Program
    {
        static void Main (string[] args)
        {
            //Console.WriteLine("стартовать?");
            //Console.ReadLine();
            URBDRemoteWorker worker = new URBDRemoteWorker();
            worker.Start();
            Console.ReadLine();
        }
    }
}
