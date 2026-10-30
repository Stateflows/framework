using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Threading;
using Stateflows.Common.Classes;
using Stateflows.Common.Interfaces;

namespace Stateflows.Locks.DistributedLock.Classes
{
    internal class DistributedLockService : IStateflowsLock
    {
        private Func<string, Task<IDistributedLock>> DistributedLockFactory { get; }
        private readonly LocalLockGate localGate;

        public DistributedLockService(Func<string, Task<IDistributedLock>> distributedLockFactory, LocalLockGate localGate)
        {
            DistributedLockFactory = distributedLockFactory;
            this.localGate = localGate;
        }

        public Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, TimeSpan? timeout = null)
            => AquireLockAsync(id, string.Empty, timeout);
        
        public async Task<IStateflowsLockHandle> AquireLockAsync(BehaviorId id, string scope, TimeSpan? timeout = null)
        {
            var waitTimeout = timeout ?? Timeout.InfiniteTimeSpan;
            if (waitTimeout != Timeout.InfiniteTimeSpan &&
                (waitTimeout < TimeSpan.Zero || waitTimeout.TotalMilliseconds > int.MaxValue))
            {
                throw new ArgumentOutOfRangeException(nameof(timeout));
            }

            var started = Stopwatch.GetTimestamp();
            var key = scope == string.Empty ? id.ToString() : $"{id.ToString()}.{scope}";
            var lease = await localGate.AcquireAsync(key, waitTimeout).ConfigureAwait(false);
            try
            {
                var distributedLock = await DistributedLockFactory(key).ConfigureAwait(false);
                var remaining = waitTimeout == Timeout.InfiniteTimeSpan
                    ? waitTimeout
                    : waitTimeout - Stopwatch.GetElapsedTime(started);
                var handle = await distributedLock.AcquireAsync(
                    waitTimeout != Timeout.InfiniteTimeSpan && remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining
                ).ConfigureAwait(false);

                return new StateflowsLockHandle(id, scope, new GatedHandle(handle, lease));
            }
            catch
            {
                lease.Dispose();
                throw;
            }
        }

        private sealed class GatedHandle(IAsyncDisposable handle, IDisposable lease) : IAsyncDisposable
        {
            private IAsyncDisposable? handle = handle;

            public async ValueTask DisposeAsync()
            {
                var current = Interlocked.Exchange(ref handle, null);
                if (current == null)
                {
                    return;
                }

                try
                {
                    await current.DisposeAsync().ConfigureAwait(false);
                }
                finally
                {
                    lease.Dispose();
                }
            }
        }
    }
}
