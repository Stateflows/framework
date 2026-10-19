using Stateflows.Common;

namespace Stateflows.Entities
{
    [NoDeduplication]
    public class ProjectionRequest<TProjection> : IRequest<TProjection>;
}
