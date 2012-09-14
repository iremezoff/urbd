using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quartz;
using Ugoria.URBD.Contracts;
using Ugoria.URBD.Contracts.Data.Commands;

namespace Ugoria.URBD.CentralService
{
    class CommandJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            // разобрать входящие параметры, сформировать объект Command и послать его. учитывать разные типы и т.п. параметры брать из context.merged
            JobDataMap dataMap = context.MergedJobDataMap;
            /*Command command = new Command();
            command.baseName = (string)dataMap["base_name"];
            command.commandType = (CommandType)dataMap["type"]; // тип команды: обмен либо ExtForms
            command.commandDate = DateTime.Now;

            if (command.commandType == CommandType.Exchange)
            {
                command.modeType = (ModeType)dataMap["mode"];
                command.withMD = (bool)dataMap["with_md"];
            }*/

            Action<Command> action = (Action<Command>)dataMap["action"];
            Command command = (Command)dataMap["command"];

            action.Invoke(command); // отправка команды
        }
    }
}
