# Testcontainers.NET
[![Build status](https://ci.appveyor.com/api/projects/status/b20um5dc0ikf8cme/branch/master?svg=true)](https://ci.appveyor.com/project/swissarmykirpan/testcontainers-dotnet-ak6r1/branch/master)[![Coverage Status](https://coveralls.io/repos/github/testcontainers/testcontainers-dotnet/badge.svg?branch=)](https://coveralls.io/github/testcontainers/testcontainers-dotnet?branch=master) <a href="https://www.nuget.org/packages/TestContainers"><img src="https://img.shields.io/nuget/v/TestContainers.svg?style=flat"></a> [![NuGet](https://img.shields.io/nuget/dt/TestContainers.svg)](https://www.nuget.org/packages/TestContainers)

## About 

Testcontainers is a .NET library  that helps to develop test automation - specially integration test scenarios. Under the hood, it uses  XUnit  framework and docker containers. https://testcontainers.org

Testcontainers make the following kinds of tests easier:

- **Data access layer integration tests**: use a containerised instance of a MySQL, PostgreSQL or Oracle database to test your data access layer code for complete compatibility, but without requiring complex setup on developers' machines and safe in the knowledge that your tests will always start with a known DB state. Any other database type that can be containerised can also be used.
- **Application integration tests**: for running your application in a short-lived test mode with dependencies, such as databases, message queues or web servers.
- **UI/Acceptance tests**: use containerised web browsers, compatible with Selenium, for conducting automated UI tests. Each test can get a fresh instance of the browser, with no browser state, plugin variations or automated browser upgrades to worry about. And you get a video recording of each test session, or just each session where tests failed.
- **Much more!** 

## Getting Started

To start with Testcontainers.NET, please make sure you have prerequisites packages installed.

### Prerequisites

- Docker - please see [General Docker requirements](https://www.testcontainers.org/supported_docker_environment/)

### Installation

To install Testcontainers.NET into your integration test project, on nuget package manager console, execute

```
Install-Package TestContainers
```

Now, you can begin writing your integration test with throwaway docker containers.

### Operating Systems

Testcontainers.NET supports Windows, Linux, and macOS as host systems. Linux Docker containers are supported on all three operating systems.

Native Windows Docker containers are only supported on Windows. Windows requires the host operating system version to match the container operating system version. You'll find further information about Windows container version compatibility [here](https://docs.microsoft.com/en-us/virtualization/windowscontainers/deploy-containers/version-compatibility).

Keep in mind to enable the correct Docker engine on Windows host systems to match the container operating system. With Docker CE you can switch the engine with: `$env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchDaemon` or `-SwitchLinuxEngine`, `-SwitchWindowsEngine`.

## Features

With Testcontainers.NET , tests can

- [ ] Create Docker Container from Docker Image
- [ ] communicate and network with container
- [ ] interact with Container instance by
  - [ ] Exposing container port to the host
  - [ ] Get container IP address
  - [ ] Expose Host port to the container
- [ ] Execute command in container through test
- [ ] Files and Volume support 
- [ ] Startup strategy
- [ ] Wait Strategy
- [ ] Accessing container Logs
- [ ] Creating Docker Images on the fly
- [ ] Custom configuration - overwrite some default properties of environments
- [ ] Advanced Options
- [ ] out of the box support for different technologies, databases and webdrivers through modules extension
- [ ] Enhancement of Unit Testing Framework Support from current XUnit Framework

## Commands

Testcontainers.NET comes with fluent API with support of various commands listed below.

- `WithImage` specifies an `IMAGE[:TAG]` to derive the container from.
- `WithWorkingDirectory` specifies and overrides the `WORKDIR` for the instruction sets.
- `WithEntrypoint` specifies and overrides the `ENTRYPOINT` that will run as an executable.
- `WithCommand` specifies and overrides the `COMMAND` instruction provided from the Dockerfile.
- `WithName` sets the container name e. g. `--name nginx`.
- `WithEnvironment` sets an environment variable in the container e. g. `-e, --env "test=containers"`.
- `WithLabel` applies metadata to a container e. g. `-l, --label dotnet.testcontainers=awesome`.
- `WithExposedPort` exposes a port inside the container e. g. `--expose=80`.
- `WithPortBinding` publishes a container port to the host e. g. `-p, --publish 80:80`.
- `WithMount` mounts a volume into the container e. g. `-v, --volume .:/tmp`.
- `WithCleanUp` removes a stopped container automatically.
- `WithOutputConsumer` redirects `stdout` and `stderr` to capture the Testcontainer output.
- `WithWaitStrategy` sets the wait strategy to complete the Testcontainer start and indicates when it is ready.
- `WithDockerfileDirectory` builds a Docker image based on a Dockerfile (`ImageFromDockerfileBuilder`).
- `WithDeleteIfExists` removes the Docker image before it is rebuilt (`ImageFromDockerfileBuilder`).

## Examples

Here is an example of a pre-configured Testcontainer. In the example, Testcontainers starts a PostgreSQL database and executes a SQL query.

```c#
  public async Task CreateSimplePostgresInstanceTest()
        {
            var container = new DatabaseContainerBuilder<PostgreSqlContainer>()
               .Begin()
               .WithImage($"{PostgreSqlContainer.IMAGE}:{PostgreSqlContainer.DEFAULT_TAG}")
               .WithExposedPorts(PostgreSqlContainer.POSTGRESQL_PORT)
               .WithEnv(("POSTGRES_PASSWORD", "Password123"))
               .Build();

            await container.Start();

            var dbConnection = new NpgsqlConnection(container.ConnectionString);
            await dbConnection.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT 1;", dbConnection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal(1, reader);

            dbConnection.Close();
            await container.Stop();
        }
```



## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/testcontainers/testcontainers-dotnet/tags). 

## Authors

* **Gurpreet Singh Sohal** - [swissarmykirpan](https://github.com/swissarmykirpan)
* **ISen** - [isen-ng](https://github.com/isen-ng)
* **Andre Hofmeister** - [HofmeisterAn](https://github.com/HofmeisterAn)
* **Lalit Kale** - [lalitkale](https://github.com/lalitkale)

See also the list of [contributors](https://github.com/testcontainers/testcontainers-dotnet/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgements

* https://github.com/microsoft/Docker.DotNet
* https://github.com/testcontainers/testcontainers-java
* https://testcontainers.org/

