using System;
using System.Threading.Tasks;

namespace Stateflows.Common.Classes
{
    /// <summary>Represents an active watch and removes it when disposed.</summary>
    public sealed class Watcher : IWatcher
    {
        private readonly IUnwatcher unwatcher;
        private readonly Func<Task> waitForDelivery;

        /// <summary>Creates a watcher that unregisters itself through the supplied unwatcher.</summary>
        /// <param name="unwatcher">The service that removes this watch.</param>
        public Watcher(IUnwatcher unwatcher)
            : this(unwatcher, () => throw new NotSupportedException("This watcher does not support a notification delivery barrier."))
        { }

        /// <summary>Creates a watcher with a callback-delivery barrier supplied by its runtime.</summary>
        public Watcher(IUnwatcher unwatcher, Func<Task> waitForDelivery)
        {
            this.unwatcher = unwatcher;
            this.waitForDelivery = waitForDelivery;
        }

        /// <inheritdoc />
        public Task WaitForDeliveryAsync() => waitForDelivery();

        /// <summary>Asynchronously removes the watch.</summary>
        public async ValueTask DisposeAsync()
        {
            await unwatcher.UnwatchAsync(this);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>Requests removal of the watch without waiting for completion.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _ = unwatcher.UnwatchAsync(this);
        }

        ~Watcher()
            => Dispose(false);
    }
}
