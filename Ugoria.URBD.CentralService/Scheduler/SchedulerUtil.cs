using System;
using Ugoria.URBD.Contracts.Services;

namespace Ugoria.URBD.CentralService
{
    static class SchedulerUtil
    {
        public static string CronExpressionBuild(string time)
        {
            string[] timeArr = time.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            string retStr = "0 0 0 * * ?";
            switch (timeArr.Length)
            {
                case 2:
                    retStr = String.Format("0 {0} {1} * * ?", timeArr[1], timeArr[0]);
                    break;
                case 3:
                    retStr = String.Format("{0} {1} {2} * * ?", timeArr[2], timeArr[1], timeArr[0]);
                    break;
            }
            return retStr;
        }

        public static string CronNameBuild(string jobName, string time, ModeType mode, bool withMD)
        {
            return String.Format("{0}_{1}_{2}_{3}", jobName, time, mode.ToString(), (withMD ? "withMD" : "withoutMD"));
        }

        public static string CronNameBuild(string jobName, string time)
        {
            return String.Format("{0}_{1}", jobName, time);
        }

        public static string CronNameBuild(string jobName)
        {
            return String.Format("{0}_checker", jobName);
        }
    }
}
