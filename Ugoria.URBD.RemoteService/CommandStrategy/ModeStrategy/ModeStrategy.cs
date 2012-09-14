using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Data;
using System.IO;

namespace Ugoria.URBD.RemoteService.CommandStrategy.ModeStrategy
{
    public class ModeStrategy : IModeStrategy
    {
        private string logFilename = "";
        private List<MLGMessage> messageList = new List<MLGMessage>();
        protected string status = "";
        protected bool isVerified = false;

        public string Status
        {
            get { return status; }
        }

        public List<MLGMessage> Messages
        {
            get { return messageList; }
        }

        public bool IsVerified
        {
            get { return isVerified; }
        }

        public virtual bool Verification ()
        {
            IEnumerator<MLGMessage> mlgEnumerator = GetMlgEnumerator();

            bool isUplSuc = false;
            bool isDnldSuc = false;
            while (mlgEnumerator.MoveNext())
            {
                switch (mlgEnumerator.Current.eventType)
                {
                    case "DistrUplSuc":
                        isUplSuc = true;
                        break;
                    case "DistrDnldSuc":
                        isDnldSuc = true;
                        break;
                    case "DistrUplErr":
                        status = "Ошибка загрузки пакета. " + mlgEnumerator.Current.information;
                        return false;
                    case "DistrDnldErr":
                        status = "Ошибка выгрузки пакета. " + mlgEnumerator.Current.information;
                        return false;
                }
            }
            if (!isUplSuc)
                status = "В log-файле обмена 1С отсутствует запись об успешности загрузки пакета. ";
            if (!isDnldSuc)
                status = "В log-файле обмена 1С отсутствует запись об успешности выгрузки пакета. ";
            isVerified = isUplSuc && isDnldSuc;
            if (isVerified)
                status = "Обмен 1С прошел успешно. ";
            return true;
        }

        protected ModeStrategy (string logFilename)
        {
            this.logFilename = logFilename;
        }

        protected IEnumerator<MLGMessage> GetMlgEnumerator ()
        {
            using (StreamReader read = new StreamReader(new FileStream(logFilename, FileMode.Open), Encoding.GetEncoding(1251)))
            {
                string line = "";
                while ((line = read.ReadLine()) != null)
                {
                    MLGMessage mlgMessage = new MLGMessage();
                    //20070728;13:59:48;Администратор;C;Distr;DistDnldBeg;1;Код ИБ: 'СП ', Файл: 'F:\Base_urbd_sql\SPB\CP\SPB_C.zip';;
                    string[] messArr = line.Split(new char[] { ';' });
                    if (!"Distr".Equals(messArr[4])) // пропуск записей, не связанных с обменом
                        continue;
                    mlgMessage.eventDate = DateTime.ParseExact(messArr[0] + messArr[1], "yyyyMMddHH:mm:ss", null);
                    mlgMessage.eventType = messArr[5];
                    mlgMessage.information = messArr[7];
                    yield return mlgMessage;
                }
            }
        }
    }
}
