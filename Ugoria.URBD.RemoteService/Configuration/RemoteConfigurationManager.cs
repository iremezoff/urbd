using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using System.IO;
using Ugoria.URBD.Core;
using System.Collections;
using Ugoria.URBD.Contracts.Data;

namespace Ugoria.URBD.RemoteService
{
    class RemoteConfigurationManager
    {
        private RemoteConfiguration remoteConfiguration;

        public RemoteConfiguration RemoteConfiguration
        {
            get { return remoteConfiguration; }
            set { remoteConfiguration = value; }
        }

        public RemoteConfigurationManager(RemoteConfiguration remoteConfiguration = null)
        {
            this.remoteConfiguration = remoteConfiguration;
        }

        public IConfiguration GetMainConfiguration()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("ftp_username", remoteConfiguration.ftpUsername);
            hashtable.Add("ftp_password", remoteConfiguration.ftpPassword);
            hashtable.Add("ftp_address", remoteConfiguration.ftpAddress);
            hashtable.Add("1c_path", remoteConfiguration.file1CPath);
            hashtable.Add("extforms_path", remoteConfiguration.extFormsPath);
            hashtable.Add("max_threads", remoteConfiguration.threadsCount);
            hashtable.Add("cp_path", remoteConfiguration.cpPath);
            hashtable.Add("pc_path", remoteConfiguration.pcPath);
            hashtable.Add("wait_time", remoteConfiguration.waitTime);
            return new Configuration(hashtable);
        }

        public IConfiguration GetBaseConfiguration(string baseName)
        {
            Base baseConf = remoteConfiguration.baseList.First(b => b.baseName.Equals(baseName));

            Hashtable hashtable = new Hashtable();
            // main
            hashtable.Add("ftp_username", remoteConfiguration.ftpUsername);
            hashtable.Add("ftp_password", remoteConfiguration.ftpPassword);
            hashtable.Add("ftp_address", remoteConfiguration.ftpAddress);
            hashtable.Add("1c_path", remoteConfiguration.file1CPath);
            hashtable.Add("extforms_path", remoteConfiguration.extFormsPath);
            hashtable.Add("max_threads", remoteConfiguration.threadsCount);
            hashtable.Add("cp_path", remoteConfiguration.cpPath);
            hashtable.Add("pc_path", remoteConfiguration.pcPath);
            hashtable.Add("wait_time", remoteConfiguration.waitTime);
            hashtable.Add("packet_exchange_wait_time", remoteConfiguration.packetExchangeWaitTime);
            hashtable.Add("packet_exchange_attempts", remoteConfiguration.packetExchangeAttempts);
            // individual
            hashtable.Add("base_path", null);
            hashtable.Add("username", null);
            hashtable.Add("password", null);
            hashtable.Add("prm_path", null);
            hashtable.Add("packets", null);
            if (baseConf != null)
            {
                string md5Filename = ServiceUtil.GetMd5Hash(baseConf.baseName);
                hashtable["base_name"] = baseConf.baseName;
                hashtable["base_path"] = baseConf.basePath;
                hashtable["username"] = baseConf.username;
                hashtable["password"] = baseConf.password;
                hashtable["prm_path"] = String.Format(@"{0}\URBD\Prm\{1}.prm",
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    md5Filename);
                hashtable["log_path"] = String.Format(@"{0}\URBD\Log\{1}.log",
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    md5Filename);
                hashtable["packets"] = baseConf.packetList.ConvertAll<IConfiguration>(e =>
                {
                    Hashtable h = new Hashtable();
                    h.Add("path", e.filePath);
                    h.Add("type", e.packetType);
                    return new Configuration(h);
                });
            }
            return new Configuration(hashtable);
        }

        public void CreateFileAndRegistryKeys()
        {
            string appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string prmFolderPath = String.Format(@"{0}\URBD\Prm", appDataFolderPath);
            string logFolderPath = String.Format(@"{0}\URBD\Log", appDataFolderPath);

            if (!Directory.Exists(prmFolderPath))
                Directory.CreateDirectory(prmFolderPath);
            if (!Directory.Exists(logFolderPath))
                Directory.CreateDirectory(logFolderPath);

            foreach (Base base1C in remoteConfiguration.baseList)
            {
                string md5HashBaseName = ServiceUtil.GetMd5Hash(base1C.baseName);
                PrmBuilder prmBuilder = PrmBuilder.Create();
                prmBuilder.FileName = String.Format(@"{0}\{1}.prm", prmFolderPath, md5HashBaseName);
                prmBuilder.LogFile = String.Format(@"{0}\{1}.log", logFolderPath, md5HashBaseName);
                FileInfo prmFileInfo = prmBuilder.Build();
                // записать данные баз в реестр и создать отчет о конфигурировании
            }
        }
    }
}
