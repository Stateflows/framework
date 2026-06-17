namespace Stateflows.Entities.Registration.Builders
{
    internal class FieldComputationContext<TTemplate, TField> : IFieldComputationContext<TTemplate, TField>
    {
        public TTemplate Entity { get; }
    }

    internal class FieldObservationContext<TField> : IFieldObservationContext<TField>
    {
        public TField Value => default;
    }

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
