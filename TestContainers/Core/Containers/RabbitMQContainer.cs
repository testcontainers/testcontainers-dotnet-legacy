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

        private string _userName;
        private string _password;
        private string _virtualHost;

        public IConnection Connection { get; private set; }

        private IConnectionFactory _connectionFactory;
        public IConnectionFactory ConnectionFactory => 
            _connectionFactory ?? (_connectionFactory = new ConnectionFactory
        {
            HostName = GetDockerHostIpAddress(),
            Port = GetMappedPort(RabbitMqPort),
            VirtualHost = _virtualHost,
            UserName = _userName,
            Password = _password,
            Protocol = Protocols.DefaultProtocol,
            RequestedHeartbeat = DefaultRequestedHeartbeatInSec
        });

        public RabbitMqContainer(string tag) : base($"{Image}:{tag}")
        {
            _userName = "guest";
            _password = "guest";
            _virtualHost = "/";
        }

        public RabbitMqContainer() : this(DefaultTag) { }

        public void SetUserName(string userName) => _userName = userName;
        public void SetPassword(string password) => _password = password;
        public void SetVirtualHost(string virtualHost) => _virtualHost = virtualHost;

        protected override void Configure()
        {
            AddExposedPort(RabbitMqPort);
            AddEnv("RABBITMQ_DEFAULT_USER", _userName);
            AddEnv("RABBITMQ_DEFAULT_PASS", _password);
            AddEnv("RABBITMQ_DEFAULT_VHOST", _virtualHost);
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