using System;
using System.Collections.Generic;
using System.Linq;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Core;
using System.Diagnostics;
using Wintellect.Threading.AsyncProgModel;
using System.Collections;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy;

namespace Ugoria.URBD.RemoteService
{
    public delegate void ReportAsyncCallback(Report report);

    class QueueExecuteManager
    {
        private RemoteConfigurationManager configurationManager;
        private Dictionary<Command, ReportAsyncCallback> queueTask;
        private SynchronizedCollection<TaskExecute> executedProcess;

        public RemoteConfigurationManager ConfigurationManager
        {
            get { return configurationManager; }
            set { configurationManager = value; }
        }

        public QueueExecuteManager(RemoteConfigurationManager cfgMngr = null)
        {
            this.configurationManager = cfgMngr;
            this.queueTask = new Dictionary<Command, ReportAsyncCallback>();
            this.executedProcess = new SynchronizedCollection<TaskExecute>();
            Init();
        }

        private void Init() { }

        public bool IsBusyBase(string baseName)
        {
            return executedProcess.FirstOrDefault(p => p.Command.baseName.Equals(baseName)) != null;
        }

        public bool IsProcessLaunch(Guid launchGuid)
        {
            return executedProcess.FirstOrDefault(p => p.CommandStrategy.LaunchGuid == launchGuid) != null;
        }

        public bool IsFromQueue(Guid reportGuid)
        {
            lock (((ICollection)queueTask).SyncRoot)
            {
                return queueTask.FirstOrDefault(t => t.Key.guid == reportGuid).Key != null;
            }
        }

        private void ChangeQueue()
        {
            if (configurationManager == null) // задачи будут копиться до тех пор, пока не появится конфигурация
                return;
            // убивать процессы больше суток <--параметр из БД
            lock (((ICollection)queueTask).SyncRoot)
            {
                if (queueTask.Count == 0)
                    return; // нет задач - нет запусков
                IConfiguration mainCfg = configurationManager.GetMainConfiguration();
                int maxThreads = (int)mainCfg.GetParameter("max_threads");
                lock (executedProcess.SyncRoot)
                {
                    if (executedProcess.Count >= maxThreads || queueTask.Count == 0) // нет свободных потоков или ожидающих задач
                        return;

                    // поиск свободной базы из ожидающих задач
                    KeyValuePair<Command, ReportAsyncCallback> taskKey = new KeyValuePair<Command, ReportAsyncCallback>(null, null);
                    foreach (KeyValuePair<Command, ReportAsyncCallback> task in queueTask)
                    {
                        if (!IsBusyBase(task.Key.baseName))
                        {
                            taskKey = task;
                            break;
                        }
                    }
                    // баз не найдено
                    if (taskKey.Key == null)
                        return;

                    queueTask.Remove(taskKey.Key);
                    Command command = taskKey.Key;
                    ReportAsyncCallback callback = taskKey.Value;

                    ICommandStrategy commandStrategy = BuildCommandStrategy(command);

                    TaskExecute taskExecute = new TaskExecute(command, commandStrategy);
                    executedProcess.Add(taskExecute);
                    AsyncEnumerator asyncEnumerator = new AsyncEnumerator();
                    asyncEnumerator.BeginExecute(QueuerEnumerator(asyncEnumerator, taskExecute, callback), asyncEnumerator.EndExecute);
                }
            }
        }

        // Wintellect.Threading AsyncEnumerator, не плодим методы обратного вызова
        private IEnumerator<int> QueuerEnumerator(AsyncEnumerator ae, TaskExecute task, ReportAsyncCallback callback)
        {
            // Запуск процесса
            Action<ReportAsyncCallback> launchFunc = task.CommandStrategy.Launch;
            
                IAsyncResult result = launchFunc.BeginInvoke(callback, ae.End(), null);
                yield return 1;
                launchFunc.EndInvoke(ae.DequeueAsyncResult());            

            // Удаляем из списка работающих процессов и сообщаем об изменении очереди            
            executedProcess.Remove(task);

            ChangeQueue();
        }

        private ICommandStrategy BuildCommandStrategy(Command command)
        {
            IConfiguration cfg = configurationManager.GetBaseConfiguration(command.baseName);

            ICommandStrategy commandStrategy = null;
            if (command.commandType == CommandType.Exchange)
            {
                IModeStrategy modeStrategy = null;
                switch (command.modeType)
                {
                    case ModeType.Aggresive: modeStrategy = new AggresiveMode((string)cfg.GetParameter("log_path"),
                        (string)cfg.GetParameter("base_path"),
                        (int)cfg.GetParameter("wait_time"));
                        break;
                    case ModeType.Passive: modeStrategy = new PassiveMode((string)cfg.GetParameter("log_path"));
                        break;
                    case ModeType.Normal: modeStrategy = new NormalMode((string)cfg.GetParameter("log_path"),
                        (int)cfg.GetParameter("wait_time"));
                        break;
                    case ModeType.Extreme: modeStrategy = new ExtremeMode((string)cfg.GetParameter("log_path"),
                        (string)cfg.GetParameter("base_path"),
                        (int)cfg.GetParameter("wait_time"),
                        executedProcess);
                        break;
                }
                commandStrategy = new ExchangeStrategy(cfg, command.guid, modeStrategy, command.withMD);
            }
            else //if (command.commandType == CommandType.ExtForms)
            {
                commandStrategy = new ExtFormsStrategy(cfg, command.commandDate);
            }
            return commandStrategy;
        }

        public void AddOperation(Command command, ReportAsyncCallback reportCallback)
        {
            lock (((ICollection)queueTask).SyncRoot)
            {
                queueTask.Add(command, reportCallback);
            }
            ChangeQueue();
        }
    }

    class TaskExecute
    {
        private Command command;
        private ICommandStrategy commandStrategy;

        public Command Command
        {
            get { return command; }
        }

        public ICommandStrategy CommandStrategy
        {
            get { return commandStrategy; }
        }

        internal TaskExecute(Command command, ICommandStrategy commandStrategy)
        {
            this.command = command;
            this.commandStrategy = commandStrategy;
        }
    }
}
