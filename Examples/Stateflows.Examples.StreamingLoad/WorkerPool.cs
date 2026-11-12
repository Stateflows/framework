namespace Stateflows.Examples.StreamingLoad;

public sealed class WorkerPool
{
    private readonly object sync = new();
    private int limit = 4;
    private int active;
    private readonly LinkedList<TaskCompletionSource<IDisposable>> waiting = new();
    public int Active { get { lock (sync) return active; } }

    public void Configure(int workers)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(workers, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(workers, 64);
        lock (sync)
        {
            limit = workers;
            AdmitWaiting();
        }
    }

    public async Task<IDisposable> EnterAsync(CancellationToken cancellationToken)
    {
        LinkedListNode<TaskCompletionSource<IDisposable>> pending;
        lock (sync)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (active < limit)
            {
                active++;
                return new Lease(this);
            }
            pending = waiting.AddLast(new TaskCompletionSource<IDisposable>(TaskCreationOptions.RunContinuationsAsynchronously));
        }
        using var registration = cancellationToken.Register(() =>
        {
            lock (sync)
            {
                if (pending.List == null) return;
                waiting.Remove(pending);
                pending.Value.TrySetCanceled(cancellationToken);
            }
        });
        return await pending.Value.Task;
    }

    private void Leave()
    {
        lock (sync)
        {
            active--;
            AdmitWaiting();
        }
    }

    private void AdmitWaiting()
    {
        while (active < limit && waiting.First is { } pending)
        {
            waiting.RemoveFirst();
            active++;
            pending.Value.TrySetResult(new Lease(this));
        }
    }

    private sealed class Lease(WorkerPool owner) : IDisposable
    {
        private WorkerPool? owner = owner;
        public void Dispose() => Interlocked.Exchange(ref owner, null)?.Leave();
    }
}
