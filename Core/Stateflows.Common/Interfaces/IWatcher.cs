using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Represents a notification subscription that can be stopped synchronously or asynchronously.</summary>
    public interface IWatcher : IDisposable, IAsyncDisposable
    {
        /// <summary>Stops the subscription asynchronously.</summary>
        /// <returns>A task that completes when the watcher has been disposed.</returns>
        public Task UnwatchAsync()
        {
            Dispose();

            return Task.CompletedTask;
        }
    }
}
