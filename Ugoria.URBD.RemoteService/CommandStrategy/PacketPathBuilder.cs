using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ugoria.URBD.RemoteService
{
    class PacketPathBuilder
    {
        public static readonly int SOURCE = 0;
        public static readonly int DEST = 1;

        private string pcPath = "";
        private string packetName = "";
        private string ftpAddress = "";
        private string basePath = "";
        private string cpPath = "";

        public string PacketName
        {
            get { return packetName; }
            set { packetName = value; }
        }

        public string PCPath
        {
            get { return pcPath; }
            set { pcPath = value; }
        }

        public string CPPath
        {
            get { return cpPath; }
            set { cpPath = value; }
        }

        public string FtpAddress
        {
            get { return ftpAddress; }
            set { ftpAddress = value; }
        }

        public string BasePath
        {
            get { return basePath; }
            set { basePath = value; }
        }

        public string[] BuildLoadPaths()
        {
            string sourcePath = String.Format(@"{0}\{1}\{2}", ftpAddress, packetName.Contains("_C") ? cpPath : pcPath, packetName);
            string destPath = String.Format(@"{0}\{1}\{2}", basePath, packetName.Contains("_C") ? "CP" : "PC", packetName);

            return new string[] { sourcePath, destPath };
        }

        public string[] BuildUnloadPaths()
        {
            string sourcePath = String.Format(@"{0}\{1}\{2}", basePath, packetName.Contains("_C") ? "CP" : "PC", packetName);
            string destPath = String.Format(@"{0}\{1}\{2}", ftpAddress, packetName.Contains("_C") ? cpPath : pcPath, packetName);

            return new string[] { sourcePath, destPath };
        }
    }
}
