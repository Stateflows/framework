using System.Collections.Concurrent;

namespace Stateflows.Storage;

internal sealed class GrainPublicationScope : IDisposable
{
    private static readonly AsyncLocal<GrainPublicationScope?> Current = new();
    private readonly GrainPublicationScope? previous;
    private readonly ConcurrentQueue<Func<Task>> publications = new();

    public GrainPublicationScope()
    {
        previous = Current.Value;
        Current.Value = this;
    }

    public static bool TryEnqueue(Func<Task> publish)
    {
        if (Current.Value is not { } scope) return false;
        scope.publications.Enqueue(publish);
        return true;
    }

    public async Task FlushAsync()
    {
        while (publications.TryDequeue(out var publish))
        {
            try
            {
                await publish();
            }
            catch
            {
                StateflowsOrleansDiagnostics.PublicationFailed("publish");
                throw;
            }
        }
    }

    public void Dispose() => Current.Value = previous;
}
