using System;
using System.Linq.Expressions;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public interface IEntityBuilder<TTemplate>
    {
        IEntityBuilder<TTemplate> AddDefaultInitializer(DefaultInitializerAction<TTemplate> action);

        IEntityBuilder<TTemplate> AddInitializer<TInitializationEvent>(InitializerAction<TTemplate, TInitializationEvent> action);

        IEntityBuilder<TTemplate> AddField<TField>(Expression<Func<TTemplate, TField>> fieldSelector, FieldBuildAction<TTemplate, TField> buildAction = null);
        // {
        //     if (fieldSelector.Body is not MemberExpression memberExpression)
        //         throw new ArgumentException("Field selector must be a simple member access expression (e.g. t => t.MyField)", nameof(fieldSelector));
        //
        //     return AddField<TField>(fieldSelector, memberExpression.Member.Name, buildAction);
        // }

        IEntityBuilder<TTemplate> AddMutation<TMutation>(MutationAction<TTemplate, TMutation> mutationAction);
        
        IEntityBuilder<TTemplate> AddProjection<TProjection>(ProjectionAction<TTemplate, TProjection> projectionAction);
    }

    public interface IFieldBuilder<TTemplate, TField>
    {
        IComputedFieldBuilder<TTemplate, TField> AddComputation(FieldComputation<TTemplate, TField> computation);

        IFieldBuilder<TTemplate, TField> AddObservation(FieldObservation<TField> observation);
    }

    public interface IComputedFieldBuilder<TTemplate, TField>
    {
        IComputedFieldBuilder<TTemplate, TField> AddObservation(FieldObservation<TField> observation);
    }

    public interface IFieldComputationContext<TTemplate, TField>
    {
        TTemplate Entity { get; }
    }

    public interface IFieldObservationContext<TField>
    {
        TField Value { get; }
    }

    public interface IMutationContext<TTemplate, TMutation>
    {
        TTemplate Entity { get; }
        TMutation MutationEvent { get; }
    }

    public interface IProjectionContext<TTemplate>
    {
        TTemplate Entity { get; }
    }

    public interface IDefaultEntityInitializationContext<TTemplate>
    {
        TTemplate Entity { get; }
    }

    public interface IEntityInitializationContext<TTemplate, TInitializationEvent>
    {
        TTemplate Entity { get; }
        TInitializationEvent InitializationEvent { get; }
    }
}

