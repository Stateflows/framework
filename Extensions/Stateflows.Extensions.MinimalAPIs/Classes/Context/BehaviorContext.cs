using Microsoft.Extensions.DependencyInjection;
using Stateflows.Common;
using Stateflows.Common.Interfaces;
using Stateflows.Entities;
using IBehaviorContext = Stateflows.Extensions.MinimalAPIs.Interfaces.IBehaviorContext;

namespace Stateflows.Extensions.MinimalAPIs;

/// <summary>
/// Endpoint-side view of a behavior. It never hydrates nor persists behavior state;
/// every operation is a message sent to the behavior (or its entity).
/// </summary>
public class BehaviorContext : IBehaviorContext
{
    private readonly IBehaviorLocator locator;
    private readonly List<Task> pendingSends = new();

    internal BehaviorContext(BehaviorId id, IBehaviorLocator locator, IServiceProvider serviceProvider)
    {
        Id = id;
        this.locator = locator;
        ServiceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public BehaviorId Id { get; }

    // Endpoints are mapped only for top-level (not owned) behavior classes.
    /// <summary>Gets false because custom HTTP endpoints address top-level behavior classes.</summary>
    public bool IsEmbedded => false;

    /// <inheritdoc/>
    public IServiceProvider ServiceProvider { get; }

    /// <summary>Queues a message to the addressed behavior for completion by the endpoint filter.</summary>
    /// <typeparam name="TEvent">Event payload type.</typeparam>
    /// <param name="event">Event to send.</param>
    /// <param name="headers">Optional delivery headers.</param>
    /// <remarks>No message is sent if the behavior locator cannot resolve the address.</remarks>
    public void Send<TEvent>(TEvent @event, IDictionary<string, EventHeader>? headers = null)
    {
        if (locator.TryLocateBehavior(Id, out var behavior))
        {
            lock (pendingSends)
            {
                pendingSends.Add(behavior.SendAsync(@event, headers));
            }
        }
    }

    internal async Task CompletePendingSendsAsync()
    {
        Task[] tasks;
        lock (pendingSends)
        {
            tasks = pendingSends.ToArray();
            pendingSends.Clear();
        }

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc/>
    public Task SubscribeAsync<TNotification>(BehaviorId behaviorId)
        => ServiceProvider.GetRequiredService<IStateflowsSubscriber>().SubscribeAsync<TNotification>(Id, behaviorId);

    /// <inheritdoc/>
    public Task UnsubscribeAsync<TNotification>(BehaviorId behaviorId)
        => ServiceProvider.GetRequiredService<IStateflowsSubscriber>().UnsubscribeAsync<TNotification>(Id, behaviorId);

    private bool TryLocateEntity(string entityName, ref IDictionary<string, EventHeader>? headers, out IBehavior entity)
    {
        var entityId = new EntityId($"{Id.Name}.entity.{entityName}", Id.Instance);

        headers ??= new Dictionary<string, EventHeader>();
        headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding
        {
            OwnerId = Id,
            ParentId = Id
        };

        return locator.TryLocateBehavior(entityId, out entity);
    }

    /// <inheritdoc/>
    public async Task<bool> TryMutateAsync<TMutationEvent>(string entityName, TMutationEvent mutationEvent, IDictionary<string, EventHeader>? headers = null)
        => TryLocateEntity(entityName, ref headers, out var entity) &&
           (await entity.SendAsync(mutationEvent, headers)).Status == EventStatus.Consumed;

    /// <inheritdoc/>
    public async Task<(bool Success, TProjection Projection)> TryGetProjectionAsync<TProjection>(string entityName, IDictionary<string, EventHeader>? headers = null)
    {
        if (!TryLocateEntity(entityName, ref headers, out var entity))
        {
            return (false, default!);
        }

        var result = await entity.RequestAsync(new ProjectionRequest<TProjection>(), headers);
        return (result.Status == EventStatus.Consumed, result.Response);
    }

    /// <inheritdoc/>
    public async Task<bool> TrySetFieldAsync<TFieldType>(string entityName, string fieldName, TFieldType fieldValue, IDictionary<string, EventHeader>? headers = null)
        => TryLocateEntity(entityName, ref headers, out var entity) &&
           (await entity.SendAsync(new FieldState<TFieldType> { Name = fieldName, Value = fieldValue }, headers)).Status == EventStatus.Consumed;

    /// <inheritdoc/>
    public async Task<(bool Success, TFieldType Field)> TryGetFieldAsync<TFieldType>(string entityName, string fieldName, IDictionary<string, EventHeader>? headers = null)
    {
        if (!TryLocateEntity(entityName, ref headers, out var entity))
        {
            return (false, default!);
        }

        var result = await entity.RequestAsync(new FieldStateRequest<TFieldType> { Name = fieldName }, headers);
        return (result.Status == EventStatus.Consumed, result.Response != null ? result.Response.Value : default!);
    }
}
