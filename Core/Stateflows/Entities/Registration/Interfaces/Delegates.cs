namespace Stateflows.Entities.Registration.Interfaces
{
    public delegate void EntitiesBuildAction(IEntitiesBuilder builder);

    public delegate void EntityBuildAction<TEntityTemplate>(IEntityBuilder<TEntityTemplate> builder);

    public delegate void FieldBuildAction<TEntityTemplate, out TFieldType>(IFieldBuilder<TEntityTemplate, TFieldType> builder);

    public delegate TFieldType FieldComputation<in TEntityTemplate, out TFieldType>(TEntityTemplate entity);

    public delegate void MutationAction<in TEntityTemplate, in TMutationEvent>(IMutationContext<TEntityTemplate, TMutationEvent> context);
    
    public delegate TProjectionTemplate ProjectionAction<in TEntityTemplate, out TProjectionTemplate>(TEntityTemplate entity);

    public delegate void DefaultInitializerAction<in TEntityTemplate>(IDefaultEntityInitializationContext<TEntityTemplate> context);

    public delegate void InitializerAction<in TEntityTemplate, in TInitializationEvent>(IEntityInitializationContext<TEntityTemplate, TInitializationEvent> context);
}

