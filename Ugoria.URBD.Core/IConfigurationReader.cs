using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace Ugoria.URBD.Core
{
    public interface IConfiguration
    {
        object GetParameter(string key);
    }

    // разнести по разным сборкам
    public class Configuration : IConfiguration
    {
        private Hashtable settings;

        public object GetParameter(string key)
        {
            return settings[key];
        }

        public Configuration(Hashtable settings)
        {
            this.settings = settings;
        }
    }

    public class ConfigurationManager
    {
        private IDataProvider dataProvider = null;

        public ConfigurationManager(IDataProvider dataProvider)
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

        private Hashtable ParseData(DataRow dataRow)
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

        public IConfiguration GetRemoteService(string basename) {
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
    }
}
