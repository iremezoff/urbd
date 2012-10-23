using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Core;
using System.Data;
using System.Net.Mail;
using System.Net;


namespace Ugoria.URBD.CentralService
{
    public class Alarmer : IAlarmer
    {
        private IDataProvider dataProvider;
        private IConfiguration configuration;
        private ILogger logger;

        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public Alarmer(IDataProvider dataProvider, IConfiguration configuration)
        {
            this.dataProvider = dataProvider;
            this.configuration = configuration;
        }

        public void Alarm(string serviceAddr, string text)
        {
            string message = string.Format("{0}\r\n\r\n--\r\n Бот рассылки УРБД 2.0\r\n", text);

            SmtpClient client = new SmtpClient((string)configuration.GetParameter("mail_address"), int.Parse((string)configuration.GetParameter("mail_port")));
            NetworkCredential credential = new NetworkCredential((string)configuration.GetParameter("mail_username"), (string)configuration.GetParameter("mail_password"));
            client.Credentials = credential;
            client.EnableSsl = false;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress((string)configuration.GetParameter("mail_sender"), (string)configuration.GetParameter("mail_name"), System.Text.Encoding.UTF8);
            mailMessage.Body = message.ToString();
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.Subject = "Автообмен УРБД";
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;

            foreach (DataRow dataRow in dataProvider.GetAlarmList(serviceAddr).Rows)
                mailMessage.To.Add(new MailAddress(dataRow["mail"].ToString()));

            if (mailMessage.To.Count == 0)
                return;

            try
            {
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                if (logger != null)
                    logger.Fail(serviceAddr,
                        string.Format("Не удалось отправить письма на адреса: {0}. Текст ошибки: {1}",
                            string.Join(", ", mailMessage.To.ToList().ConvertAll<string>(m => m.Address)),
                            ex.Message));
            }
        }

        public void Alarm(Uri serviceUri, string text)
        {
            Alarm(Uri2AddressString(serviceUri), text);
        }

        private static string Uri2AddressString(Uri uri)
        {
            return String.Format("{0}:{1}", uri.Host, uri.Port);
        }
    }
}
