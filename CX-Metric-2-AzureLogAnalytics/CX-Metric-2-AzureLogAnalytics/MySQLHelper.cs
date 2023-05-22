using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CX_Metric_2_AzureLogAnalytics
{
    internal class ParameterModel
    {
        public string VARIABLE_NAME { get; set; }
        public string VARIABLE_VALUE { get; set; }
    }


    internal class MySQLHelper
    {
        public MySQLHelper(string serverName,string userId,string password)
        {
            ServerName = serverName;
            UserId = userId;
            Password = password;
        }

        public string ServerName { get; }
        public string UserId { get; }
        public string Password { get; }

        internal IEnumerable<TModel> GetData<TModel>(string sql)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = ServerName,
                UserID = UserId,
                Password = Password
            };

            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                conn.Open();
                return conn.Query<TModel>(sql);
            }
        }
    }
}
