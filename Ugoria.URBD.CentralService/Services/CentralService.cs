using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using Ugoria.URBD.Contracts.Data.Reports;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.CentralService
{
    public delegate RemoteConfiguration RemoteServiceHandler (ICentralService sender, RequestConfigureEventArgs args);
    public delegate void RemoteNoticePID1CHandler (ICentralService sender, NoticePID1CArgs args);
    public delegate void RemoteNoticeReportHandler (ICentralService sender, NoticeReportArgs args);

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    class CentralService : ICentralService
    {
        #region events
        public event RemoteServiceHandler RemoteRequestConfiguration;
        public event RemoteNoticePID1CHandler RemoteNoticePID1C;
        public event RemoteNoticeReportHandler RemoteNoticeReport;
        #endregion

        public void NoticePID1C (LaunchReport launchReport, Uri address)
        {
            if (RemoteNoticePID1C != null)
                RemoteNoticePID1C(this, new NoticePID1CArgs(launchReport, address));
        }

        public void NoticeReport (OperationReport report, Uri address)
        {
            if (RemoteNoticeReport != null)
                RemoteNoticeReport(this, new NoticeReportArgs(report, address));
        }

        public RemoteConfiguration RequestConfiguration (Uri address)
        {
            RemoteEndpointMessageProperty property = (RemoteEndpointMessageProperty)OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            if (RemoteRequestConfiguration != null)
                return RemoteRequestConfiguration(this, new RequestConfigureEventArgs(address));
            return null;            
        }
    }

    public class RequestConfigureEventArgs : EventArgs
    {
        private Uri uri;

        public Uri Uri
        {
            get { return uri; }
        }

        internal RequestConfigureEventArgs (Uri uri)
        {
            this.uri = uri;
        }
    }

    public class NoticePID1CArgs : EventArgs
    {
        private LaunchReport launchReport;

        public LaunchReport LaunchReport
        {
            get { return launchReport; }
        }

        private Uri uri;

        public Uri Uri
        {
            get { return uri; }
        }

        internal NoticePID1CArgs (LaunchReport report, Uri uri)
        {
            this.launchReport = report;
            this.uri = uri;
        }
    }

    public class NoticeReportArgs : EventArgs
    {
        private OperationReport report;

        public OperationReport Report
        {
            get { return report; }
        }

        private Uri uri;

        public Uri Uri
        {
            get { return uri; }
        }

        internal NoticeReportArgs(OperationReport report, Uri uri)
        {
            this.report = report;
            this.uri = uri;
        }
    }
}
