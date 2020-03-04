using System;
using System.Collections.Generic;
using TestContainers.Internal.Builders;

namespace TestContainers.Images.Builders
{
    /// <summary>
    /// Builder class to consolidate services and inject them into an IImage implementation
    /// </summary>
    /// <typeparam name="T">type of image to build</typeparam>
    public class ImageBuilder<T> : AbstractBuilder<ImageBuilder<T>, T> where T : IImage
    {
        private readonly List<Action<HostContext, T>> _configureImageActions = new List<Action<HostContext, T>>();

        /// <summary>
        /// Allows the configuration of this image
        /// </summary>
        /// <param name="delegate">a delegate to configure this image</param>
        /// <returns>self</returns>
        /// <exception cref="ArgumentNullException">when @delegate is null</exception>
        public ImageBuilder<T> ConfigureImage(Action<HostContext, T> @delegate)
        {
            _configureImageActions.Add(@delegate ?? throw new ArgumentNullException(nameof(@delegate)));
            return this;
        }

        /// <inheritdoc />
        protected override void PostActivateHook(HostContext hostContext, T instance)
        {
            foreach (var action in _configureImageActions)
            {
                action.Invoke(hostContext, instance);
            }
        }
    }
}
