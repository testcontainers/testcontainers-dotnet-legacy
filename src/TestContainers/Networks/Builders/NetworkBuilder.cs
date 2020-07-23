using System;
using System.Collections.Generic;
using TestContainers.Internal.Builders;

namespace TestContainers.Networks.Builders
{
    /// <summary>
    /// Builder class to consolidate services and inject them into an INetwork implementation
    /// </summary>
    /// <typeparam name="T">type of network to build</typeparam>
    public class NetworkBuilder<T> : AbstractBuilder<NetworkBuilder<T>, T> where T : INetwork
    {
        private readonly List<Action<HostContext, T>> _configureNetworkActions = new List<Action<HostContext, T>>();

        /// <summary>
        /// Allows the configuration of this image
        /// </summary>
        /// <param name="delegate">a delegate to configure this image</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public NetworkBuilder<T> ConfigureNetwork(Action<HostContext, T> @delegate)
        {
            _configureNetworkActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        /// <inheritdoc />
        protected override void PostActivateHook(HostContext hostContext, T instance)
        {
            foreach (var action in _configureNetworkActions)
            {
                action.Invoke(hostContext, instance);
            }
        }
    }
}
