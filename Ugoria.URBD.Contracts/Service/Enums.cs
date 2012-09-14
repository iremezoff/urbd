using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.Contracts.Service
{
    public enum CommandType { Exchange, ExtForms, Checker };
    public enum ModeType { Aggresive, Passive, Extreme, Normal };
    public enum PacketType { Load = 0, Unload = 1 };
    public enum ReportStatus { ExchangeSuccess, ExtFormsSuccess, ExchangeFail, ExtFormsFail, ExchangeWarning };

    public enum RemoteProcessStatus : uint
    {
        UnknownFail = 0x0,
        Miss = 0x1000,
        LongProcess = 0x2000,
        ServiceFail = 0x3000,
        LongStart = 0x4000
    }
}
