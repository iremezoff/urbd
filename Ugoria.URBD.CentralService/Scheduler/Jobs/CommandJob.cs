using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.CentralService.CommandBuilding;

namespace Ugoria.URBD.CentralService.Scheduler
{
    class CommandJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // разобрать входящие параметры, сформировать объект Command и послать его. учитывать разные типы и т.п. параметры брать из context.merged
            JobDataMap dataMap = context.MergedJobDataMap;

            Action<CommandBuilder, int> action = (Action<CommandBuilder, int>)dataMap["action"];
            CommandBuilder builder = (CommandBuilder)dataMap["command_builder"];
            
            int userId = (int)dataMap["user_id"];
            action.Invoke( builder, userId); // отправка команды
        }
    }
}
