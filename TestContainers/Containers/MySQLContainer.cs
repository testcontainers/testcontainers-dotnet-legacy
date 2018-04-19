using System.Linq;
using TestContainers.Core.Containers;

namespace TestContainers.Containers
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

        public MySqlContainer() : base($"{IMAGE}:latest") { }

        public MySqlContainer(string dockerImageName) : base(dockerImageName) { }

        int GetMappedPort(int portNo) => portNo;

        public override string ConnectionString => $"Server={GetContainerIpAddress()};UID={UserName};pwd={Password}";

        protected override string TestQueryString => "SELECT 1";
    }
}