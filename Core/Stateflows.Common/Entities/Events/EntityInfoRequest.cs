using Stateflows.Common;

namespace Stateflows.Entities
{
    [NoTracing, NoDeduplication]
    public sealed class EntityInfoRequest : IRequest<EntityInfo>
    { }
}
