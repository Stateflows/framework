using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stateflows.Common.Engine;

internal sealed class PublicationScope : IDisposable
{
    private static readonly AsyncLocal<PublicationScope?> Current = new();
    private readonly PublicationScope? previous;
    private readonly ConcurrentQueue<Func<Task>> deferred = new();
    private readonly ConcurrentQueue<Task> immediate = new();
    private readonly bool runtimeOwned;

    public static bool IsRuntimeOwned => Current.Value?.runtimeOwned == true;

    public PublicationScope(bool runtimeOwned = false)
    {
        this.runtimeOwned = runtimeOwned;
        previous = Current.Value;
        Current.Value = this;
    }

    public static bool TryDispatch(Func<Task> publish, bool immediately)
    {
        if (Current.Value is not { } scope)
            return false;

        if (immediately)
        {
            // Publish has a synchronous public API. Never block an Orleans turn waiting for a grain RPC.
            scope.immediate.Enqueue(Task.Run(publish));
        }
        else
        {
            scope.deferred.Enqueue(publish);
        }
        return true;
    }

    public Task WaitForImmediateAsync() => Task.WhenAll(immediate.ToArray());

    public async Task FlushAsync()
    {
        while (deferred.TryDequeue(out var publish))
            await publish();
    }

    public void Dispose() => Current.Value = previous;
}
