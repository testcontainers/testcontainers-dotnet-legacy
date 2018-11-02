using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public abstract class DatabaseContainerBuilder<TBuilder, TContainer> : GenericContainerBuilder<TBuilder, TContainer>
        where TBuilder : DatabaseContainerBuilder<TBuilder, TContainer>
        where TContainer : DatabaseContainer, new()
    {
        protected DatabaseContainerBuilder() { }

        protected DatabaseContainerBuilder(string image) : base(image) { }

        public TBuilder WithDatabaseName(string databaseName)
        {
            Container.DatabaseName = databaseName;
            return Self;
        }

        public TBuilder WithUserName(string userName)
        {
            Container.UserName = userName;
            return Self;
        }

        public TBuilder WithPassword(string password)
        {
            Container.Password = password;
            return Self;
        }
    }

    public class DatabaseContainerBuilder<TContainer> : DatabaseContainerBuilder<DatabaseContainerBuilder<TContainer>, TContainer>
        where TContainer : DatabaseContainer, new()
    {
        public DatabaseContainerBuilder() { }

        public DatabaseContainerBuilder(string image) : base(image) { }
    }
}
