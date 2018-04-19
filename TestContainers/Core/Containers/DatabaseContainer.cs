using System;
using System.Threading.Tasks;
using Polly;

namespace TestContainers.Core.Containers
{
    public class DatabaseContainer : Container
    {
        protected int GetStartupTimeoutSeconds => 120;

        protected int GetConnectTImeoutSeconds => 120;

        public DatabaseContainer(string dockerImageName) : base(dockerImageName)
        {

        }

        public DatabaseContainer()
        {

        }

        public virtual string DatabaseName { get; set; }
        public virtual string ConnectionString { get; set; }

        public virtual string UserName { get; set; }

        public virtual string Password { get; set; }

        protected virtual string TestQueryString { get; }
    }
}