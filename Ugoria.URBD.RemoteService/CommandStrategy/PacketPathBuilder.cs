using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.RemoteService
{
    class PacketPathBuilder
    {
        public static readonly int SOURCE = 0;
        public static readonly int DEST = 1;

        private string packetName = "";
        private string ftpAddress = "";
        private string basePath = "";

        private string ftpCP = "";

        public string FtpCP
        {
            get { return ftpCP; }
            set { ftpCP = value; }
        }
        private string ftpPC = "";

        public string FtpPC
        {
            get { return ftpPC; }
            set { ftpPC = value; }
        }

        public string PacketName
        {
            get { return packetName; }
            set { packetName = value; }
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

        public Uri[] BuildLoadPaths()
        {
            //string sourcePath = String.Format(@"{0}/{1}/{2}", ftpAddress, packetName.Replace("CP", "urbd/centr").Replace("PC", "urbd/peref"), packetName);
            StringBuilder strBldr = new StringBuilder();
            strBldr.AppendFormat("{0}/{1}", ftpAddress, packetName.ToUpper()).Replace("CP", ftpCP).Replace("PC", ftpPC); // подмена относительных путей
            strBldr.Replace(@"//", @"/", ftpAddress.Length - 1, strBldr.Length - ftpAddress.Length - 1).Replace(@"\/", @"/").Replace(@"/\", @"/"); // удаление лишних слешей
            Uri sourceUri = new Uri(strBldr.ToString());
            Uri destUri = new Uri(String.Format(@"{0}\{1}", basePath, packetName));

            return new Uri[] { sourceUri, destUri };
        }

        public Uri[] BuildUnloadPaths()
        {
            StringBuilder strBldr = new StringBuilder();
            strBldr.AppendFormat("{0}/{1}", ftpAddress, packetName.ToUpper()).Replace("CP", ftpCP).Replace("PC", ftpPC); // подмена относительных путей
            strBldr.Replace(@"//", @"/", ftpAddress.Length - 1, strBldr.Length - ftpAddress.Length - 1).Replace(@"\/", @"/").Replace(@"/\", @"/"); // удаление лишних слешей            
            Uri sourceUri = new Uri(String.Format(@"{0}\{1}", basePath, packetName));
            //string destPath = String.Format(@"{0}/{1}/{2}", ftpAddress, packetName.Replace("CP", "urbd/centr").Replace("PC", "urbd/peref"), packetName);
            Uri destUri = new Uri(strBldr.ToString());

            return new Uri[] { sourceUri, destUri };
        }
    }
}
