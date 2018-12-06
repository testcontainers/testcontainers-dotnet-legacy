using System;
using System.Reflection;

namespace TestContainers.Core.Containers
{
    public sealed class MySqlContainer : SqlDatabaseContainer
    {
        public const string NAME = "mysql";
        public const string IMAGE = "mysql";
        public const int MYSQL_PORT = 3306;
        protected override Type ConnectionType { get; }

        protected override Type ExceptionType { get; }

        protected override Type CommandType { get; }

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "root";
        string _password = "Password123";

        public MySqlContainer() : base()
        {
            var assembly = Assembly.Load("MySql.Data");
            ConnectionType = assembly.GetType("MySql.Data.MySqlClient.MySqlConnection", true);
            CommandType = assembly.GetType("MySql.Data.MySqlClient.MySqlCommand", true);
            ExceptionType = assembly.GetType("MySql.Data.MySqlClient.MySqlException", true);
        }

        public override string ConnectionString => $"Server={GetDockerHostIpAddress()};UID={UserName};pwd={Password};SslMode=none;";

    }
}