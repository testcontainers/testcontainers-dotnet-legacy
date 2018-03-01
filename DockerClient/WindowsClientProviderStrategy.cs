using System;
using Docker.DotNet;

public class WindowsClientProviderStrategy : DockerClientProviderStrategy 
{
    //private static final int PING_TIMEOUT_DEFAULT = 5;
    //private static final String PING_TIMEOUT_PROPERTY_NAME = "testcontainers.windowsprovider.timeout";

    protected override DockerClientConfiguration Config { get; } =
        new DockerClientConfiguration(new Uri("tcp://localhost:2375"));

    protected override bool IsApplicable() => Utils.IsWindows();

    protected override string GetDescription() =>
        "Docker for windows (via TCP port 2375";

    protected override void Test()
    {

    }

    // @Override
    // public void test() throws InvalidConfigurationException {
    //     config = tryConfiguration("tcp://localhost:2375");
    // }

    // @Override
    // public String getDescription() {
    //     return "Docker for Windows (via TCP port 2375)";
    // }

    // @NotNull
    // protected DockerClientConfig tryConfiguration(String dockerHost) {
    //     config = DefaultDockerClientConfig.createDefaultConfigBuilder()
    //             .withDockerHost(dockerHost)
    //             .withDockerTlsVerify(false)
    //             .build();
    //     client = getClientForConfig(config);

    //     final int timeout = Integer.getInteger(PING_TIMEOUT_PROPERTY_NAME, PING_TIMEOUT_DEFAULT);
    //     ping(client, timeout);

    //     return config;
    // }
}