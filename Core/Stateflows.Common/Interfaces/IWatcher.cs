using System;
using System.Threading.Tasks;

namespace Stateflows.Common
{
    /// <summary>Represents a notification subscription that can be stopped synchronously or asynchronously.</summary>
    public interface IWatcher : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Waits for watch callbacks queued by publications completed before this call.
        /// Call after sending an event to include its committed notifications in an HTTP response.
        /// </summary>
        /// <remarks>
        /// Delivery is best-effort, without retries. Call outside behavior processing and watch callbacks
        /// to avoid waiting for work that depends on the caller. SSE completion means a server write attempt,
        /// not acknowledgment by the remote client.
        /// </remarks>
        Task WaitForDeliveryAsync()
            => throw new NotSupportedException("This watcher does not support a notification delivery barrier.");

        /// <summary>Stops the subscription asynchronously.</summary>
        /// <returns>A task that completes when the watcher has been disposed.</returns>
        public Task UnwatchAsync()
        {
            Dispose();

            return Task.CompletedTask;
        }
    }
}
