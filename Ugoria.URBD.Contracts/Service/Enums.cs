using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Contracts.Services
{
    public enum CommandType { Exchange, ExtForms, Checker };
    public enum ModeType { Passive='P', Normal='N', Aggresive='A', Extreme='E' };
    public enum PacketType { Load = 'L', Unload = 'U' };
    public enum ReportStatus { Interrupt, ExchangeSuccess, ExtFormsSuccess, ExchangeFail, ExtFormsFail, ExchangeWarning };

    public enum RemoteProcessStatus : uint
    {
        UnknownFail = 0x0,
        Miss = 0x1000,
        LongProcess = 0x2000,
        ServiceFail = 0x3000,
        LongStart = 0x4000
    }
}
