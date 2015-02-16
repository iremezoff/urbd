using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Shared;
using System.Data;
using System.Net.Mail;
using System.Net;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.CentralService.DataProvider;
using Ugoria.URBD.CentralService.Logging;


namespace Ugoria.URBD.CentralService.Alarming
{
    public class Alarmer : IAlarmer
    {
        private IConfiguration configuration;

        public Alarmer(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Alarm(string text)
        {
            Alarm(Guid.Empty, text);
        }

        public void Alarm(Guid reportGuid, string text)
        {
            string message = string.Format("{0}<br><br>--<br>С уважением, <br>Бот рассылки сообщений<br>сервиса управления распределенными базами данных 1С<br>Департамента эксплуатации<br>Дирекции Информационных Технологий", text);
            string smsMessage = "Сервис УРБД. " + text;

            // Mail Cfg
            SmtpClient client = new SmtpClient((string)configuration.GetParameter("main.mail_address"), int.Parse((string)configuration.GetParameter("main.mail_port")));
            client.Credentials = new NetworkCredential((string)configuration.GetParameter("mail_username"), (string)configuration.GetParameter("mail_password"));
            client.EnableSsl = false;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress((string)configuration.GetParameter("main.mail_sender"), (string)configuration.GetParameter("main.mail_name"), System.Text.Encoding.UTF8);
            mailMessage.Body = message.ToString();
            mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            mailMessage.Subject = "Сервис УРБД";
            mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;

            // Sms Cfg
            List<string> phoneNumberList = new List<string>();

            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                foreach (DataRow dataRow in dataProvider.GetAlarmList(reportGuid).Rows)
                {
                    if (((bool)dataRow["on_mail"]) && !string.IsNullOrEmpty(dataRow["mail"].ToString()))
                    {
                        mailMessage.To.Add(new MailAddress(dataRow["mail"].ToString()));                        
                    }
                    if (((bool)dataRow["on_phone"]) && !string.IsNullOrEmpty(dataRow["phone"].ToString()))
                        phoneNumberList.Add(dataRow["phone"].ToString());
                }
            }

            string throwMailMsg = string.Empty;
            if (mailMessage.To.Count > 0)
            {
                try
                {
                    client.Send(mailMessage);
                }
                catch (Exception ex)
                {
                    throwMailMsg = string.Format("Не удалось отправить письма на адреса: {0}. Текст ошибки: {1}",
                                string.Join(", ", mailMessage.To.ToList().ConvertAll<string>(m => m.Address)),
                                ex.Message);
                }
            }
            string throwSmsMsg = string.Empty;
            if (phoneNumberList.Count > 0)
            {
                try
                {
                    using (DBDataProvider dataProvider = new DBDataProvider())
                    {
                        dataProvider.SetPhoneMessage(phoneNumberList, smsMessage);
                    }
                }
                catch (Exception ex)
                {
                    throwSmsMsg = string.Format("Не удалось отправить сообщения на номера: {0}. Текст ошибки: {1}",
                            string.Join(", ", phoneNumberList),
                            ex.Message);
                }
            }
            IEnumerable<string> msgs = new string[] { throwMailMsg, throwSmsMsg }.Where(s => !string.IsNullOrEmpty(s));
            if (msgs.Count() > 0)
                throw new AlarmException(string.Join(".\r\n", msgs));
        }
    }
}
