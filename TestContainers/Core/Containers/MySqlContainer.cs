using System;
using System.Linq;
using Newtonsoft.Json;

namespace TestContainers.Core.Containers
{
    public class MySqlContainer : DatabaseContainer
    {
        public const string NAME = "mysql";
        public const string IMAGE = "mysql";
        public const int MYSQL_PORT = 3306;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "root";
        string _password = "Password123";

        public MySqlContainer() : base()
        {

        }


        int GetMappedPort(int portNo) => portNo;


        public override string ConnectionString
        {
            get
            {
                Console.WriteLine(JsonConvert.SerializeObject(ContainerInspectResponse));
                return $"Server={ContainerInspectResponse.NetworkSettings.Networks.Values.FirstOrDefault().IPAddress};UID={UserName};pwd={Password};SslMode=none;";
            }
        }

        protected override string TestQueryString => "SELECT 1";
    }
}