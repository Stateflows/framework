using System;
using System.Threading.Tasks;
using Stateflows.Common;

namespace Stateflows.Entities.Registration.Interfaces
{
    /// <summary>Configures entity registrations.</summary>
    /// <param name="builder">The entity registration builder.</param>
    public delegate void EntitiesBuildAction(IEntitiesBuilder builder);

    /// <summary>Configures a registered entity template.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <param name="builder">The entity builder.</param>
    public delegate void EntityBuildAction<TTemplate>(IEntityBuilder<TTemplate> builder);

    /// <summary>Configures an entity template field.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TFieldType">The field value type.</typeparam>
    /// <param name="builder">The field builder.</param>
    public delegate void FieldBuildAction<in TTemplate, out TFieldType>(IFieldBuilder<TTemplate, TFieldType> builder);

    /// <summary>Computes the value of an entity field from the entity state.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TFieldType">The computed field type.</typeparam>
    /// <param name="entity">The entity used to compute the field.</param>
    /// <returns>The computed field value.</returns>
    public delegate TFieldType FieldComputation<in TTemplate, out TFieldType>(TTemplate entity);

    /// <summary>Applies a mutation event to an entity through its operation context.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TMutationEvent">The mutation event type.</typeparam>
    /// <param name="context">The entity and triggering event.</param>
    public delegate void MutationAction<in TTemplate, in TMutationEvent>(IMutationContext<TTemplate, TMutationEvent> context);
    
    /// <summary>Computes a projection from an entity template.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TProjectionTemplate">The projection value type.</typeparam>
    /// <param name="entity">The entity to project.</param>
    /// <returns>The computed projection.</returns>
    public delegate TProjectionTemplate ProjectionAction<in TTemplate, out TProjectionTemplate>(TTemplate entity);

    /// <summary>Initializes a default entity instance.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <param name="context">The new entity instance.</param>
    public delegate void DefaultInitializerAction<in TTemplate>(IDefaultEntityInitializationContext<TTemplate> context);

    /// <summary>Initializes an entity in response to a typed event.</summary>
    /// <typeparam name="TTemplate">The entity template type.</typeparam>
    /// <typeparam name="TInitializationEvent">The event type that triggers initialization.</typeparam>
    /// <param name="context">The entity and triggering event.</param>
    public delegate void InitializerAction<in TTemplate, in TInitializationEvent>(IEntityInitializationContext<TTemplate, TInitializationEvent> context);

    /// <summary>Creates an entity observer using the current dependency-injection scope and behavior context.</summary>
    /// <param name="serviceProvider">The service provider for the current execution.</param>
    /// <param name="context">The current behavior action context.</param>
    /// <returns>The observer instance.</returns>
    public delegate IEntityObserver EntityObserverFactory(IServiceProvider serviceProvider, IBehaviorActionContext context);
    /// <summary>Asynchronously creates an entity observer for the current execution.</summary>
    /// <param name="serviceProvider">The service provider for the current execution.</param>
    /// <param name="context">The current behavior action context.</param>
    /// <returns>A task producing the observer instance.</returns>
    public delegate Task<IEntityObserver> EntityObserverFactoryAsync(IServiceProvider serviceProvider, IBehaviorActionContext context);

    /// <summary>Creates an entity interceptor using the current dependency-injection scope and behavior context.</summary>
    /// <param name="serviceProvider">The service provider for the current execution.</param>
    /// <param name="context">The current behavior action context.</param>
    /// <returns>The interceptor instance.</returns>
    public delegate IEntityInterceptor EntityInterceptorFactory(IServiceProvider serviceProvider, IBehaviorActionContext context);
    /// <summary>Asynchronously creates an entity interceptor for the current execution.</summary>
    /// <param name="serviceProvider">The service provider for the current execution.</param>
    /// <param name="context">The current behavior action context.</param>
    /// <returns>A task producing the interceptor instance.</returns>
    public delegate Task<IEntityInterceptor> EntityInterceptorFactoryAsync(IServiceProvider serviceProvider, IBehaviorActionContext context);

    /// <summary>Creates an entity exception handler using the current dependency-injection scope and behavior context.</summary>
    /// <param name="serviceProvider">The service provider for the current execution.</param>
    /// <param name="context">The current behavior action context.</param>
    /// <returns>The exception handler instance.</returns>
    public delegate IEntityExceptionHandler EntityExceptionHandlerFactory(IServiceProvider serviceProvider, IBehaviorActionContext context);
    /// <summary>Asynchronously creates an entity exception handler for the current execution.</summary>
    /// <param name="serviceProvider">The service provider for the current execution.</param>
    /// <param name="context">The current behavior action context.</param>
    /// <returns>A task producing the exception handler instance.</returns>
    public delegate Task<IEntityExceptionHandler> EntityExceptionHandlerFactoryAsync(IServiceProvider serviceProvider, IBehaviorActionContext context);
}
