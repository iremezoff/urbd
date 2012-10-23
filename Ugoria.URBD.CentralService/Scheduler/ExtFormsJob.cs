using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data.Commands;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Core;

namespace Ugoria.URBD.CentralService
{
    class ExtFormsJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // разобрать входящие параметры, сформировать объект Command и послать его. учитывать разные типы и т.п. параметры брать из context.merged
            JobDataMap dataMap = context.MergedJobDataMap;

            Action<int,  CommandType, ModeType, bool, int> action = (Action<int, CommandType, ModeType, bool, int>)dataMap["action"];
            IConfiguration cfg = (IConfiguration)dataMap["cfg"];

            int userId = (int)dataMap["user_id"];

            action.Invoke((int)cfg.GetParameter("base_id"),
                CommandType.ExtForms,
                ModeType.Passive,
                false,
                userId); // отправка команды
        }
    }
}
