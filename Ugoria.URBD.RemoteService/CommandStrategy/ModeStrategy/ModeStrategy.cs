using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    abstract class ModeStrategy : IModeStrategy
    {
        private Verifier verifier;
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
            set { message = value; }
        }

        protected ModeStrategy(Verifier verifier)
        {
            this.verifier = verifier;
        }

        public virtual bool CompleteExchange()
        {
            verifier.Verification();
            List<string> errMessages = new List<string>();

            foreach (KeyValuePair<FileInfo, PacketInfo> mlgRecord in verifier.MlgReport)
            {
                if (!mlgRecord.Value.isSuccess && string.IsNullOrEmpty(mlgRecord.Value.status))
                {
                    isAborted = true;
                    isSuccess = false;
                    errMessages.Add(String.Format("{0} пакета {1} была прервана", mlgRecord.Value.type == Contracts.Services.PacketType.Load ? "Загрузка" : "Выгрузка", mlgRecord.Key.Name));
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
                errMessages.Add("Сеанс 1С был аварийно завершен");
            }
            message = string.Join(". ", errMessages).Replace("..", ".");
            if (errMessages.Count == 0)
                message = "Процесс обмена прошел успешно";
            return isSuccess;
        }
    }
}
