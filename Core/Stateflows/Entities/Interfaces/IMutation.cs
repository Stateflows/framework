namespace Stateflows.Entities;

/// <summary>Provides a static mutation operation for an entity event type.</summary>
/// <typeparam name="TEntityTemplate">The entity template type.</typeparam>
/// <typeparam name="TMutationEvent">The event type handled by the mutation.</typeparam>
public interface IMutation<in TEntityTemplate, in TMutationEvent>
{
    /// <summary>Applies the event's changes to the entity.</summary>
    /// <param name="entity">The entity state to modify.</param>
    /// <param name="mutationEvent">The triggering mutation event.</param>
    static abstract void Mutate(TEntityTemplate entity, TMutationEvent mutationEvent);
}