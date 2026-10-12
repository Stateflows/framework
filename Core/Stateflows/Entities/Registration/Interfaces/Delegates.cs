namespace Stateflows.Entities.Registration.Interfaces
{
    public delegate void EntitiesBuildAction(IEntitiesBuilder builder);

    public delegate void EntityBuildAction<TTemplate>(IEntityBuilder<TTemplate> builder);

    public delegate void FieldBuildAction<in TTemplate, out TFieldType>(IFieldBuilder<TTemplate, TFieldType> builder);

    public delegate TFieldType FieldComputation<in TTemplate, out TFieldType>(TTemplate entity);

    public delegate void MutationAction<in TTemplate, in TMutationEvent>(IMutationContext<TTemplate, TMutationEvent> context);
    
    public delegate TProjectionTemplate ProjectionAction<in TTemplate, out TProjectionTemplate>(TTemplate entity);

    public delegate void DefaultInitializerAction<in TTemplate>(IDefaultEntityInitializationContext<TTemplate> context);

    public delegate void InitializerAction<in TTemplate, in TInitializationEvent>(IEntityInitializationContext<TTemplate, TInitializationEvent> context);
}

