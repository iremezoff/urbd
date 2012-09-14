using System.Xml;
using System.Data.SqlClient;
using System;

namespace Ugoria.URBD.CentralService
{
    public class DB
    {
        private static SqlConnectionStringBuilder cnStrBldr;
        public static string connectionString = @"Data Source=work\sqlserver;Initial Catalog=URBD2;Persist Security Info=True;User ID=sa;Password=123456";

        private DB() { }

        public SqlConnection Connection
        {
            get
            {
                if (cnStrBldr == null)
                {
                    cnStrBldr = new SqlConnectionStringBuilder(connectionString);
                    cnStrBldr.ConnectTimeout = 30;
                    cnStrBldr.IntegratedSecurity = false;
                }
                return new SqlConnection(cnStrBldr.ConnectionString);
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
