using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Ugoria.URBD.Shared;
using System.Threading;
using Ugoria.URBD.Contracts.Handlers.Strategy.Exchange.Mode;

namespace Ugoria.URBD.RemoteService.Strategy.Exchange.Mode
{
    abstract class DefaultMode : IMode
    {
        private Verifier verifier;
        protected string basepath;
        protected int waitTime = 1;
        private static readonly string messageUrbdPattern = @"{0}\ExtForms\!md_message_urbd.txt";
        private bool attempt = false;

        private bool isAborted = false;

        public bool IsAborted
        {
            get { return isAborted; }
        }
        private bool isSuccess = true;

        public bool IsSuccess
        {
            get { return isSuccess; }
        }
        private bool isWarning = false;

        public bool IsWarning
        {
            get { return isWarning; }
        }

        public Verifier Verifier
        {
            get { return verifier; }
        }

        private string message = "";

        public string Message
        {
            get { return message; }
        }

        protected DefaultMode(Verifier verifier, string basepath, int waitTime)
        {
            this.verifier = verifier;
            this.basepath = basepath;
        }

        public virtual bool CompleteExchange(bool haveMD)
        {
            verifier.Verification();
            List<string> errMessages = new List<string>();

            isSuccess = true; // на случай, если производится повторый запуск проверки
            foreach (KeyValuePair<string, PacketInfo> mlgRecord in verifier.MlgReport)
            {
                if (!mlgRecord.Value.isSuccess && string.IsNullOrEmpty(mlgRecord.Value.status))
                {
                    isAborted = true;
                    isSuccess = false;
                    errMessages.Add(String.Format("{0} пакета {1} была прервана.", mlgRecord.Value.type == Contracts.Services.PacketType.Load ? "Загрузка" : "Выгрузка", mlgRecord.Key));
                }
                else if (!mlgRecord.Value.isSuccess && !string.IsNullOrEmpty(mlgRecord.Value.status))
                {
                    if (mlgRecord.Value.status.Contains("Данные из указанного файла переноса данных уже загружались в текущую информационную базу."))
                        isWarning = true;
                    else
                        isSuccess = false;
                    errMessages.Add(mlgRecord.Value.status);
                }
                else if (!mlgRecord.Value.isSuccess)
                    isSuccess = false;
            }
            // Сессия была завершена аварийно
            if (verifier.MlgReport.Count == 0)
            {
                isAborted = true;
                isSuccess = false;
                errMessages.Add("Ошибка доступа к метаданным (открыт конфигуратор) или иная причина.");
            }
            message = string.Join(" ", errMessages);
            if (errMessages.Count == 0)
                message = "Процесс обмена прошел успешно";

            // обмен не прошел, но есть MD, нужен новый запуск
            if (!isSuccess && haveMD && !attempt)
            {
                LogHelper.Write2Log("Создание файла с просьбой обновить релиз с MD", LogLevel.Information);
                // создание файла с просьбой
                using (StreamWriter sw = new StreamWriter(String.Format(messageUrbdPattern, basepath), false, Encoding.GetEncoding(1251)))
                {
                    sw.WriteLine("Требуется выполнить автообмен (новый релиз), закройте программу 1С");
                }
                LogHelper.Write2Log(String.Format("Повтор запуска через (мин): {0}", waitTime), LogLevel.Information);
                Thread.Sleep(new TimeSpan(0, waitTime, 0)); // спать 10 минут до следующей попытки
                attempt = true;
            }
            else
            {
                FileInfo messageFile = new FileInfo(String.Format(messageUrbdPattern, basepath));
                if (messageFile.Exists)
                    messageFile.Delete();
            }
            return isSuccess;
        }
    }
}
