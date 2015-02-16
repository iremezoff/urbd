using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Ugoria.URBD.RemoteService
{
    class Program
    {
        static void Main (string[] args)
        {
            //Console.WriteLine("стартовать?");
            //Console.ReadLine();
            try
            {
                URBDRemoteWorker worker = new URBDRemoteWorker();
                worker.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
    }
}
