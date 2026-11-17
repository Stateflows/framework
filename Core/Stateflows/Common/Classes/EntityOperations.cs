using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateflows.Common.Engine;
using Stateflows.Entities;

namespace Stateflows.Common.Classes;

internal static class EntityOperations
{
    private static BehaviorId? Target(BehaviorId? owner, string name)
        => owner.HasValue ? new EntityId($"{owner.Value.Name}.entity.{name}", owner.Value.Instance) : null;

    private static IDictionary<string, EventHeader> Prepare(BehaviorId owner, IDictionary<string, EventHeader>? headers)
    {
        headers ??= new Dictionary<string, EventHeader>();
        headers[nameof(BehaviorEmbedding)] = new BehaviorEmbedding { OwnerId = owner, ParentId = owner };
        return headers;
    }

    private static bool Transactional(IDictionary<string, EventHeader>? headers)
        => headers?.Values.Any(header => header is JoinTransaction) == true;

    private static string? DefinitionId(BehaviorId? target, string element)
        => target.HasValue ? $"entity:{target.Value.Name}/{element}" : null;

    public static Task<bool> MutateAsync<T>(IBehaviorLocator locator, BehaviorId? owner, string name, T mutation, IDictionary<string, EventHeader>? headers)
    {
        var target = Target(owner, name);
        return BehaviorObservation.EntityAsync(owner, target, name, EntityOperationKind.Mutation, mutation!, Transactional(headers),
            DefinitionId(target, $"mutation:{Event.GetName(typeof(T))}"), async () =>
        {
            if (!owner.HasValue) return false;
            var deliveryHeaders = Prepare(owner.Value, headers);
            return locator.TryLocateBehavior(target!.Value, out var entity) &&
                (await entity.SendAsync(mutation, deliveryHeaders)).Status == EventStatus.Consumed;
        });
    }

    public static Task<bool> SetFieldAsync<T>(IBehaviorLocator locator, BehaviorId? owner, string name, string field, T value, IDictionary<string, EventHeader>? headers)
    {
        var target = Target(owner, name);
        var request = new FieldState<T> { Name = field, Value = value };
        return BehaviorObservation.EntityAsync(owner, target, name, EntityOperationKind.FieldWrite, request, Transactional(headers),
            DefinitionId(target, $"field:{field}"), async () =>
        {
            if (!owner.HasValue) return false;
            var deliveryHeaders = Prepare(owner.Value, headers);
            return locator.TryLocateBehavior(target!.Value, out var entity) &&
                (await entity.SendAsync(request, deliveryHeaders)).Status == EventStatus.Consumed;
        });
    }

    public static Task<(bool Success, T Field)> GetFieldAsync<T>(IBehaviorLocator locator, BehaviorId? owner, string name, string field, IDictionary<string, EventHeader>? headers)
    {
        var target = Target(owner, name);
        var request = new FieldStateRequest<T> { Name = field };
        return BehaviorObservation.EntityAsync(owner, target, name, EntityOperationKind.FieldRead, request, Transactional(headers),
            DefinitionId(target, $"field:{field}"), async () =>
        {
            if (!owner.HasValue) return (false, default(T)!);
            var deliveryHeaders = Prepare(owner.Value, headers);
            if (!locator.TryLocateBehavior(target!.Value, out var entity)) return (false, default(T)!);
            var result = await entity.RequestAsync(request, deliveryHeaders);
            return (result.Status == EventStatus.Consumed, result.Response != null ? result.Response.Value : default!);
        });
    }

    public static Task<(bool Success, T Projection)> GetProjectionAsync<T>(IBehaviorLocator locator, BehaviorId? owner, string name, IDictionary<string, EventHeader>? headers)
    {
        var target = Target(owner, name);
        var request = new ProjectionRequest<T>();
        return BehaviorObservation.EntityAsync(owner, target, name, EntityOperationKind.ProjectionRead, request, Transactional(headers),
            DefinitionId(target, $"projection:{Event.GetName(typeof(T))}"), async () =>
        {
            if (!owner.HasValue) return (false, default(T)!);
            var deliveryHeaders = Prepare(owner.Value, headers);
            if (!locator.TryLocateBehavior(target!.Value, out var entity)) return (false, default(T)!);
            var result = await entity.RequestAsync(request, deliveryHeaders);
            return (result.Status == EventStatus.Consumed, result.Response);
        });
    }
}
