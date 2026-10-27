namespace Stateflows.Storage;

internal sealed class OutboxPublicationScope : IDisposable
{
    private static readonly AsyncLocal<OutboxPublicationScope?> Current = new();
    private readonly OutboxPublicationScope? previous;
    private readonly System.Collections.Concurrent.ConcurrentQueue<OutboxPublication> publications = new();

    public OutboxPublicationScope()
    {
        previous = Current.Value;
        Current.Value = this;
    }

    public static bool TryEnqueue(string publisherKey, OrleansEventHolder[] notifications)
    {
        if (Current.Value is not { } scope)
            return false;
        scope.publications.Enqueue(new OutboxPublication { PublisherKey = publisherKey, Notifications = notifications });
        return true;
    }

    public OutboxPublication[] GetPublications() => publications.ToArray();

    public void Dispose() => Current.Value = previous;
}
