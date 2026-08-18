using System;
using System.Linq.Expressions;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public interface IEntityBuilder<TEntityTemplate>
    {
        IEntityBuilder<TEntityTemplate> AddDefaultInitializer(DefaultInitializerAction<TEntityTemplate> action);

        IEntityBuilder<TEntityTemplate> AddInitializer<TInitializationEvent>(InitializerAction<TEntityTemplate, TInitializationEvent> action);

        IEntityBuilder<TEntityTemplate> AddField<TField>(Expression<Func<TEntityTemplate, TField>> fieldSelector, FieldBuildAction<TEntityTemplate, TField> buildAction = null);

        IEntityBuilder<TEntityTemplate> AddMutation<TMutationEvent>(MutationAction<TEntityTemplate, TMutationEvent> mutationAction);
        
        IEntityBuilder<TEntityTemplate> AddMutation<TMutationEvent, TMutation>()
            where TMutation : class, IMutation<TEntityTemplate, TMutationEvent>
            => AddMutation<TMutationEvent>(c => TMutation.Mutate(c.Entity, c.MutationEvent));
        
        IEntityBuilder<TEntityTemplate> AddProjection<TProjectionTemplate>(ProjectionAction<TEntityTemplate, TProjectionTemplate> projectionAction, PublishScope publishScope = PublishScope.None);

        IEntityBuilder<TEntityTemplate> AddProjection<TProjectionTemplate, TProjection>()
            where TProjection : class, IProjection<TEntityTemplate, TProjectionTemplate>
            => AddProjection(TProjection.Project);

        IEntityBuilder<TEntityTemplate> SetResourceName(string resourceName);
    }

    public interface IFieldBuilder<out TEntityTemplate, in TFieldType>
    {
        IFieldBuilder<TEntityTemplate, TFieldType> SetAccess(FieldAccess fieldAccess);
        
        void AddComputation(FieldComputation<TEntityTemplate, TFieldType> computation);
        
        void AddComputation<TComputation>()
            where TComputation : IComputation<TEntityTemplate, TFieldType>
            => AddComputation(TComputation.Compute);
        
        void AddDefaultValue(TFieldType defaultValue);
    }

    public interface IMutationContext<out TEntityTemplate, out TMutationEvent>
    {
        TEntityTemplate Entity { get; }
        TMutationEvent MutationEvent { get; }
    }

    public interface IProjectionContext<out TEntityTemplate>
    {
        TEntityTemplate Entity { get; }
    }

    public interface IDefaultEntityInitializationContext<out TEntityTemplate>
    {
        TEntityTemplate Entity { get; }
    }

    public interface IEntityInitializationContext<out TEntityTemplate, out TInitializationEvent>
    {
        TEntityTemplate Entity { get; }
        TInitializationEvent InitializationEvent { get; }
    }
}

