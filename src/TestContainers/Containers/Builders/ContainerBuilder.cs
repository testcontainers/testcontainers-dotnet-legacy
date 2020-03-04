using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using TestContainers.Images;
using TestContainers.Images.Builders;
using TestContainers.Internal.Builders;
using TestContainers.Networks;
using TestContainers.Networks.Builders;

namespace TestContainers.Containers.Builders
{
    /// <summary>
    /// Builder class to consolidate services and inject them into an IContainer implementation
    /// </summary>
    /// <typeparam name="T">type of container to build</typeparam>
    public class ContainerBuilder<T> : AbstractBuilder<ContainerBuilder<T>, T> where T : IContainer
    {
        private readonly List<Action<HostContext, T>> _configureContainerActions = new List<Action<HostContext, T>>();
        private Func<HostContext, ContainerBuilder<T>, IImage> _imageProvider;
        private Func<HostContext, ContainerBuilder<T>, INetwork> _networkProvider;

        /// <summary>
        /// Sets the docker image name used to build this container
        /// </summary>
        /// <param name="name">image name and tag</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when name is null</exception>
        public ContainerBuilder<T> ConfigureImage(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return ConfigureImage(c => name);
        }

        /// <summary>
        /// Sets the docker image name used to build this container
        /// </summary>
        /// <param name="delegate">a delegate to provide a name</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureImage(Func<HostContext, string> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            return ConfigureImage((c, b) =>
            {
                return new ImageBuilder<GenericImage>()
                    .WithContextFrom(b)
                    .ConfigureImage((hostContext, i) =>
                    {
                        i.ImageName = @delegate.Invoke(c);
                    })
                    .Build();
            });
        }

        /// <summary>
        /// Sets the docker image used to build this container
        /// </summary>
        /// <param name="image">the docker image to use to build this container</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when dockerImage is null</exception>
        public ContainerBuilder<T> ConfigureImage(IImage image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image));
            }

            return ConfigureImage((c, b) => image);
        }

        /// <summary>
        /// Sets the docker image used to build this container
        /// </summary>
        /// <param name="delegate">a delegate to provide the docker image</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureImage(Func<HostContext, ContainerBuilder<T>, IImage> @delegate)
        {
            _imageProvider = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            return this;
        }

        /// <summary>
        /// Sets the docker network that this container will be attached to
        /// </summary>
        /// <param name="name">name of the docker network to attach to</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when dockerNetwork is null</exception>
        public ContainerBuilder<T> ConfigureNetwork(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return ConfigureNetwork(c => name);
        }

        /// <summary>
        /// Sets the docker network that this container will be attached to
        /// </summary>
        /// <param name="delegate">a delegate to provide the docker network name</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when dockerNetwork is null</exception>
        public ContainerBuilder<T> ConfigureNetwork(Func<HostContext, string> @delegate)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException(nameof(@delegate));
            }

            return ConfigureNetwork((c, b) =>
            {
                return new NetworkBuilder<UserDefinedNetwork>()
                    .WithContextFrom(b)
                    .ConfigureNetwork((hostContext, i) =>
                    {
                        i.NetworkName = @delegate.Invoke(c);
                    })
                    .Build();
            });
        }

        /// <summary>
        /// Sets the docker network that this container will be attached to
        /// </summary>
        /// <param name="network">the docker network to attach to</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when dockerNetwork is null</exception>
        public ContainerBuilder<T> ConfigureNetwork(INetwork network)
        {
            if (network == null)
            {
                throw new ArgumentNullException(nameof(network));
            }

            return ConfigureNetwork((c, b) => network);
        }

        /// <summary>
        /// Sets the docker network that this container will be attached to
        /// </summary>
        /// <param name="delegate">a delegate to provide the docker network</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureNetwork(Func<HostContext, ContainerBuilder<T>, INetwork> @delegate)
        {
            _networkProvider = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
            return this;
        }

        /// <summary>
        /// Allows the configuration of this container
        /// </summary>
        /// <param name="delegate">a delegate to configure this container</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ContainerBuilder<T> ConfigureContainer(Action<HostContext, T> @delegate)
        {
            _configureContainerActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        /// <inheritdoc />
        protected override void PreActivateHook(HostContext hostContext)
        {
            var image = _imageProvider != null ? _imageProvider.Invoke(hostContext, this) : NullImage.Instance;

            ConfigureServices(services =>
            {
                services.AddSingleton(image);
            });
        }

        /// <inheritdoc />
        protected override void PostActivateHook(HostContext hostContext, T instance)
        {
            if (_networkProvider != null)
            {
                var network = _networkProvider.Invoke(hostContext, this);
                instance.Network = network;
            }

            foreach (var action in _configureContainerActions)
            {
                action.Invoke(hostContext, instance);
            }
        }
    }
}
