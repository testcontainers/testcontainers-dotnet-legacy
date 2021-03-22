using System;
using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Internal
{
    /// <summary>
    /// Extensions for tasks
    /// </summary>
    public static class TaskExtensions
    {
        private static readonly Action<Task> IgnoreTaskContinuation = t => { _ = t.Exception; };

        /// <summary>
        /// Ignores the output of the task, whether it is successful or whether an exception is thrown
        /// </summary>
        /// <param name="task">Task to ignore</param>
        public static void Ignore(this Task task)
        {
            if (task.IsCompleted)
            {
                _ = task.Exception;
            }
            else
            {
                task.ContinueWith(
                    IgnoreTaskContinuation,
                    CancellationToken.None,
                    TaskContinuationOptions.NotOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously,
                    TaskScheduler.Default);
            }
        }
    }
}
