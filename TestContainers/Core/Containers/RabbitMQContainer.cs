using System;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client;

namespace TestContainers.Core.Containers
{
    public class RabbitMqContainer : GenericContainer
    {
        public const string Image = "rabbitmq";
        public const string DefaultTag = "3.7-alpine";
        public const int RabbitMqPort = 5672;
        private const int DefaultRequestedHeartbeatInSec = 60;

        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";

        public string RabbitMqUrl => $"amqp://{GetDockerHostIpAddress()}:{GetMappedPort(RabbitMqPort)}";

        public IConnection Connection { get; private set; }

        private IConnectionFactory _connectionFactory;
        public IConnectionFactory ConnectionFactory => 
            _connectionFactory ?? (_connectionFactory = new ConnectionFactory
        {
            HostName = GetDockerHostIpAddress(),
            Port = GetMappedPort(RabbitMqPort),
            VirtualHost = VirtualHost,
            UserName = UserName,
            Password = Password,
            Protocol = Protocols.DefaultProtocol,
            RequestedHeartbeat = DefaultRequestedHeartbeatInSec
        });

        public RabbitMqContainer(string tag) : base($"{Image}:{tag}") { }
        public RabbitMqContainer() : this(DefaultTag) { }

        protected override void Configure()
        {
            AddExposedPort(RabbitMqPort);
            AddEnv("RABBITMQ_DEFAULT_USER", UserName);
            AddEnv("RABBITMQ_DEFAULT_PASS", Password);
            AddEnv("RABBITMQ_DEFAULT_VHOST", VirtualHost);
        }

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();
            
            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<Exception>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    Connection = ConnectionFactory.CreateConnection();
                    if (!Connection.IsOpen)
                    {
                        throw new Exception("Connection not open");
                    }
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                Connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}