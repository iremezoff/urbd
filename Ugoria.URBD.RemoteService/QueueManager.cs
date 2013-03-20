using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Handlers;

namespace Ugoria.URBD.RemoteService
{
    class QueueExecuteManager
    {
        private RemoteConfigurationManager configurationManager;
        private List<ExecuteCommand> queueTasks;
        private SynchronizedCollection<TaskExecute> executedProcess;
        public event ReportAsyncCallback TaskExecuted;

        public int ExecutedProcessCount
        {
            get { return executedProcess.Count; }
        }
        private object lockConfManager = new object();
        private MessageHandler messageHandler;

        public MessageHandler MessageHandler
        {
            get { return messageHandler; }
            set { messageHandler = value; }
        }

        public RemoteConfigurationManager ConfigurationManager
        {
            get { return configurationManager; }
            set
            {
                lock (lockConfManager)
                {
                    configurationManager = value;
                }
            }
        }

        public QueueExecuteManager(RemoteConfigurationManager cfgMngr = null)
        {
            this.configurationManager = cfgMngr;
            this.queueTasks = new List<ExecuteCommand>();
            this.executedProcess = new SynchronizedCollection<TaskExecute>();
            Init();
        }

        private void Init() { }

        public bool IsBusyBase(int baseId)
        {
            return executedProcess.Any(p => p.Command.baseId == baseId);
        }

        public bool IsProcessLaunch(Guid reportGuid)
        {
            return executedProcess.Any(p => p.Command.reportGuid == reportGuid);
        }

        public bool IsFromQueue(Guid reportGuid)
        {
            lock (queueTasks)
            {
                return queueTasks.Any(t => t.reportGuid == reportGuid);
            }
        }

        public void KillTask(ExecuteCommand interruptCommand, ReportAsyncCallback callback)
        {
            ICommandStrategy strategy = BuildCommandStrategy(interruptCommand);

            lock (queueTasks)
            {
                LogHelper.Write2Log(String.Format("Поиск в очереди задачи для ИБ ", interruptCommand.reportGuid, interruptCommand.baseName), LogLevel.Information);
                ExecuteCommand command = queueTasks.FirstOrDefault(t => t.reportGuid == interruptCommand.reportGuid);
                if (command != null)
                {
                    LogHelper.Write2Log(String.Format("Найдена задача {0} для ИБ {1}. Попытка удаления", interruptCommand.reportGuid, interruptCommand.baseName), LogLevel.Information);
                    strategy.Interrupt();
                    OperationReport operationReport = messageHandler.GetOperationReport(command, strategy);
                    operationReport.message = "Задача исключена из очереди";
                    callback(operationReport);
                    ChangeQueue();
                    return;
                }
            }
            LogHelper.Write2Log(String.Format("Поиск в выполняющихся процессах {0} для ИБ {1}", interruptCommand.reportGuid, interruptCommand.baseName), LogLevel.Information);
            TaskExecute task = executedProcess.FirstOrDefault(p => p.Command.reportGuid == interruptCommand.reportGuid);
            if (task != null)
            {
                LogHelper.Write2Log(String.Format("Найден процесс задачи {0} для ИБ {1}. Попытка снятия", interruptCommand.reportGuid, interruptCommand.baseName), LogLevel.Information);
                task.CommandStrategy.Interrupt();

                callback(messageHandler.GetOperationReport(task.Command, task.CommandStrategy));
                executedProcess.Remove(task);
                ChangeQueue();
                return;
            }

            strategy.Interrupt();
            OperationReport unknownReport = messageHandler.GetOperationReport(interruptCommand, strategy);
            LogHelper.Write2Log(String.Format("Задача {0} для ИБ {1} потеряна, ничего не снято", interruptCommand.reportGuid, interruptCommand.baseName), LogLevel.Information);
            unknownReport.message = "Задача отсутствовала в очереди и не была запущена";
            callback(unknownReport);
        }

        private void ChangeQueue()
        {
            LogHelper.Write2Log("Изменение очереди", LogLevel.Information);
            if (configurationManager == null) // задачи будут копиться до тех пор, пока не появится конфигурация
                return;
            // убивать процессы больше суток <--параметр из БД
            lock (queueTasks)
            {
                LogHelper.Write2Log("Проверка наличия задач в очереди", LogLevel.Information);
                if (queueTasks.Count == 0)
                    return; // нет задач - нет запусков
                LogHelper.Write2Log("Задачи есть", LogLevel.Information);
                IConfiguration mainCfg = null;
                lock (lockConfManager)
                {
                    mainCfg = configurationManager.GetMainConfiguration();
                }
                int maxThreads = int.Parse((string)mainCfg.GetParameter("main.max_threads"));

                lock (executedProcess.SyncRoot)
                {
                    LogHelper.Write2Log("Поиск свободной очереди", LogLevel.Information);
                    // проверка, есть ли свободные потоки в каждом из пулов. Если занять хотя бы один пул, задача не будет выполнена
                    ExecuteCommand waitingTask = queueTasks.FirstOrDefault(
                        t => t.pools.Max( // прогон по ожидающим заданиям
                            p => executedProcess.Count( // прогон по выполняющимся заданиям, занимающим определенные пулы
                                e => e.Command.pools.Any(p2 => p2 == p))) < maxThreads // превышение хотя бы по одному пулу не дает возможность задаче выполниться
                        );
                    // баз не найдено
                    if (waitingTask == null)
                        return;
                    LogHelper.Write2Log("Есть свободные потоки в пулах для ИБ " + waitingTask.baseName, LogLevel.Information);
                    queueTasks.Remove(waitingTask);
                    ExecuteCommand command = waitingTask;

                    ICommandStrategy commandStrategy = BuildCommandStrategy(command);

                    TaskExecute taskExecute = new TaskExecute(command, commandStrategy);
                    executedProcess.Add(taskExecute);

                    Action<TaskExecute> taskLaunch = TaskLaunch;
                    taskLaunch.BeginInvoke(taskExecute, r => taskLaunch.EndInvoke(r), null);
                }
            }
        }

        // Wintellect.Threading AsyncEnumerator, не плодим методы обратного вызова
        private void TaskLaunch(TaskExecute task)
        {
            // Запуск процесса
            LogHelper.Write2Log(String.Format("Запуск процесса {0}. ИБ: {1}. Время команды: {2:HH:mm:ss dd.MM.yyyy}. Тип: {3}", task.Command.reportGuid, task.Command.baseName, task.Command.commandDate, task.Command, task.Command.GetType()), LogLevel.Information);
            //ICommandStrategy strategy = task.CommandStrategy;

            ICommandStrategy strategy = task.CommandStrategy;
            string message = string.Empty;
            try
            {
                strategy.Prepare();
                bool launchComplete = false;
                while (!launchComplete && !strategy.IsInterrupt)
                {
                    strategy.StartLaunch();
                    if(TaskExecuted!=null)
                    TaskExecuted(messageHandler.GetLaunchReport(task.Command, task.CommandStrategy));

                    launchComplete = strategy.EndLaunch();
                }
                strategy.Conclusion(); // зачистку и сбор информации осуществляем в любом случае
            }
            catch (URBDException ex)
            {
                message = ex.Message;
            }
            catch (Exception ex)
            {
                message = ex.GetType() + ": " + ex.Message;
                LogHelper.Write2Log(ex);
            }
            OperationReport report = messageHandler.GetOperationReport(task.Command, strategy);
            report.message = string.Join(". ", new string[] { report.message, message }.Where(s => !string.IsNullOrEmpty(s)));
            if(TaskExecuted!=null)
            TaskExecuted(report);

            LogHelper.Write2Log(String.Format("Процесс {0} для ИБ {1} завершен", task.Command.reportGuid, task.Command.baseName), LogLevel.Information);

            // Удаляем из списка работающих процессов и сообщаем об изменении очереди, если конечно процесс не был прерван
            if (task.CommandStrategy.IsInterrupt)
                return;
            executedProcess.Remove(task);
            ChangeQueue();
        }

        private ICommandStrategy BuildCommandStrategy(Command command)
        {
            IConfiguration cfg = null;
            lock (lockConfManager)
            {
                cfg = configurationManager.GetBaseConfiguration(command.baseId);
            }
            ICommandStrategy commandStrategy = messageHandler.GetStrategy(cfg, command);
            return commandStrategy;
        }

        public void AddOperation(ExecuteCommand command)
        {
            lock (queueTasks)
            {
                queueTasks.Add(command);
            }
            ChangeQueue();
        }
    }

    class TaskExecute
    {
        private ExecuteCommand command;
        private ICommandStrategy commandStrategy;

        public ExecuteCommand Command
        {
            get { return command; }
        }

        public ICommandStrategy CommandStrategy
        {
            get { return commandStrategy; }
        }

        internal TaskExecute(ExecuteCommand command, ICommandStrategy commandStrategy)
        {
            this.command = command;
            this.commandStrategy = commandStrategy;
        }
    }
}
