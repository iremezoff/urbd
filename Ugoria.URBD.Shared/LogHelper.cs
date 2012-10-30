using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Ugoria.URBD.Shared
{
    public enum URBDComponent { Central, Remote, Web }
    public enum LogLevel { Error, Information, Warning }

    public static class LogHelper
    {
        private static object objLock = new object();
        private static int tile = 0;
        private static bool LogEnabled = true;
        private static string logDir = "";

        public static bool IsConsoleOutputEnabled = false;
        public static bool IsDiagnosticTraceOutputEnabled = false;
        public static URBDComponent CurrentComponent = URBDComponent.Web;
        // возможные пути сохранения логов: директрория программы, диск С, директория AppData приложения
        private static string[] logDirs = new string[] { AppDomain.CurrentDomain.BaseDirectory, @"c:\", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) };

        static LogHelper()
        {
            foreach (string el in logDirs)
            {
                if (AccessCheck.IsRuleAllow(el, FileSystemRights.CreateDirectories))
                {
                    logDir = el;
                    break;
                }
            }

            if (string.IsNullOrEmpty(logDir))
            {
                LogEnabled = false;
                return;
            }

            logDir += @"\urbd-logs";
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);
        }        

        public static void Write2Log(URBDComponent component, string message, LogLevel level)
        {
            if (!LogEnabled)
                return;
            lock (objLock)
            {
                string outputMsg = String.Format("[{0:dd.MM.yyyy HH:mm:ss}, {1}] {2}", DateTime.Now, level, message);
                if (IsConsoleOutputEnabled)
                    Console.WriteLine(outputMsg);
                if (IsDiagnosticTraceOutputEnabled)
                    System.Diagnostics.Trace.WriteLine(outputMsg);
                while (true)
                {
                    string filepath = String.Format("{0}/{1}_{2:yyyy-MM-dd}{3}.txt", logDir, component, DateTime.Now, tile > 0 ? "_" + tile : string.Empty);
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(filepath, true))
                        {
                            sw.WriteLine(outputMsg);
                        }
                        break;
                    }
                    catch (IOException ex)
                    {
                        tile++;
                        if (tile < 10)
                            continue;
                        if (IsConsoleOutputEnabled)
                            Console.WriteLine("Не удалось записать в log-файл: " + ex);
                        if (IsDiagnosticTraceOutputEnabled)
                            System.Diagnostics.Trace.WriteLine("Не удалось записать в log-файл: " + ex);
                        break;
                    }
                }
            }
        }

        public static void Write2Log(string message, LogLevel level)
        {
            Write2Log(CurrentComponent, message, level);
        }

        public static void Write2Log(Exception exception)
        {
            Write2Log(CurrentComponent, String.Format("Exception message: {0}\r\nInnerException message: {1}\r\nTrace: {2}",
                exception.Message,
                exception.InnerException != null ? exception.InnerException.Message : string.Empty,
                exception.StackTrace), LogLevel.Error);
        }

        public static void Write2Log(URBDComponent component, Exception exception)
        {
            Write2Log(component, String.Format("Exception message: {0}\r\nInnerException message: {1}\r\nTrace: {2}",
                exception.Message,
                exception.InnerException != null ? exception.InnerException.Message : string.Empty,
                exception.StackTrace), LogLevel.Error);
        }
    }
}
