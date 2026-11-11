namespace Stateflows.Storage;

internal sealed class BehaviorTransactionScope(Func<bool> hasActiveStream) : IDisposable
{
    private static readonly AsyncLocal<BehaviorTransactionScope?> current = new();
    private readonly BehaviorTransactionScope? previous = current.Value;
    private readonly Func<bool> streamActive = hasActiveStream;
    private Exception? infrastructureFailure;

    public bool HasJoinedCalls { get; private set; }
    public Exception? InfrastructureFailure => Volatile.Read(ref infrastructureFailure);

    public void Enter() => current.Value = this;

    public static void Joining()
    {
        if (current.Value is not { } scope)
            return;
        if (scope.streamActive())
            throw new InvalidOperationException("An Activity with an active stream cannot send a joined transaction call.");
        scope.HasJoinedCalls = true;
    }

    public static void RecordInfrastructureFailure(Exception exception)
    {
        if (current.Value is { } scope)
            Interlocked.CompareExchange(ref scope.infrastructureFailure, exception, null);
    }

    public void Dispose() => current.Value = previous;
}
