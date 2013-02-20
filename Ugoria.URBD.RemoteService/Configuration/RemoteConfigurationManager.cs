using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ugoria.URBD.Contracts;
using System.IO;
using Ugoria.URBD.Shared;
using System.Collections;
using Ugoria.URBD.Contracts.Data;
using System.Security.Principal;
using Microsoft.Win32;
using Ugoria.URBD.Shared.Configuration;
using System.Security.AccessControl;

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
            Hashtable hashtable = new Hashtable(remoteConfiguration.configuration);

            return new Configuration(hashtable);
        }

        public IConfiguration GetBaseConfiguration(int baseId)
        {
            Hashtable hashtable = new Hashtable(remoteConfiguration.configuration);

            Hashtable baseHash = remoteConfiguration.bases[baseId];
            if (baseHash != null)
            {
                foreach (DictionaryEntry entry in baseHash)
                    hashtable.Add(entry.Key, entry.Value);
            }
            return new Configuration(hashtable);
        }

        public static IDictionary<string, string> RemoteValidation(RemoteConfiguration configuration)
        {
            IDictionary<string, string> report = new Dictionary<string, string>();

            string identity = SecureHelper.ConvertUserdomainToClassic((string)configuration.configuration["username"]);
            if (!string.IsNullOrEmpty((string)configuration.configuration["service.1c_path"]))
            {
                FileInfo file1CInfo = new FileInfo((string)configuration.configuration["service.1c_path"]);

                if (!file1CInfo.Exists)
                    report.Add("service.1c_path", "Отсутствует исполнительный файл 1С на сервере");
                else if (SecureHelper.IsRuleAllow(file1CInfo.FullName, identity, FileSystemRights.ExecuteFile))
                    report.Add("service.1c_path", "Отсутствует право на исполнение файла 1С");
            }
            foreach (KeyValuePair<int, Hashtable> baseHash in configuration.bases)
            {
                DirectoryInfo basePathInfo = new DirectoryInfo((string)baseHash.Value["base.1c_database"]);
                if (!basePathInfo.Exists)
                    report.Add("base.path", "Отсутствует директория на сервере");
                else if (SecureHelper.IsRuleAllow(basePathInfo.FullName, identity, FileSystemRights.Write))
                    report.Add("base.path", "Отсутствует право на запись в директорию");
            }
            return report;
        }
    }
}
