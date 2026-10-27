using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>Requests the status and metadata of an entity instance.</summary>
    [NoTracing, NoDeduplication]
    public sealed class EntityInfoRequest : IRequest<EntityInfo>
    { }
}
