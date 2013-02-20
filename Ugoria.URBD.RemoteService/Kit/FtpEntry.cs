using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService.Kit
{
    enum FtpEntryType { File, Directory }
    class FtpEntry
    {
        public FtpEntryType Type { get; set; }
        public Uri Uri { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
