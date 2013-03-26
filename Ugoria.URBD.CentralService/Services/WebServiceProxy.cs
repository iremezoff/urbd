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
using Ugoria.URBD.Contracts.Service;
using Ugoria.URBD.Contracts.Data.Reports;

namespace Ugoria.URBD.CentralService
{
    public class WebServiceProxy : IWebService, IDisposable
    {
        #region privates
        private int attempts = 3;
        private IWebService webService;

        private Exception exception;

        public Exception Exception
        {
            get { return exception; }
        }

        private ChannelFactory<IWebService> channelFactory;
        #endregion

        private ICommunicationObject commObj;

        private bool isSuccess;

        public bool IsSuccess
        {
            get { return isSuccess; }
        }

        public WebServiceProxy(ChannelFactory<IWebService> channelFactory)
        {
            this.channelFactory = channelFactory;
            webService = channelFactory.CreateChannel();
            commObj = (ICommunicationObject)webService;
        }

        private void RebuildService(Exception ex)
        {
            attempts--;
            Thread.Sleep(new TimeSpan(0, 0, 30));
            webService = channelFactory.CreateChannel();
            commObj = (ICommunicationObject)webService;
            exception = ex;
        }

        public void NotifyCommand(ExecuteCommand command)
        {
            while (attempts > 0)
            {
                try
                {
                    webService.NotifyCommand(command);
                    isSuccess = true;
                    return;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
        }

        public void NotifyReport(Report report)
        {
            while (attempts > 0)
            {
                try
                {
                    webService.NotifyReport(report);
                    isSuccess = true;
                    return;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
        }

        public void Dispose()
        {
            try
            {
                // Определить, односторонняя ли операция и вырубить соединение
                if (commObj.State == CommunicationState.Opened)
                  commObj.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}
