using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestContainers.Internal.Builders
{
    public static class LoggingContainerBuilderExtensions
    {
        /// <summary>
        /// Allows the configuration of logging builder
        /// </summary>
        /// <param name="builder">builder</param>
        /// <param name="configureLogging">delegate to configure logging with</param>
        /// <typeparam name="TSelf">the builder's type</typeparam>
        /// <typeparam name="TInstance">the type that you're building</typeparam>
        /// <returns>self</returns>
        public static TSelf ConfigureLogging<TSelf, TInstance>(this AbstractBuilder<TSelf, TInstance> builder,
            Action<HostContext, ILoggingBuilder> configureLogging)
            where TSelf : AbstractBuilder<TSelf, TInstance>
        {
            return builder.ConfigureServices((context, collection) =>
                collection.AddLogging(loggingBuilder => configureLogging(context, loggingBuilder)));
        }

        /// <summary>
        /// Allows the configuration of logging builder
        /// </summary>
        /// <param name="builder">builder</param>
        /// <param name="configureLogging">delegate to configure logging with</param>
        /// <typeparam name="TSelf">the builder's type</typeparam>
        /// <typeparam name="TInstance">the type that you're building</typeparam>
        /// <returns>self</returns>
        public static TSelf ConfigureLogging<TSelf, TInstance>(this AbstractBuilder<TSelf, TInstance> builder,
            Action<ILoggingBuilder> configureLogging)
            where TSelf : AbstractBuilder<TSelf, TInstance>
        {
            return builder.ConfigureServices(collection => collection.AddLogging(configureLogging));
        }
    }
}
