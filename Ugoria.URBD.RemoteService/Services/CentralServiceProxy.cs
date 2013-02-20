using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data.Reports;
using System.ServiceModel;
using Ugoria.URBD.Contracts.Data;
using System.Threading;

namespace Ugoria.URBD.RemoteService.Services
{
    class CentralServiceProxy : ICentralService, IDisposable
    {
        private ChannelFactory<ICentralService> channelFactory;
        private EndpointAddress endpointAddr;
        private ICommunicationObject commObj;
        private int attempts = 3;
        private bool isSuccess;

        private Exception exception;

        public Exception Exception
        {
            get { return exception; }
        }

        public bool IsSuccess
        {
            get { return isSuccess; }
        }

        private ICentralService centralService;

        public CentralServiceProxy(ChannelFactory<ICentralService> channelFactory, EndpointAddress endpointAddr)
        {
            this.channelFactory = channelFactory;
            this.endpointAddr = endpointAddr;
            centralService = channelFactory.CreateChannel(endpointAddr);
            commObj = (ICommunicationObject)centralService;
        }

        public void NoticePID1C(LaunchReport launchReport, Uri address)
        {
            while (attempts > 0)
            {
                try
                {
                    centralService.NoticePID1C(launchReport, address);
                    isSuccess = true;
                    return;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
        }

        private void RebuildService(Exception ex)
        {
            attempts--;
            Thread.Sleep(new TimeSpan(0, 0, 30));
            centralService = channelFactory.CreateChannel(endpointAddr);
            commObj = (ICommunicationObject)centralService;
            exception = ex;
        }

        public void NoticeReport(OperationReport report, Uri address)
        {
            while (attempts > 0)
            {
                try
                {
                    centralService.NoticeReport(report, address);
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
                // определить, односторонняя ли операция
                if (commObj.State == CommunicationState.Opened)
                    commObj.Close();
            }
            catch (Exception)
            {
            }
        }

        public RemoteConfiguration RequestConfiguration(Uri address)
        {
            while (attempts > 0)
            {
                try
                {
                    RemoteConfiguration conf = centralService.RequestConfiguration(address);
                    isSuccess = true;
                    return conf;
                }
                catch (Exception ex)
                {
                    RebuildService(ex);
                }
            }
            return null;
        }
    }
}
