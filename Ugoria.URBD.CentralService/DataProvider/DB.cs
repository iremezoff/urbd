﻿using System.Xml;
using System.Data.SqlClient;
using System;
using System.Configuration;

namespace Ugoria.URBD.CentralService.DataProvider
{
    public class DB
    {
        private static SqlConnectionStringBuilder cnStrBldr;
        private static string connectionString;
        private static string ozekiConnectionString;

        private DB()
        {
            connectionString = ConfigurationManager.ConnectionStrings["CentralServiceConnectionString"].ConnectionString;
            ozekiConnectionString = ConfigurationManager.ConnectionStrings["OzekiConnectionString"].ConnectionString;
        }

        public SqlConnection Connection
        {
            get
            {
                if (cnStrBldr == null)
                {
                    cnStrBldr = new SqlConnectionStringBuilder(connectionString);
                    cnStrBldr.ConnectTimeout = 30;
                }
                return new SqlConnection(cnStrBldr.ConnectionString);
            }
        }

        public SqlConnection OzekiConnection
        {
            get
            {
                return new SqlConnection(ozekiConnectionString);
            }
        }

        public void Reset()
        {
            cnStrBldr = null;
        }

        private sealed class SingletonCreator
        {
            private static readonly DB instance = new DB();
            public static DB Instance { get { return instance; } }
        }

        public static DB Instance
        {
            get { return SingletonCreator.Instance; }
        }
    }
}
