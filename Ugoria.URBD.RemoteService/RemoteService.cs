using System;
using Ugoria.URBD.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data;
using System.Net.Sockets;

namespace Ugoria.URBD.RemoteService
{
    public delegate void ResetConfigureHandler(IRemoteService sender, EventArgs args);
    public delegate void CommandSendedHandler(IRemoteService sender, CommandEventArgs args);
    public delegate RemoteProcessStatus CheckProcessHandler(IRemoteService sender, CheckEventArgs args);
    public delegate void InterruptProcessHandler(IRemoteService sender, InterruptEventArgs args);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)] // единственный экземпляр на все входящие подключения и последовательная обработка входящих команд
    class RemoteService : IRemoteService
    {
        public event ResetConfigureHandler Configured;
        public event CommandSendedHandler CommandSended;
        public event CheckProcessHandler ProcessChecked;
        public event InterruptProcessHandler Interrupted;

        private Uri localUri;
        private Uri centralUri;

        public Uri CentralUri
        {
            get { return centralUri; }
        }

        public Uri LocalUri
        {
            get { return localUri; }
        }

        public void ResetConfiguration()
        {
            localUri = OperationContext.Current.IncomingMessageProperties.Via;
            RemoteEndpointMessageProperty property = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            centralUri = new Uri(String.Format("net.tcp://{0}:8000/URBDCentralService", property.Address));
            if (Configured != null)
                Configured(this, new EventArgs());
        }

        public void CommandExecute(Command command)
        {
            localUri = OperationContext.Current.IncomingMessageProperties.Via;
            RemoteEndpointMessageProperty property = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            centralUri = new Uri(String.Format("net.tcp://{0}:8000/URBDCentralService", property.Address));
            if (CommandSended != null)
                CommandSended(this, new CommandEventArgs(command));
        }

        public RemoteProcessStatus CheckProcess(CheckCommand command)
        {
            localUri = OperationContext.Current.IncomingMessageProperties.Via;
            RemoteEndpointMessageProperty property = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            centralUri = new Uri(String.Format("net.tcp://{0}:8000/URBDCentralService", property.Address)); 
            if (ProcessChecked != null)
                return ProcessChecked(this, new CheckEventArgs(command));
            return RemoteProcessStatus.UnknownFail;
        }

        public void InterruptProcess(Guid reportGuid)
        {
            localUri = OperationContext.Current.IncomingMessageProperties.Via;
            RemoteEndpointMessageProperty property = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            centralUri = new Uri(String.Format("net.tcp://{0}:8000/URBDCentralService", property.Address));
            if (Interrupted != null)
                Interrupted(this, new InterruptEventArgs(reportGuid));
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

    public class InterruptEventArgs : EventArgs
    {
        private Guid guid;

        public Guid CommandGuid
        {
            get { return guid; }
        }

        internal InterruptEventArgs(Guid guid)
        {
            this.guid = guid;
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
