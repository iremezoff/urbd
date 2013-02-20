using System.Collections;
using System.Collections.Generic;
using System.Data;
using Ugoria.URBD.CentralService.DataProvider;
using Ugoria.URBD.Contracts.Services;
using Ugoria.URBD.Shared.Configuration;
using System;

namespace Ugoria.URBD.CentralService
{
    public class CentralConfigurationManager
    {
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

        public Hashtable GetCentralServiceConfiguration()
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                DataTable settingsData = dataProvider.GetSettings();
                return ParseData(settingsData);
            }
        }

        public static IConfiguration GetConfiguration(DataRow dataRow)
        {
            return new Configuration(ParseData(dataRow));
        }

        public Hashtable GetRemoteServiceConfiguration(int baseId)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                DataSet dataSet = dataProvider.GetScheduleData(baseId);
                return ParseData(dataSet.Tables["Base"]);
            }
        }

        public Hashtable GetRemoteServiceConfiguration(string address)
        {
            using (DBDataProvider dataProvider = new DBDataProvider())
            {
                // запрос конфигурации удаленного сервиса: параметры ftp, путь до БД, список баз и список пакетов этих баз
                DataSet settingsData = dataProvider.GetServiceSettings(address);
                Hashtable hashtable = new Hashtable();
                hashtable.Add("service.1c_path", null);

                List<Hashtable> baseList = new List<Hashtable>();
                foreach (DataRow baseRow in settingsData.Tables["Base"].Rows)
                {
                    if (hashtable["service.1c_path"] == null) hashtable["service.1c_path"] = baseRow["1c_path"];

                    Hashtable baseHashtable = new Hashtable();
                    foreach (DataColumn column in baseRow.Table.Columns)
                    {
                        baseHashtable.Add("base." + column.ColumnName, baseRow[column]);
                    }
                    List<Hashtable> packetList = new List<Hashtable>();
                    foreach (DataRow packetRow in baseRow.GetChildRows("BasePacketRelation"))
                    {
                        packetList.Add(new Hashtable()
                        { { "packet.filename", (string)packetRow["filename"] },
                            {"packet.type", (PacketType)((string)packetRow["type"])[0]}
                        });
                    }
                    baseHashtable.Add("base.packet_list", packetList);

                    Dictionary<string, string> dirHashtable = new Dictionary<string, string>();
                    foreach (DataRow extDirRow in baseRow.GetChildRows("ExtDirectoriesBaseRelation"))
                    {
                        dirHashtable.Add((string)extDirRow["ftp_path"], (string)extDirRow["local_path"]);
                    }
                    baseHashtable.Add("base.extdir_table", dirHashtable);

                    baseList.Add(baseHashtable);
                }
                hashtable.Add("base_list", baseList);
                return hashtable;
            }
        }
    }
}
