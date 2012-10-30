using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using Ugoria.URBD.CentralService.DataProvider;
using Ugoria.URBD.Shared.Configuration;
using Ugoria.URBD.Contracts.Data;
using System.IO;
using System.Security.Principal;
using System.Security.AccessControl;
using Ugoria.URBD.Shared;

namespace Ugoria.URBD.CentralService
{
    public class CentralConfigurationManager
    {
        private IDataProvider dataProvider = null;

        public CentralConfigurationManager(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        private Hashtable ParseData(DataTable dataTable)
        {
            Hashtable hashtable = new Hashtable();

            foreach (DataRow dataRow in dataTable.Rows)
            {
                hashtable.Add(dataRow["key"], dataRow["value"]);
            }
            return hashtable;
        }

        private static Hashtable ParseData(DataRow dataRow)
        {
            Hashtable hashtable = new Hashtable();
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                hashtable.Add(column.ColumnName, dataRow[column]);
            }
            return hashtable;
        }

        public IConfiguration GetCentralServiceConfiguration()
        {
            DataTable settingsData = dataProvider.GetSettings();
            return new Configuration(ParseData(settingsData));
        }

        public static IConfiguration GetConfiguration(DataRow dataRow)
        {
            return new Configuration(ParseData(dataRow));
        }

        public IConfiguration GetRemoteService(string basename)
        {
            DataSet dataSet = dataProvider.GetScheduleData(basename);

            return new Configuration(ParseData(dataSet.Tables["Base"]));
        }

        public IConfiguration GetRemoteServiceConfiguration(string address)
        {
            // запрос конфигурации удаленного сервиса: параметры ftp, путь до БД, список баз и список пакетов этих баз
            DataSet settingsData = dataProvider.GetServiceSettings(address);
            Hashtable hashtable = new Hashtable();
            hashtable.Add("1c_path", null);

            List<IConfiguration> baseList = new List<IConfiguration>();
            foreach (DataRow baseRow in settingsData.Tables["Base"].Rows)
            {
                if (hashtable["1c_path"] == null) hashtable["1c_path"] = baseRow["1c_path"];
                Hashtable baseHashtable = ParseData(baseRow);
                List<IConfiguration> packetList = new List<IConfiguration>();
                foreach (DataRow packetRow in baseRow.GetChildRows("BasePacket"))
                {
                    packetList.Add(new Configuration(ParseData(packetRow)));
                }
                baseHashtable.Add("packets", packetList);
                baseList.Add(new Configuration(baseHashtable));
            }
            hashtable.Add("bases", baseList);
            return new Configuration(hashtable);
        }

        public static IDictionary<string, string> RemoteValidation(Uri remoteUri, RemoteConfiguration configuration)
        {
            IDictionary<string, string> report = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(configuration.file1CPath))
            {
                FileInfo file1CInfo = new FileInfo(String.Format(@"\\{0}\{1}", remoteUri.Host, configuration.file1CPath.Replace(":", "$")));

                if (!file1CInfo.Exists)
                    report.Add("File1CPath", "Отсутствует исполнительный файл 1С на сервере");
                else if (AccessCheck.IsRuleAllow(file1CInfo.FullName, FileSystemRights.ExecuteFile))
                    report.Add("File1CPath", "Отсутствует право на исполнение файла 1С");
            }

            if (configuration.baseList != null)
            {
                foreach (Base @base in configuration.baseList)
                {
                    DirectoryInfo basePathInfo = new DirectoryInfo(String.Format(@"\\{0}\{1}", remoteUri.Host, @base.basePath.Replace(":", "$")));
                    if (!basePathInfo.Exists)
                        report.Add("BasePath", "Отсутствует директория на сервере");
                    else if (AccessCheck.IsRuleAllow(basePathInfo.FullName, FileSystemRights.Write))
                        report.Add("BasePath", "Отсутствует право на запись в директорию");
                }
            }

            return report;
        }
    }
}
