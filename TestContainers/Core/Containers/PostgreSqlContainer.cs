using System;
using System.Reflection;

namespace TestContainers.Core.Containers
{
    public sealed class PostgreSqlContainer : SqlDatabaseContainer
    {
        public const string IMAGE = "postgres";
        public const string DEFAULT_TAG = "9.6.8";
        public const int POSTGRESQL_PORT = 5432;
        protected override Type ConnectionType { get; }

        protected override Type ExceptionType { get; }

        protected override Type CommandType { get; }

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "postgres";
        string _password = "Password123";

        public PostgreSqlContainer() : base()
        {
            var assembly = Assembly.Load("Npgsql");
            ConnectionType = assembly.GetType("Npgsql.NpgsqlConnection", true);
            CommandType = assembly.GetType("Npgsql.NpgsqlCommand", true);
            ExceptionType = assembly.GetType("Npgsql.NpgsqlException", true);
        }

        public override string ConnectionString => $"Host={GetDockerHostIpAddress()};Username={UserName};pwd={Password}";
    }
}