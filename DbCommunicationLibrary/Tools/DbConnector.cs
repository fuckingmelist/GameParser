using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbCommunicationLibrary.Tools
{
    public class DbConnector
    {
        public static MySqlConnection GetConnection()
        {
            string connectionString = "Server=127.0.0.1;User=root;Password=1234;Database=gamechat";

            return new MySqlConnection(connectionString);
        }
    }
}
