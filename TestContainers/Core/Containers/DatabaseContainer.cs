namespace TestContainers.Core.Containers
{
    public abstract class DatabaseContainer : Container
    {
        protected int GetStartupTimeoutSeconds => 120;

        protected int GetConnectTimeoutSeconds => 10;

        public DatabaseContainer() : base()
        {

        }

        public virtual string DatabaseName { get; set; }
        public abstract string ConnectionString { get; }

        public virtual string UserName { get; set; }

        public virtual string Password { get; set; }
    }
}