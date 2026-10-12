using System;
using System.Linq.Expressions;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public interface IEntityBuilder<TTemplate>
    {
        IEntityBuilder<TTemplate> AddDefaultInitializer(DefaultInitializerAction<TTemplate> action);

        IEntityBuilder<TTemplate> AddInitializer<TInitializationEvent>(InitializerAction<TTemplate, TInitializationEvent> action);

        IEntityBuilder<TTemplate> AddField<TField>(Expression<Func<TTemplate, TField>> fieldSelector, FieldBuildAction<TTemplate, TField> buildAction = null);

        IEntityBuilder<TTemplate> AddMutation<TMutationEvent>(MutationAction<TTemplate, TMutationEvent> mutationAction);
        
        IEntityBuilder<TTemplate> AddMutation<TMutationEvent, TMutation>()
            where TMutation : class, IMutation<TTemplate, TMutationEvent>
            => AddMutation<TMutationEvent>(c => TMutation.Mutate(c.Entity, c.MutationEvent));
        
        IEntityBuilder<TTemplate> AddProjection<TProjectionTemplate>(ProjectionAction<TTemplate, TProjectionTemplate> projectionAction, PublishScope publishScope = PublishScope.None);

        IEntityBuilder<TTemplate> AddProjection<TProjectionTemplate, TProjection>()
            where TProjection : class, IProjection<TTemplate, TProjectionTemplate>
            => AddProjection(TProjection.Project);

        IEntityBuilder<TTemplate> SetResourceName(string resourceName);
    }

    public interface IFieldBuilder<out TTemplate, in TFieldType>
    {
        IFieldBuilder<TTemplate, TFieldType> SetAccess(FieldAccess fieldAccess);
        
        void AddComputation(FieldComputation<TTemplate, TFieldType> computation);
        
        void AddComputation<TComputation>()
            where TComputation : IComputation<TTemplate, TFieldType>
            => AddComputation(TComputation.Compute);
        
        void AddDefaultValue(TFieldType defaultValue);
    }

    public interface IMutationContext<out TTemplate, out TMutationEvent>
    {
        TTemplate Entity { get; }
        TMutationEvent MutationEvent { get; }
    }

    public interface IProjectionContext<out TTemplate>
    {
        TTemplate Entity { get; }
    }

    public interface IDefaultEntityInitializationContext<out TTemplate>
    {
        TTemplate Entity { get; }
    }

    public interface IEntityInitializationContext<out TTemplate, out TInitializationEvent>
    {
        TTemplate Entity { get; }
        TInitializationEvent InitializationEvent { get; }
    }
}

