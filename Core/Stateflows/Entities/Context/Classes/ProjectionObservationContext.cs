using Stateflows.Entities.Enums;

namespace Stateflows.Entities.Context.Classes
{
    internal sealed class ProjectionObservationContext<TTemplate>(
        EntityActionContext actionContext,
        TTemplate entity,
        PublishScope publishScope)
        : DelegatingActionContext(actionContext), IProjectionObservationContext<TTemplate>
    {
        public TTemplate Entity { get; } = entity;

        public PublishScope PublishScope { get; } = publishScope;
    }
}