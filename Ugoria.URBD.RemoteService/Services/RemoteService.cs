using System;
using Ugoria.URBD.Contracts;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data;
using System.Net.Sockets;

namespace Ugoria.URBD.RemoteService.Services
{
    public delegate void CommandSendedHandler(IRemoteService sender, CommandEventArgs args);
    public delegate RemoteProcessStatus CheckProcessHandler(IRemoteService sender, CheckEventArgs args);
    public delegate void InterruptProcessHandler(IRemoteService sender, InterruptEventArgs args);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)] // единственный экземпляр на все входящие подключения и последовательная обработка входящих команд
    class RemoteService : IRemoteService
    {
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
}
