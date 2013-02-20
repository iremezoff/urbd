using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService.Scheduler
{
    class CheckJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // разобрать входящие параметры, сформировать объект Command и послать его. учитывать разные типы и т.п. параметры брать из context.merged
            JobDataMap dataMap = context.MergedJobDataMap;

            Action<Command> action = (Action<Command>)dataMap["action"];
            Command builder = (Command)dataMap["command"];

            action.Invoke(builder); // отправка команды
        }
    }
}
