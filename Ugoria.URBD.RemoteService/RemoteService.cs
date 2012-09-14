using System;
using Ugoria.URBD.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.RemoteService
{
    public delegate void RegisteredHandler(IRemoteService sender, RegisteredEventArgs args);
    public delegate void ConfiguredHandler(IRemoteService sender, ConfigureEventArgs args);
    public delegate void CommandSendedHandler(IRemoteService sender, CommandEventArgs args);
    public delegate RemoteProcessStatus CheckProcessHandler(IRemoteService sender, CheckEventArgs args);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)] // единственный экземпляр на все входящие подключения и последовательная обработка входящих команд
    class RemoteService : IRemoteService
    {
        public event RegisteredHandler Registered;
        public event ConfiguredHandler Configured;
        public event CommandSendedHandler CommandSended;
        public event CheckProcessHandler ProcessChecked;

        public void Configure(RemoteConfiguration configuration)
        {
            Console.WriteLine("конфигурирование сервиса");
            if (Configured != null)
                Configured(this, new ConfigureEventArgs(configuration));
        }

        public void CommandExecute(Command command)
        {
            Console.WriteLine("послана команда");
            if (CommandSended != null)
                CommandSended(this, new CommandEventArgs(command));
        }

        public void RegisterCentralService(Uri callbackUri)
        {
            RemoteEndpointMessageProperty property = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            Console.WriteLine("регистрация сервиса");
            string ctor = String.Format("{0}://{1}:{2}{3}", callbackUri.Scheme, property.Address, callbackUri.Port, callbackUri.LocalPath);
            if (Registered != null)
                Registered(this, new RegisteredEventArgs(new Uri(ctor), OperationContext.Current.IncomingMessageProperties.Via));
        }

        public RemoteProcessStatus CheckProcess(CheckCommand command)
        {
            Console.WriteLine("проверка процесса");
            if (ProcessChecked != null)
                return ProcessChecked(this, new CheckEventArgs(command));
            return RemoteProcessStatus.UnknownFail;
        }
    }

    public class ConfigureEventArgs : EventArgs
    {
        private RemoteConfiguration configuration;

        public RemoteConfiguration Configuration
        {
            get { return configuration; }
        }

        internal ConfigureEventArgs(RemoteConfiguration configuration)
        {
            this.configuration = configuration;
        }
    }

    public class CommandEventArgs : EventArgs
    {
        private Command command;

        public Command Command
        {
            get { return command; }
        }

        internal CommandEventArgs(Command command)
        {
            this.command = command;
        }
    }

    public class CheckEventArgs : EventArgs
    {
        private CheckCommand checkCommand;

        public CheckCommand CheckCommand
        {
            get { return checkCommand; }
        }

        internal CheckEventArgs(CheckCommand checkCommand)
        {
            this.checkCommand = checkCommand;
        }
    }

    public class RegisteredEventArgs : EventArgs
    {
        private Uri centralUri;
        private Uri localUri;

        public Uri CentralUri
        {
            get { return centralUri; }
        }        

        public Uri LocalUri
        {
            get { return localUri; }
        }

        internal RegisteredEventArgs(Uri centralUri, Uri localUri)
        {
            this.centralUri = centralUri;
            this.localUri = localUri;
        }
    }
}
