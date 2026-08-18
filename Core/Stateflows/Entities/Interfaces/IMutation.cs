namespace Stateflows.Entities;

public interface IMutation<in TEntityTemplate, in TMutationEvent>
{
    static abstract void Mutate(TEntityTemplate entity, TMutationEvent mutationEvent);
}