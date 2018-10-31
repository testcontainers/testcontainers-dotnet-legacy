using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public abstract class RabbitMqContainerBuilder<TBuilder, TContainer> : GenericContainerBuilder<TBuilder, TContainer>
        where TBuilder : RabbitMqContainerBuilder<TBuilder, TContainer>
        where TContainer : RabbitMqContainer, new()
    {
        protected RabbitMqContainerBuilder() { }

        protected RabbitMqContainerBuilder(string image) : base(image) { }

        public TBuilder WithUser(string userName)
        {
            Container.SetUserName(userName);
            return Self;
        }

        public TBuilder WithPassword(string password)
        {
            Container.SetPassword(password);
            return Self;
        }

        public TBuilder WithVirtualHost(string virtualHost)
        {
            Container.SetVirtualHost(virtualHost);
            return Self;
        }
    }

    public class RabbitMqContainerBuilder : RabbitMqContainerBuilder<RabbitMqContainerBuilder, RabbitMqContainer>
    {
        public RabbitMqContainerBuilder() { }

        public RabbitMqContainerBuilder(string image) : base(image) { }
    }
}
