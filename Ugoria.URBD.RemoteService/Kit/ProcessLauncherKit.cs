using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Security;

namespace Ugoria.URBD.RemoteService.Kit
{
    public enum LaunchMode { Config, Enterprise }
    public delegate int ProcessCallback();

    class Process1CLauncherKit
    {
        private string path1c;

        public string Path1c
        {
            get { return path1c; }
            set { path1c = value; }
        }
        private string username;

        public string Username
        {
            get { return username; }
            set { username = value; }
        }
        private SecureString password;

        public SecureString Password
        {
            get { return password; }
            set { password = value; }
        }
        private string basePath;

        public string BasePath
        {
            get { return basePath; }
            set { basePath = value; }
        }
        private LaunchMode launchMode;

        public LaunchMode LaunchMode
        {
            get { return launchMode; }
            set { launchMode = value; }
        }

        private string prmPath;

        public string PrmFile
        {
            get { return prmPath; }
            set { prmPath = value; }
        }

        public int Start()
        {
            process.StartInfo.Arguments = ArgumentsBuild();
            process.Start();
            return process.Id;
        }

        public int BeginStart(AsyncCallback callback)
        {
            process.StartInfo.Arguments = ArgumentsBuild();
            process.Exited += (sender, args) => callback(null);
            if (!string.IsNullOrEmpty(username) && password != null)
            {
                //process.StartInfo.UserName = username;
                //process.StartInfo.Password = password;
            }
            else
            {
                process.StartInfo.UserName = null;
                process.StartInfo.Password = null;
            }
            process.Start();
            return process.Id;
        }

        public void EndStart(IAsyncResult result)
        {
            process.WaitForExit();
        }

        public void Interrupt()
        {
            if (process.HasExited)
                return;
            process.Kill(); // далее будет вызвано событие Exited
        }

        private string ArgumentsBuild()
        {
            string arguments = "";
            switch (launchMode)
            {
                case Kit.LaunchMode.Config:
                    if (string.IsNullOrEmpty(prmPath))
                        throw new ArgumentException("В режиме Config путь до prm-файла не может быть пустым");
                    arguments = String.Format("config /D\"{0}\" /N {1} /P {2} /@\"{3}\"", basePath, user1c, password1c, prmPath);
                    break;
            }
            return arguments;
        }

        private Process process;
        private string password1c;

        public string Password1c
        {
            get { return password1c; }
            set { password1c = value; }
        }
        private string user1c;

        public string User1c
        {
            get { return user1c; }
            set { user1c = value; }
        }

        public Process1CLauncherKit(string path1c, string user1c, string password1c, string basePath, LaunchMode launchMode)
        {
            this.path1c = path1c;
            this.user1c = user1c;
            this.password1c = password1c;
            this.basePath = basePath;
            this.launchMode = launchMode;

            this.process = new Process();
            process.StartInfo.FileName = path1c;
            process.StartInfo.UseShellExecute = false;
        }
    }

    class ProcessEventArgs : EventArgs
    {
        private AsyncCallback callback;

        public AsyncCallback Callback
        {
            get { return callback; }
        }

        private ProcessAsyncResult asyncResult;

        public ProcessAsyncResult AsyncResult
        {
            get { return asyncResult; }
        }

        internal ProcessEventArgs(AsyncCallback callback, ProcessAsyncResult asyncResult)
        {
            this.callback = callback;
            this.asyncResult = asyncResult;
        }
    }

    class ProcessAsyncResult : IAsyncResult
    {
        private int pid;

        public int Pid
        {
            get { return pid; }
            set { pid = value; }
        }

        public object AsyncState
        {
            get { return pid; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return new AutoResetEvent(false); }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        private bool isCompleted = false;

        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }

        private Process process;

        public Process Process
        {
            get { return process; }
            set { process = value; }
        }

        private AsyncCallback callback;

        public AsyncCallback Callback
        {
            get { return callback; }
            set { callback = value; }
        }
    }
}
