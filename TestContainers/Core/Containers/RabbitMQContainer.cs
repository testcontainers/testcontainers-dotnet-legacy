using System;
using System.Threading.Tasks;
using Polly;
using RabbitMQ.Client;

namespace TestContainers.Core.Containers
{
    public class RabbitMQContainer : Container
    {
        public const string IMAGE = "rabbitmq:3.7-alpine";
        public const int Port = 5672;
        public const int DefaultRequestedHeartbeatInSec = 60;

        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string VirtualHost { get; set; } = "/";
        public IConnection Connection { get; private set; }

        private IConnectionFactory GetConnectionFactory()
        {
            return new ConnectionFactory
            {
                HostName = GetDockerHostIpAddress(),
                Port = GetMappedPort(Port),
                VirtualHost = VirtualHost,
                UserName = UserName,
                Password = Password,
                Protocol = Protocols.DefaultProtocol,
                RequestedHeartbeat = DefaultRequestedHeartbeatInSec
            };
            
        }
        int GetMappedPort(int port) => port;

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
                    Connection = GetConnectionFactory().CreateConnection();
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