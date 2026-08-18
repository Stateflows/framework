namespace Stateflows.Entities.Registration.Builders
{
    internal class MutationContext<TTemplate, TMutation>(TTemplate entity, TMutation mutationEvent) : IMutationContext<TTemplate, TMutation>
    {
        public TTemplate Entity => entity;
        public TMutation MutationEvent => mutationEvent;
    }

    internal class DefaultEntityInitializationContext<TTemplate>(TTemplate entity) : IDefaultEntityInitializationContext<TTemplate>
    {
        public TTemplate Entity => entity;
    }

    internal class EntityInitializationContext<TTemplate, TInitializationEvent>(TTemplate entity, TInitializationEvent initializationEvent)
        : IEntityInitializationContext<TTemplate, TInitializationEvent>
    {
        public TTemplate Entity => entity;
        public TInitializationEvent InitializationEvent => initializationEvent;
    }

    internal class ProjectionContext<TTemplate>(TTemplate entity)
        : IProjectionContext<TTemplate>
    {
        public TTemplate Entity => entity;
    }
}
