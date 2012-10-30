using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data;
using Ugoria.URBD.Contracts.Data.Commands;
using System.Net.Sockets;
using System.Threading;

namespace Ugoria.URBD.CentralService
{
    public enum Code { FaultFail, CommunicationFail, Success, MissProcess };

    public class RemoteServiceProxy : IRemoteService, IDisposable
    {
        #region privates
        private int attempts = 3;
        private IRemoteService remoteService;

        private Exception exception;

        public Exception Exception
        {
            get { return exception; }
        }

        private EndpointAddress addr;
        private ChannelFactory<IRemoteService> channelFactory;
        #endregion

        private ICommunicationObject commObj;

        private bool isSuccess;

        public bool IsSuccess
        {
            get { return isSuccess; }
        }

        public RemoteServiceProxy (ChannelFactory<IRemoteService> channelFactory, EndpointAddress addr)
        {
            this.channelFactory = channelFactory;
            this.addr = addr;
            remoteService = channelFactory.CreateChannel(addr);
            commObj = (ICommunicationObject)remoteService;
        }

        private void RebuildService (Exception ex)
        {
            attempts--;
            Thread.Sleep(new TimeSpan(0,0,30));
            remoteService = channelFactory.CreateChannel(addr);
            commObj = (ICommunicationObject)remoteService;
            exception = ex;
        }

        public void CommandExecute (Command command)
        {
            while (attempts > 0)
            {
                try
                {
                    remoteService.CommandExecute(command);
                    isSuccess = true;
                    return;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
        }

        public RemoteProcessStatus CheckProcess (CheckCommand checkCommand)
        {
            while (attempts > 0)
            {
                try
                {
                    RemoteProcessStatus status = remoteService.CheckProcess(checkCommand);
                    commObj.Close();
                    isSuccess = true;
                    return status;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
            return RemoteProcessStatus.UnknownFail;
        }

        public void Dispose ()
        {
            try
            {
                // Определить, односторонняя ли операция и вырубить соединение
                //if (commObj.State == CommunicationState.Opened)
                //  commObj.Close();
            }
            catch (Exception)
            {
            }
        }

        public void InterruptProcess(Guid commandGuid)
        {
            while (attempts > 0)
            {
                try
                {
                    remoteService.InterruptProcess(commandGuid);
                    isSuccess = true;
                    return;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
        }
    }
}
