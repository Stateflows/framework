using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Stateflows.Entities.Attributes;
using Stateflows.Entities.Enums;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    /// <summary>Configures initialization, fields, mutations, projections, and execution hooks for an entity type.</summary>
    /// <typeparam name="TTemplate">The entity template whose fields and values are exposed to operations.</typeparam>
    /// <remarks>See the <see href="https://github.com/Stateflows/framework/wiki/entity-fields">entity fields</see>,
    /// <see href="https://github.com/Stateflows/framework/wiki/entity-mutations">mutations</see>, and
    /// <see href="https://github.com/Stateflows/framework/wiki/entity-projections">projections</see> guides.</remarks>
    public interface IEntityBuilder<TTemplate>
    {
        /// <summary>Registers initialization to run when a default entity instance is created.</summary>
        /// <param name="action">Initialization callback receiving the newly created entity.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddDefaultInitializer(DefaultInitializerAction<TTemplate> action);

        /// <summary>Registers initialization to run when an entity is initialized by an event.</summary>
        /// <typeparam name="TInitializationEvent">The event that triggers this initializer.</typeparam>
        /// <param name="action">Callback that initializes the entity using the event payload.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddInitializer<TInitializationEvent>(InitializerAction<TTemplate, TInitializationEvent> action);

        /// <summary>Registers an entity field and optionally configures its access, default, or computation.</summary>
        /// <typeparam name="TField">The field's value type.</typeparam>
        /// <param name="fieldSelector">A direct member selector identifying a field on the template.</param>
        /// <param name="buildAction">Optional callback for configuring field access, default value, or computation.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddField<TField>(Expression<Func<TTemplate, TField>> fieldSelector, FieldBuildAction<TTemplate, TField> buildAction = null);

        /// <summary>Registers a mutation event and the operation that changes entity state.</summary>
        /// <typeparam name="TMutationEvent">The event handled by the mutation.</typeparam>
        /// <param name="mutationAction">The mutation operation.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddMutation<TMutationEvent>(MutationAction<TTemplate, TMutationEvent> mutationAction);
        
        /// <summary>Registers a mutation implemented by a static-operation type.</summary>
        /// <typeparam name="TMutationEvent">The event handled by the mutation.</typeparam>
        /// <typeparam name="TMutation">The implementation providing the mutation operation.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddMutation<TMutationEvent, TMutation>()
            where TMutation : class, IMutation<TTemplate, TMutationEvent>
            => AddMutation<TMutationEvent>(c => TMutation.Mutate(c.Entity, c.MutationEvent));
        
        /// <summary>Registers a projection computed from the entity template.</summary>
        /// <typeparam name="TProjectionTemplate">The projection value type.</typeparam>
        /// <param name="projectionAction">Computes the projection from the entity.</param>
        /// <param name="publishScope">The audiences to which projection changes are published.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddProjection<TProjectionTemplate>(ProjectionAction<TTemplate, TProjectionTemplate> projectionAction, PublishScope publishScope = PublishScope.None);

        /// <summary>Registers a projection implemented by a static-operation type.</summary>
        /// <typeparam name="TProjectionTemplate">The projection value type.</typeparam>
        /// <typeparam name="TProjection">The implementation providing the projection operation.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddProjection<TProjectionTemplate, TProjection>()
            where TProjection : class, IProjection<TTemplate, TProjectionTemplate>
            => AddProjection(TProjection.Project);

        /// <summary>Assigns the entity to a named resource group.</summary>
        /// <param name="resourceName">The resource group configured on the host.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> SetResourceName(string resourceName);

        /// <summary>
        /// Adds an observer to the entity. Entity observers run after global observers, in registration order.
        /// </summary>
        /// <param name="observerFactoryAsync">The factory method to create the observer.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddObserver(EntityObserverFactoryAsync observerFactoryAsync);

        /// <summary>
        /// Adds an observer to the entity. Entity observers run after global observers, in registration order.
        /// </summary>
        /// <param name="observerFactory">The factory method to create the observer.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddObserver(EntityObserverFactory observerFactory)
            => AddObserver((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds an observer to the entity. Entity observers run after global observers, in registration order.
        /// </summary>
        /// <typeparam name="TObserver">The type of the observer.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddObserver<TObserver>()
            where TObserver : class, IEntityObserver;

        /// <summary>
        /// Adds an exception handler to the entity.
        /// </summary>
        /// <param name="exceptionHandlerFactoryAsync">The factory method to create the exception handler.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddExceptionHandler(EntityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>
        /// Adds an exception handler to the entity.
        /// </summary>
        /// <param name="exceptionHandlerFactory">The factory method to create the exception handler.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddExceptionHandler(EntityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds an exception handler to the entity.
        /// </summary>
        /// <typeparam name="THandler">The type of the exception handler.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddExceptionHandler<THandler>()
            where THandler : class, IEntityExceptionHandler;

        /// <summary>
        /// Adds an interceptor to the entity. Entity interceptors run after global interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactoryAsync">The factory method to create the interceptor.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddInterceptor(EntityInterceptorFactoryAsync interceptorFactoryAsync);

        /// <summary>
        /// Adds an interceptor to the entity. Entity interceptors run after global interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactory">The factory method to create the interceptor.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddInterceptor(EntityInterceptorFactory interceptorFactory)
            => AddInterceptor((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));

        /// <summary>
        /// Adds an interceptor to the entity. Entity interceptors run after global interceptors, in registration order.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntityBuilder<TTemplate> AddInterceptor<TInterceptor>()
            where TInterceptor : class, IEntityInterceptor;
    }

    /// <summary>Configures access, computation, and default values for a registered entity field.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TFieldType">The field value type.</typeparam>
    public interface IFieldBuilder<out TTemplate, in TFieldType>
    {
        /// <summary>Sets which operations may read or write the field.</summary>
        /// <param name="fieldAccess">The access policy for the field.</param>
        /// <returns>This field builder for fluent configuration.</returns>
        IFieldBuilder<TTemplate, TFieldType> SetAccess(FieldAccess fieldAccess);
        
        /// <summary>Defines a computation that derives the field value from the entity.</summary>
        /// <param name="computation">The calculation invoked when the field is initialized or invalidated.</param>
        void AddComputation(FieldComputation<TTemplate, TFieldType> computation);
        
        /// <summary>Defines the field computation using a static-operation type.</summary>
        /// <typeparam name="TComputation">The type providing the computation operation.</typeparam>
        void AddComputation<TComputation>()
            where TComputation : IComputation<TTemplate, TFieldType>
            => AddComputation(TComputation.Compute);
        
        /// <summary>Sets the field's initial value when no computation supplies it.</summary>
        /// <param name="defaultValue">The value assigned during initialization.</param>
        void AddDefaultValue(TFieldType defaultValue);
    }

    /// <summary>Exposes an entity and the event currently being applied by a mutation.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
    public interface IMutationContext<out TTemplate, out TMutationEvent>
    {
        /// <summary>Gets the entity being mutated.</summary>
        TTemplate Entity { get; }
        /// <summary>Gets the event that triggered the mutation.</summary>
        TMutationEvent MutationEvent { get; }
    }

    /// <summary>Exposes the entity used to compute a projection.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    public interface IProjectionContext<out TTemplate>
    {
        /// <summary>Gets the entity whose projection is being calculated.</summary>
        TTemplate Entity { get; }
    }

    /// <summary>Exposes an entity during default-instance initialization.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    public interface IDefaultEntityInitializationContext<out TTemplate>
    {
        /// <summary>Gets the entity being initialized.</summary>
        TTemplate Entity { get; }
    }

    /// <summary>Exposes an entity and triggering event during entity initialization.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TInitializationEvent">The initialization event type.</typeparam>
    public interface IEntityInitializationContext<out TTemplate, out TInitializationEvent>
    {
        /// <summary>Gets the entity being initialized.</summary>
        TTemplate Entity { get; }
        /// <summary>Gets the event that triggered initialization.</summary>
        TInitializationEvent InitializationEvent { get; }
    }
}
