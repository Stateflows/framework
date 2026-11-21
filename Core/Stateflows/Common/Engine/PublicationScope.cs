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
    private readonly ConcurrentQueue<Task> immediate = new();
    private readonly object sync = new();
    private Task tail = Task.CompletedTask;
    private readonly bool runtimeOwned;
    private readonly Func<bool>? dispatchGuard;

    public static bool IsRuntimeOwned => Current.Value?.runtimeOwned == true;

    public PublicationScope(
        bool runtimeOwned = false,
        Func<bool>? dispatchGuard = null)
    {
        this.runtimeOwned = runtimeOwned;
        this.dispatchGuard = dispatchGuard;
        previous = Current.Value;
        Current.Value = this;
    }

    public static bool TryDispatch(Func<Task> publish)
    {
        if (Current.Value is not { } scope)
            return false;

        lock (scope.sync)
        {
            var previous = scope.tail;
            // Publish has a synchronous public API. Preserve ordering without blocking the behavior turn.
            scope.tail = Task.Run(async () =>
            {
                await previous.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                if (scope.dispatchGuard?.Invoke() != false)
                    await publish();
            });
            scope.immediate.Enqueue(scope.tail);
        }
        return true;
    }

    public Task WaitForImmediateAsync() => Task.WhenAll(immediate.ToArray());

    public void Dispose() => Current.Value = previous;
}
