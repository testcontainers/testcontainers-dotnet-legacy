namespace TestContainers.Core.Containers
{
    public abstract class DatabaseContainer : GenericContainer
    {
        protected string DatabaseName = "test";
        protected string UserName = "test";
        protected string Password = "test";

        protected DatabaseContainer(string dockerImageName) : base(dockerImageName) { }

        public void SetDatabaseName(string databaseName) => DatabaseName = databaseName;
        public void SetUserName(string userName) => UserName = userName;
        public void SetPassword(string password) => Password = password;

        protected abstract string GetTestQueryString();

        public abstract string GetConnectionString();
    }
}