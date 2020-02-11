using System;
using System.Threading.Tasks;

namespace TestContainers.Internal
{
    /// <inheritdoc />
    public class BatchWorkerFromDelegate : BatchWorker
    {
        private readonly Func<Task> _work;

        /// <inheritdoc />
        public BatchWorkerFromDelegate(Func<Task> work)
        {
            _work = work;
        }

        /// <inheritdoc />
        protected override Task Work()
        {
            return _work();
        }
    }
}
