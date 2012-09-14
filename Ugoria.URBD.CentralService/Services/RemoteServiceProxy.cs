using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService
{
    public delegate void NoticeHandler (RemoteServiceProxy sender, NoticeEventArgs e);
    public delegate void CommandSendedHandler (RemoteServiceProxy sender, SendCommandEventArgs e);
    public enum Code { FaultFail, CommunicationFail, Success, MissProcess };

    public class RemoteServiceProxy : IRemoteService
    {
        #region privates
        private IRemoteService remoteService;
        private int attemptCount = 3;
        private object objLock = new object(); // для предотвращения одновременных отправок нескольких команд

        private Uri addr;
        private ChannelFactory<IRemoteService> channelFactory;
        #endregion

        #region events
        public event NoticeHandler Event;// = new NoticeHandler((a1, a2) => { });
        public event CommandSendedHandler CommandSended;
        #endregion

        #region Properties
        public IRemoteService RemoteService
        {
            get { return remoteService; }
            set { remoteService = value; }
        }

        public int AttemptCount
        {
            get { return attemptCount; }
        }
        #endregion

        public RemoteServiceProxy (Uri addr, ChannelFactory<IRemoteService> channelFactory)
        {
            this.addr = addr;
            this.channelFactory = channelFactory;
            remoteService = channelFactory.CreateChannel(new EndpointAddress(addr));
        }

        private object Invoke (Delegate operation, params object[] args)
        {
            lock (objLock)
            {
                while (attemptCount > 0)
                {
                    try
                    {
                        return operation.DynamicInvoke(args);
                    }
                    catch (Exception ex)
                    {
                        attemptCount--;
                        if (attemptCount > 0)
                            remoteService = channelFactory.CreateChannel(new EndpointAddress(addr));
                        else
                            Event(this, new NoticeEventArgs(ex));
                    }
                }
            }
            return null;
        }

        public void Configure (RemoteConfiguration configuration)
        {
            Delegate operation = new Action<RemoteConfiguration>(conf =>
            {
                remoteService.Configure(conf);
                Event(this, new NoticeEventArgs("Конфигурирование сервиса"));
            });
            Invoke(operation);
        }

        public void CommandExecute (Command command)
        {
            command.commandDate = DateTime.Now; // команда подана
            command.guid = Guid.NewGuid();
            Delegate operation = new Action<Command>(cmd =>
            {
                remoteService.CommandExecute(cmd);
                // здесь также указать, какая база обновляется
                string message = String.Format("Запрос на {0}.\nИБ: {1}.\nРежим: {2}.\nMD: {3}.",
                    cmd.commandType == CommandType.Exchange ? "обмен пакетами" : "обновление ExtForms",
                    cmd.baseName,
                    cmd.commandType == CommandType.Exchange ? cmd.modeType.ToString() : "-",
                    cmd.commandType == CommandType.Exchange ? cmd.withMD.ToString() : "-");
                Event(this, new NoticeEventArgs(message));

                CommandSended(this, new SendCommandEventArgs(cmd));
            });

            Invoke(operation, command);
        }


        public void RegisterCentralService (Uri centralUri)
        {
            Delegate operation = new Action<Uri>(uri =>
            {
                remoteService.RegisterCentralService(uri);
                Event(this, new NoticeEventArgs("Сервис зарегистрирован"));
            });

            Invoke(operation, centralUri);
        }

        public RemoteProcessStatus CheckProcess (CheckCommand checkCommand)
        {
            Delegate operation = new Func<CheckCommand, RemoteProcessStatus>(cmd =>
            {
                return remoteService.CheckProcess(checkCommand);
                //Event(this, new NoticeEventArgs("Проверка сервиса"));
            });

            object status = Invoke(operation, checkCommand) ?? RemoteProcessStatus.UnknownFail;
            return (RemoteProcessStatus)status;
        }
    }
}
