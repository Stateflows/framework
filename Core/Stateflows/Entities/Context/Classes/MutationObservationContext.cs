namespace Stateflows.Entities.Context.Classes
{
    internal sealed class MutationObservationContext<TTemplate, TMutationEvent>(
        EntityActionContext actionContext,
        TTemplate entity,
        TMutationEvent mutationEvent)
        : DelegatingActionContext(actionContext), IMutationObservationContext<TTemplate, TMutationEvent>
    {
        public TTemplate Entity { get; } = entity;

        public TMutationEvent MutationEvent { get; } = mutationEvent;
    }
}