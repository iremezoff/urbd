using System;
using Ugoria.URBD.CentralService;
using System.ServiceModel;

namespace Ugoria.URBD.Shared.Services
{
    public delegate void NoticeHandler(RemoteServiceProxy sender, NoticeEventArgs e);
    public class NoticeEventArgs : EventArgs
    {
        private Code code = Code.FaultFail;
        private string message = "";

        public Code Code
        {
            get { return code; }
        }

        public string Message
        {
            get { return message; }
        }

        internal NoticeEventArgs(Code code, string message)
        {
            this.code = code;
            this.message = message;
        }

        internal NoticeEventArgs(string message)
        {
            this.code = Code.Success;
            this.message = message;
        }

        internal NoticeEventArgs(Exception ex)
        {
            if (ex is FaultException)
                this.code = Code.FaultFail;
            else if (ex is CommunicationException)
                this.code = Code.CommunicationFail;
            message = ex.Message;
        }
    }
}
