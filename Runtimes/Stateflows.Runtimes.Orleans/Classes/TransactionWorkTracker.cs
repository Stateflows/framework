namespace Stateflows;

internal sealed class TransactionWorkTracker
{
    private readonly object sync = new();
    private int count;
    private bool stopping;

    public int Count
    {
        get { lock (sync) return count; }
    }

    public bool IsStopping
    {
        get { lock (sync) return stopping; }
    }

    public void BeginShutdown()
    {
        lock (sync)
            stopping = true;
    }

    public IDisposable? TryEnter(bool allowDuringShutdown = false)
    {
        lock (sync)
        {
            if (stopping && (!allowDuringShutdown || count == 0))
                return null;
            count++;
            return new Lease(this);
        }
    }

    private sealed class Lease(TransactionWorkTracker owner) : IDisposable
    {
        private TransactionWorkTracker? current = owner;

        public void Dispose()
        {
            var tracker = Interlocked.Exchange(ref current, null);
            if (tracker == null)
                return;
            lock (tracker.sync)
                tracker.count--;
        }
    }
}
