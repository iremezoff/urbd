using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts.Handlers.Strategy;
using Ugoria.URBD.RemoteService.Kit;
using System.Net;
using Ugoria.URBD.Contracts.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Ugoria.URBD.RemoteService.Strategy
{
    public class MlgCollectStrategy : ICommandStrategy
    {
        private volatile bool isInterrupt = false;
        private bool isComplete = false;
        private MlgCollectContext context;

        private NetworkConnection netConn;

        public NetworkConnection NetConn
        {
            get { return netConn; }
        }

        public MlgCollectContext Context
        {
            get { return context; }
        }

        public bool IsInterrupt
        {
            get { return isInterrupt; }
        }

        public bool IsComplete
        {
            get { return isComplete; }
        }

        public MlgCollectStrategy(MlgCollectContext context)
        {
            this.context = context;
        }

        public void StartLaunch()
        {
            context.StartTime = DateTime.Now;
            context.LaunchGuid = Guid.NewGuid();
        }

        public bool EndLaunch()
        {
            int buffSize = 1024 * 10; // 10Kb
            FileInfo mlgFile = new FileInfo(string.Format(@"{0}\SYSLOG\1cv7.mlg", context.BasePath));
            long unreadByteCount = mlgFile.Length;
            byte[] buffer = new byte[buffSize];
            DateTime lastMessageDate = DateTime.MaxValue;
            StringBuilder lineCache = new StringBuilder();
            Encoding asciiEncoding = Encoding.GetEncoding(1251);

            using (FileStream stream = mlgFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                while (unreadByteCount > 0 && lastMessageDate > context.PrevDateLog && !isInterrupt)
                {
                    int offset = (unreadByteCount >= buffSize) ? buffSize : (int)unreadByteCount;
                    unreadByteCount -= offset;
                    stream.Seek(unreadByteCount, SeekOrigin.Begin);
                    stream.Read(buffer, 0, offset);
                    lineCache.Insert(0, asciiEncoding.GetString(buffer, 0, offset));

                    int lineStart = 0;
                    // Обработка строк из кеша, пока он полон и поканаходятся новые переносы строк (lineStart>0)
                    while ((lineStart = lineCache.GetLineStart()) >= 0 && lineCache.Length > 0)
                    {
                        string line = null;
                        // начало строки найдено, необходимо получить её и удалить из кеша (+-2 - это символы \r\n)
                        if (lineStart > 1)
                        {
                            line = lineCache.ToString(lineStart, lineCache.Length - lineStart);
                            lineCache.Remove(lineStart - 2, lineCache.Length - lineStart + 2);
                        }
                        // достигнуто начало файла
                        else if (lineStart == 0 && unreadByteCount == 0)
                        {
                            line = lineCache.ToString();
                            lineCache.Clear();
                        }
                        // начало очередной строки ещё не считанов кеш
                        else
                            break;
                        MlgMessage mlgMessage = BuildMlgMessage(line);
                        // бывает, что на новую строку переводятся данные с предыдущей, поэтому самостоятельной записью это не считается
                        if (mlgMessage == null)
                        {
                            if (!string.IsNullOrEmpty(line))
                                lineCache.Append(' ').Append(line.Trim());
                            continue;
                        }
                        lastMessageDate = mlgMessage.eventDate;
                        // на случай, если в кеш попали старые записи
                        if (lastMessageDate > context.PrevDateLog)
                        {
                            context.Messages.Push(mlgMessage);
                        }
                    }
                }
            }
            return true;
        }

        public void Interrupt()
        {
            isInterrupt = true;
        }

        public void Prepare()
        {
            if (new Uri(context.BasePath).IsUnc)
            {
                netConn = new NetworkConnection(context.BasePath, new NetworkCredential(context.Username, context.Password));
            }
        }

        public void Conclusion()
        {
            isComplete = true;
            if (netConn != null)
                netConn.Dispose();
        }

        private static Regex mlgLineRegex = new Regex(@"^([\p{L}\d]+)+/([\p{N}]+)+/(?:\(?([\p{L}\s]+)+\)?)*?([\p{N}]+)+$");

        private static MlgMessage BuildMlgMessage(string line)
        {
            MlgMessage mlgMessage = new MlgMessage();
            //20070728;13:59:48;Администратор;C;Distr;DistDnldBeg;1;Код ИБ: 'СП ', Файл: 'F:\Base_urbd_sql\SPB\CP\SPB_C.zip';;
            //20080602;17:30:58;Администратор;C;Distr;DistUplStatus;2;Изменен;B/133/(НВ )52090;Контрагенты НВ052137 Валиев Вадим Индусович
            string[] messArr = line.Split(new char[] { ';' });
            if (messArr.Length < 10) // пропуск записей с недостатком информации
                return null;
            mlgMessage.eventDate = DateTime.ParseExact(messArr[0] + messArr[1], "yyyyMMddHH:mm:ss", null);
            mlgMessage.eventType = messArr[5];
            mlgMessage.information = messArr[7];
            mlgMessage.eventGroup = messArr[4];
            mlgMessage.account = messArr[2];
            mlgMessage.mode1c = messArr[3];
            if ("Refs".Equals(messArr[4]) || "Docs".Equals(messArr[4]) || "Accs".Equals(messArr[4]) || ("DistUplStatus".Equals(messArr[5]) && ("2".Equals(messArr[6]) || "3".Equals(messArr[6]))))
            {
                Match match = mlgLineRegex.Match(messArr[8]);
                if (match.Success)
                {
                    mlgMessage.objectTypeCode = match.Groups[1].Value;
                    mlgMessage.objectTypeNumber = match.Groups[2].Value;
                    mlgMessage.baseCode = match.Groups[3].Value.Trim();
                    mlgMessage.objectIdentifier = match.Groups[4].Value;
                    mlgMessage.additional = messArr[9];
                }
            }
            messArr = null;
            return mlgMessage;
        }
    }

    public static class StringBuilderHelper
    {
        public static int GetLineStart(this StringBuilder builder)
        {
            for (int i = builder.Length - 1; i >= 0; i--)
            {
                if (builder[i] == '\n')
                    return i + 1;
            }
            return 0;
        }
    }
}
