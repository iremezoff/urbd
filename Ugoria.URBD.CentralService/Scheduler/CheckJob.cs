using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Core;

namespace Ugoria.URBD.CentralService.Scheduler
{
    class CheckJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // разобрать входящие параметры, сформировать объект Command и послать его. учитывать разные типы и т.п. параметры брать из context.merged
            JobDataMap dataMap = context.MergedJobDataMap;

            Action<int, CommandType> action = (Action<int, CommandType>)dataMap["action"];
            IConfiguration cfg = (IConfiguration)dataMap["cfg"];

            action.Invoke(int.Parse((string)dataMap["base_id"]),
                CommandType.Exchange); // отправка команды
        }
    }
}
