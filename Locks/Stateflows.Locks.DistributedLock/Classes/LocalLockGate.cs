using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Locks.DistributedLock.Classes
{
    internal sealed class LocalLockGate
    {
        private readonly Dictionary<string, Entry> entries = new(StringComparer.Ordinal);

        public async Task<IDisposable> AcquireAsync(string key, TimeSpan timeout)
        {
            Entry entry;
            lock (entries)
            {
                if (!entries.TryGetValue(key, out entry!))
                {
                    entry = new Entry();
                    entries.Add(key, entry);
                }

                entry.References++;
            }

            try
            {
                if (!await entry.Semaphore.WaitAsync(timeout).ConfigureAwait(false))
                {
                    throw new TimeoutException($"Timed out waiting for the local behavior lock '{key}'.");
                }
            }
            catch
            {
                ReleaseReference(key, entry, false);
                throw;
            }

            return new Lease(this, key, entry);
        }

        private void ReleaseReference(string key, Entry entry, bool acquired)
        {
            lock (entries)
            {
                if (acquired)
                {
                    entry.Semaphore.Release();
                }

                // References include both holders and waiters, preventing removal during a handoff.
                if (--entry.References == 0)
                {
                    entries.Remove(key);
                    entry.Semaphore.Dispose();
                }
            }
        }

        private sealed class Entry
        {
            public readonly SemaphoreSlim Semaphore = new(1, 1);
            public int References;
        }

        private sealed class Lease(LocalLockGate owner, string key, Entry entry) : IDisposable
        {
            private LocalLockGate? owner = owner;

            public void Dispose()
                => Interlocked.Exchange(ref owner, null)?.ReleaseReference(key, entry, true);
        }
    }
}
