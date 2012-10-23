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
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy;
using Ugoria.URBD.RemoteService.CommandStrategy;
using Ugoria.URBD.Logging;

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

        public bool IsBusyBase(int baseId)
        {
            return executedProcess.FirstOrDefault(p => p.Command.baseId==baseId) != null;
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

        public void KillTask(Guid reportGuid, ReportAsyncCallback callback)
        {
            lock (((ICollection)queueTask).SyncRoot)
            {
                LogHelper.Write2Log("Поиск в очереди " + reportGuid, LogLevel.Information);
                Command cmd = queueTask.FirstOrDefault(t => t.Key.guid == reportGuid).Key;
                if (cmd != null)
                {
                    LogHelper.Write2Log("Найдено. Попытка удаления " + reportGuid, LogLevel.Information);
                    ReportBuilder reportBuilder = ReportBuilder.Create();
                    reportBuilder.CommandDate = cmd.commandDate;
                    reportBuilder.ReportGuid = reportGuid;
                    OperationReportBuilder builder = OperationReportBuilder.Create();
                    builder.CompleteDate = DateTime.Now;
                    builder.Status = ReportStatus.Interrupt;
                    builder.Message = "Задача исключена из очереди";
                    reportBuilder.ConcreteBuilder = builder;

                    ReportAsyncCallback reportCallback = queueTask[cmd];
                    reportCallback(reportBuilder.Build());
                    ChangeQueue();
                    return;
                }
                LogHelper.Write2Log("Поиск в выполняющихся процессах " + reportGuid, LogLevel.Information);
                TaskExecute task = executedProcess.FirstOrDefault(p => p.Command.guid == reportGuid);
                if (task != null)
                {
                    LogHelper.Write2Log(String.Format("Найдено. Попытка снятия задачи {0} и процесса", reportGuid, task.CommandStrategy.LaunchGuid), LogLevel.Information);
                    task.CommandStrategy.Interrupt();
                    executedProcess.Remove(task);
                    ChangeQueue();
                    return;
                }                
            }
            LogHelper.Write2Log(String.Format("Задача {0} потеряна, ничего не снято", reportGuid), LogLevel.Information);
            ReportBuilder reportBuilderEmpty = ReportBuilder.Create();
            reportBuilderEmpty.ReportGuid = reportGuid;
            OperationReportBuilder concreteBuilderEmpty = OperationReportBuilder.Create();
            concreteBuilderEmpty.CompleteDate = DateTime.Now;
            concreteBuilderEmpty.Status = ReportStatus.Interrupt;
            concreteBuilderEmpty.Message = "Задача отсутствовала в очереди и не была запущена";
            reportBuilderEmpty.ConcreteBuilder = concreteBuilderEmpty;
            callback(reportBuilderEmpty.Build());
        }

        private void ChangeQueue()
        {
            LogHelper.Write2Log("Изменение очереди", LogLevel.Information);
            if (configurationManager == null) // задачи будут копиться до тех пор, пока не появится конфигурация
                return;
            // убивать процессы больше суток <--параметр из БД
            lock (((ICollection)queueTask).SyncRoot)
            {
                LogHelper.Write2Log("Проверка наличия задач в очереди", LogLevel.Information);
                if (queueTask.Count == 0)
                    return; // нет задач - нет запусков
                LogHelper.Write2Log("Задачи есть", LogLevel.Information);
                IConfiguration mainCfg = configurationManager.GetMainConfiguration();
                int maxThreads = (int)mainCfg.GetParameter("max_threads");

                lock (executedProcess.SyncRoot)
                {
                    LogHelper.Write2Log("Поиск свободных потоков", LogLevel.Information);
                    if (executedProcess.Count >= maxThreads) // нет свободных потоков 
                        return;

                    LogHelper.Write2Log("Поиск свободной базы для задачи", LogLevel.Information);
                    // поиск свободной базы из ожидающих задач
                    KeyValuePair<Command, ReportAsyncCallback> taskKey = new KeyValuePair<Command, ReportAsyncCallback>(null, null);
                    foreach (KeyValuePair<Command, ReportAsyncCallback> task in queueTask)
                    {
                        if (!IsBusyBase(task.Key.baseId))
                        {
                            taskKey = task;
                            break;
                        }
                    }
                    // баз не найдено
                    if (taskKey.Key == null)
                        return;
                    LogHelper.Write2Log("База найдена: " + taskKey.Key.baseId, LogLevel.Information);
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
            LogHelper.Write2Log(String.Format("Запуск процесса {0}. База: {1}. Время команды: {2}. Режим: {3}", task.Command.guid, task.Command.baseId, task.Command.commandDate, task.Command.modeType), LogLevel.Information);
            Action<ReportAsyncCallback> launchFunc = task.CommandStrategy.Launch;

            IAsyncResult result = launchFunc.BeginInvoke(callback, ae.End(), null);
            yield return 1;
            launchFunc.EndInvoke(ae.DequeueAsyncResult());
            LogHelper.Write2Log(String.Format("Процесс {0} завершен", task.Command.guid), LogLevel.Information);

            // Удаляем из списка работающих процессов и сообщаем об изменении очереди, если конечно процесс не был прерван
            if (task.CommandStrategy.IsInterrupt)
                yield break;
            executedProcess.Remove(task);
            ChangeQueue();
        }

        private ICommandStrategy BuildCommandStrategy(Command command)
        {
            IConfiguration cfg = configurationManager.GetBaseConfiguration(command.baseId);

            ICommandStrategy commandStrategy = null;
            if (command.commandType == CommandType.Exchange)
            {
                Verifier verifier = new Verifier((string)cfg.GetParameter("log_path"));
                IModeStrategy modeStrategy = null;
                switch (command.modeType)
                {
                    case ModeType.Aggresive: modeStrategy = new AggresiveMode(verifier,
                        (string)cfg.GetParameter("base_path"),
                        (int)cfg.GetParameter("wait_time"));
                        break;
                    case ModeType.Passive: modeStrategy = new PassiveMode(verifier);
                        break;
                    case ModeType.Normal: modeStrategy = new NormalMode(verifier, (int)cfg.GetParameter("wait_time"));
                        break;
                    case ModeType.Extreme: modeStrategy = new ExtremeMode(verifier,
                        (string)cfg.GetParameter("base_path"),
                        (int)cfg.GetParameter("wait_time"));
                        break;
                }
                commandStrategy = new ExchangeStrategy(cfg, command.guid, modeStrategy, command.releaseUpdate);
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
