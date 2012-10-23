using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Core;

namespace Ugoria.URBD.CentralService.Scheduler
{
    class ExchangeJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // разобрать входящие параметры, сформировать объект Command и послать его. учитывать разные типы и т.п. параметры брать из context.merged
            JobDataMap dataMap = context.MergedJobDataMap;

            Action<int, CommandType, ModeType, int> action = (Action<int, CommandType, ModeType, int>)dataMap["action"];
            IConfiguration cfg = (IConfiguration)dataMap["cfg"];
            ModeType mode = ModeType.Normal;
            switch ((string)cfg.GetParameter("mode"))
            {
                case "A": mode = ModeType.Aggresive; break;
                case "E": mode = ModeType.Extreme; break;
                case "P": mode = ModeType.Passive; break;
            }

            int userId = (int)dataMap["user_id"];

            action.Invoke((int)cfg.GetParameter("base_id"),
                CommandType.Exchange,
                mode,
                userId); // отправка команды
        }
    }
}
