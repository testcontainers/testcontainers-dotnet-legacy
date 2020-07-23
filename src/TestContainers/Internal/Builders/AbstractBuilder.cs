using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TestContainers.DockerClient;

namespace TestContainers.Internal.Builders
{
    /// <summary>
    /// Base builder that holds common methods for builders
    /// </summary>
    /// <typeparam name="TSelf">The inherited builder's type</typeparam>
    /// <typeparam name="TInstance">The type to build</typeparam>
    public abstract class AbstractBuilder<TSelf, TInstance> where TSelf : AbstractBuilder<TSelf, TInstance>
    {
        private const string ApplicationNameKey = "applicationName";
        private const string EnvironmentKey = "environment";
        private const string DefaultEnvironment = "Development";

        private readonly List<Action<HostContext, IServiceCollection>> _configurationActions =
            new List<Action<HostContext, IServiceCollection>>();

        private readonly List<Action<IConfigurationBuilder>> _configureHostActions =
            new List<Action<IConfigurationBuilder>>();

        private readonly List<Action<HostContext, IConfigurationBuilder>> _configureAppActions =
            new List<Action<HostContext, IConfigurationBuilder>>();

        /// <summary>
        /// Copies the context from another builder
        /// </summary>
        /// <param name="builder">builder to copy from</param>
        /// <typeparam name="T1">builder's type</typeparam>
        /// <typeparam name="T2">builder's built type</typeparam>
        /// <returns>self</returns>
        public TSelf WithContextFrom<T1, T2>(AbstractBuilder<T1, T2> builder)
            where T1 : AbstractBuilder<T1, T2>
        {
            _configurationActions.AddRange(builder._configurationActions);
            _configureHostActions.AddRange(builder._configureHostActions);
            _configureAppActions.AddRange(builder._configureAppActions);

            return (TSelf) this;
        }

        /// <summary>
        /// Allows the configuration of host settings
        /// </summary>
        /// <param name="delegate">a delegate to configure host settings</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public TSelf ConfigureHostConfiguration(Action<IConfigurationBuilder> @delegate)
        {
            _configureHostActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return (TSelf) this;
        }

        /// <summary>
        /// Allows the configuration of app settings
        /// </summary>
        /// <param name="delegate">a delegate to configure app settings</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public TSelf ConfigureAppConfiguration(Action<HostContext, IConfigurationBuilder> @delegate)
        {
            _configureAppActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return (TSelf) this;
        }

        /// <summary>
        /// Allows the configuration of services
        /// </summary>
        /// <param name="delegate">a delegate to configure services</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public TSelf ConfigureServices(Action<HostContext, IServiceCollection> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            _configurationActions.Add(@delegate);
            return (TSelf) this;
        }

        /// <summary>
        /// Allows the configuration of services
        /// </summary>
        /// <param name="delegate">a delegate to configure services</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public TSelf ConfigureServices(Action<IServiceCollection> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            return ConfigureServices((context, collection) => @delegate(collection));
        }

        /// <summary>
        /// Builds the container
        /// </summary>
        /// <returns>An implementation of the container with services injected</returns>
        public TInstance Build()
        {
            var hostConfig = BuildHostConfiguration();
            var hostContext = new HostContext
            {
                ApplicationName = hostConfig[ApplicationNameKey],
                EnvironmentName = hostConfig[EnvironmentKey] ?? DefaultEnvironment,
                Configuration = hostConfig
            };

            var appConfig = BuildAppConfiguration(hostContext, hostConfig);
            hostContext.Configuration = appConfig;

            ConfigureServices(
                services =>
                {
                    services.AddSingleton<IDockerClientProvider, EnvironmentDockerClientProvider>();
                    services.AddSingleton<IDockerClientProvider, NpipeDockerClientProvider>();
                    services.AddSingleton<IDockerClientProvider, UnixDockerClientProvider>();

                    services.AddSingleton<DockerClientFactory>();
                    services.AddScoped(provider =>
                        provider.GetRequiredService<DockerClientFactory>()
                            .CreateAsync()
                            .Result);

                    services.AddLogging();
                });

            PreActivateHook(hostContext);

            var serviceProvider = BuildServiceProvider(hostContext);
            var container = ActivatorUtilities.CreateInstance<TInstance>(serviceProvider);

            PostActivateHook(hostContext, container);

            return container;
        }

        /// <summary>
        /// Hook that will be run before the container is activated
        /// </summary>
        /// <param name="hostContext">Host context for the hook to use</param>
        protected virtual void PreActivateHook(HostContext hostContext)
        {
        }

        /// <summary>
        /// Hook that will run after the container is activated
        /// </summary>
        /// <param name="hostContext">Host context for the hook to use</param>
        /// <param name="instance">Instance that was just activated</param>
        protected virtual void PostActivateHook(HostContext hostContext, TInstance instance)
        {
        }

        private IConfiguration BuildHostConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();
            foreach (var buildAction in _configureHostActions)
            {
                buildAction(configBuilder);
            }

            return configBuilder.Build();
        }

        private IConfiguration BuildAppConfiguration(HostContext hostContext, IConfiguration hostConfiguration)
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddConfiguration(hostConfiguration);

            foreach (var buildAction in _configureAppActions)
            {
                buildAction(hostContext, configBuilder);
            }

            return configBuilder.Build();
        }

        private IServiceProvider BuildServiceProvider(HostContext hostContext)
        {
            var services = new ServiceCollection();
            foreach (var configureServices in _configurationActions)
            {
                configureServices(hostContext, services);
            }

            return new DefaultServiceProviderFactory().CreateServiceProvider(services);
        }
    }
}
