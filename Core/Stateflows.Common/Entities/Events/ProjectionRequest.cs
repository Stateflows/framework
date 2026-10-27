using Stateflows.Common;

namespace Stateflows.Entities
{
    /// <summary>Requests the current value of an entity projection.</summary>
    /// <typeparam name="TProjection">The projection value type.</typeparam>
    [NoDeduplication]
    public class ProjectionRequest<TProjection> : IRequest<TProjection>;
}
