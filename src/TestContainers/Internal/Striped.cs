using System;
using System.Collections.Concurrent;
using System.Threading;

namespace TestContainers.Internal
{
    /// <summary>
    /// A map of locks or derivatives of it
    /// </summary>
    /// <typeparam name="T">Type of lock</typeparam>
    public class Striped<T>
    {
        /// <summary>
        /// Create strips semaphore slims
        /// </summary>
        /// <returns>Striped semaphore slimes</returns>
        public static Striped<SemaphoreSlim> ForSemaphoreSlim()
        {
            return new Striped<SemaphoreSlim>(() => new SemaphoreSlim(1, 1));
        }

        private readonly ConcurrentDictionary<object, T> _strips = new ConcurrentDictionary<object, T>();

        private readonly Func<T> _supplier;

        /// <summary>
        /// Constructs a maps of locks
        /// </summary>
        /// <param name="supplier"></param>
        public Striped(Func<T> supplier)
        {
            _supplier = supplier;
        }

        /// <summary>
        /// Gets or creates a lock based on the key
        /// </summary>
        /// <param name="key">key to the lock</param>
        /// <returns>Instance of the lock</returns>
        public T Get(object key)
        {
            return _strips.GetOrAdd(key, _supplier.Invoke());
        }
    }
}
