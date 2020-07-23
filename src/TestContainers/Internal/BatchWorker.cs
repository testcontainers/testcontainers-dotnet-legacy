using System;
using System.Threading.Tasks;

namespace TestContainers.Internal
{
    /// <summary>
    /// Worker that batches up requests
    /// </summary>
    public abstract class BatchWorker
    {
        private readonly object _lockable = new object();

        private bool _startingCurrentWorkCycle;

        private DateTime? _scheduledNotify;

        // Task for the current work cycle, or null if idle
        private volatile Task _currentWorkCycle;

        // Flag is set to indicate that more work has arrived during execution of the task
        private volatile bool _moreWork;

        private volatile bool _isDisposed;

        // Used to communicate the task for the next work cycle to waiters.
        // This value is non-null only if there are waiters.
        private TaskCompletionSource<Task> _nextWorkCyclePromise;

        /// <summary>Implement this member in derived classes to define what constitutes a work cycle</summary>
        protected abstract Task WorkAsync();

        /// <summary>
        /// Notify the worker that there is more work.
        /// </summary>
        public void Notify()
        {
            lock (_lockable)
            {
                if (_currentWorkCycle != null || _startingCurrentWorkCycle)
                {
                    // lets the current work cycle know that there is more work
                    _moreWork = true;
                }
                else
                {
                    // start a work cycle
                    Start();
                }
            }
        }

        /// <summary>
        /// Instructs the batch worker to run again to check for work, if
        /// it has not run again already by then, at specified <paramref name="utcTime"/>.
        /// </summary>
        /// <param name="utcTime"></param>
        public void Notify(DateTime utcTime)
        {
            var now = DateTime.UtcNow;

            if (now >= utcTime)
            {
                Notify();
            }
            else
            {
                lock (_lockable)
                {
                    if (!_scheduledNotify.HasValue || _scheduledNotify.Value > utcTime)
                    {
                        _scheduledNotify = utcTime;

                        ScheduleNotifyAsync(utcTime, now).Ignore();
                    }
                }
            }
        }

        private async Task ScheduleNotifyAsync(DateTime time, DateTime now)
        {
            await Task.Delay(time - now);

            if (_scheduledNotify == time)
            {
                Notify();
            }
        }

        private void Start()
        {
            if (_isDisposed)
            {
                return;
            }

            // Indicate that we are starting the worker (to prevent double-starts)
            _startingCurrentWorkCycle = true;

            // Clear any scheduled runs
            _scheduledNotify = null;

            try
            {
                // Start the task that is doing the work
                _currentWorkCycle = WorkAsync();
            }
            finally
            {
                // By now we have started, and stored the task in currentWorkCycle
                _startingCurrentWorkCycle = false;

                // chain a continuation that checks for more work, on the same scheduler
                _currentWorkCycle.ContinueWith(t => this.CheckForMoreWork(), TaskScheduler.Current);
            }
        }

        /// <summary>
        /// Executes at the end of each work cycle on the same task scheduler.
        /// </summary>
        private void CheckForMoreWork()
        {
            TaskCompletionSource<Task> signal = null;
            Task taskToSignal = null;

            lock (_lockable)
            {
                if (_moreWork)
                {
                    _moreWork = false;

                    // see if someone created a promise for waiting for the next work cycle
                    // if so, take it and remove it
                    signal = this._nextWorkCyclePromise;
                    this._nextWorkCyclePromise = null;

                    // start the next work cycle
                    Start();

                    // the current cycle is what we need to signal
                    taskToSignal = _currentWorkCycle;
                }
                else
                {
                    _currentWorkCycle = null;
                }
            }

            // to be safe, must do the signalling out here so it is not under the lock
            signal?.SetResult(taskToSignal);
        }

        /// <summary>
        /// Check if this worker is idle.
        /// </summary>
        public bool IsIdle()
        {
            // no lock needed for reading volatile field
            return _currentWorkCycle == null;
        }

        /// <summary>
        /// Wait for the current work cycle, and also the next work cycle if there is currently unserviced work.
        /// </summary>
        /// <returns></returns>
        public async Task WaitForCurrentWorkToBeServicedAsync()
        {
            Task<Task> waitfortasktask = null;
            Task waitfortask = null;

            // Figure out exactly what we need to wait for
            lock (_lockable)
            {
                if (!_moreWork)
                {
                    // Just wait for current work cycle
                    waitfortask = _currentWorkCycle;
                }
                else
                {
                    // we need to wait for the next work cycle
                    // but that task does not exist yet, so we use a promise that signals when the next work cycle is launched
                    if (_nextWorkCyclePromise == null)
                    {
                        _nextWorkCyclePromise = new TaskCompletionSource<Task>();
                    }

                    waitfortasktask = _nextWorkCyclePromise.Task;
                }
            }

            // Do the actual waiting outside of the lock
            if (waitfortasktask != null)
            {
                await await waitfortasktask;
            }
            else if (waitfortask != null)
            {
                await waitfortask;
            }
        }

        internal void Dispose()
        {
            _isDisposed = true;
        }
    }
}
