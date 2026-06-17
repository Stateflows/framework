using System.Threading.Tasks;

namespace Stateflows.Entities.Registration.Interfaces
{
    public delegate void EntitiesBuildAction(IEntitiesBuilder builder);

    public delegate void EntityBuildAction<TTemplate>(IEntityBuilder<TTemplate> builder);

    public delegate void FieldBuildAction<TTemplate, TField>(IFieldBuilder<TTemplate, TField> builder);

    public delegate TField FieldComputation<TTemplate, out TField>(TTemplate entity);

    public delegate Task FieldObservation<TField>(IFieldObservationContext<TField> context);

    public delegate void MutationAction<TTemplate, TMutation>(IMutationContext<TTemplate, TMutation> context);
    
    public delegate TTemplate DefaultProjectionAction<TTemplate>(IProjectionContext<TTemplate> context);
    
    public delegate TProjection ProjectionAction<TTemplate, out TProjection>(TTemplate entity);

    public delegate void DefaultInitializerAction<TTemplate>(IDefaultEntityInitializationContext<TTemplate> context);

    public delegate void InitializerAction<TTemplate, TInitializationEvent>(IEntityInitializationContext<TTemplate, TInitializationEvent> context);
}

