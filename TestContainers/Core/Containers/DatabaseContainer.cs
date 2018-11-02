namespace TestContainers.Core.Containers
{
    public abstract class DatabaseContainer : GenericContainer
    {
        public string DatabaseName { get; set; } = "testcontainersdb";
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "admin";

        protected DatabaseContainer(string dockerImageName) : base(dockerImageName) { }

        protected abstract string GetTestQueryString();

        public abstract string GetConnectionString();
    }
}