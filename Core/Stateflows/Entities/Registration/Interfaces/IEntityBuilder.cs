using System;
using System.Threading.Tasks;
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

        /// <summary>
        /// Adds an observer to the entity. Entity observers run after global observers, in registration order.
        /// </summary>
        /// <param name="observerFactoryAsync">The factory method to create the observer.</param>
        IEntityBuilder<TTemplate> AddObserver(EntityObserverFactoryAsync observerFactoryAsync);

        /// <summary>
        /// Adds an observer to the entity. Entity observers run after global observers, in registration order.
        /// </summary>
        /// <param name="observerFactory">The factory method to create the observer.</param>
        IEntityBuilder<TTemplate> AddObserver(EntityObserverFactory observerFactory)
            => AddObserver((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds an observer to the entity. Entity observers run after global observers, in registration order.
        /// </summary>
        /// <typeparam name="TObserver">The type of the observer.</typeparam>
        IEntityBuilder<TTemplate> AddObserver<TObserver>()
            where TObserver : class, IEntityObserver;

        /// <summary>
        /// Adds an exception handler to the entity.
        /// </summary>
        /// <param name="exceptionHandlerFactoryAsync">The factory method to create the exception handler.</param>
        IEntityBuilder<TTemplate> AddExceptionHandler(EntityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>
        /// Adds an exception handler to the entity.
        /// </summary>
        /// <param name="exceptionHandlerFactory">The factory method to create the exception handler.</param>
        IEntityBuilder<TTemplate> AddExceptionHandler(EntityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds an exception handler to the entity.
        /// </summary>
        /// <typeparam name="THandler">The type of the exception handler.</typeparam>
        IEntityBuilder<TTemplate> AddExceptionHandler<THandler>()
            where THandler : class, IEntityExceptionHandler;

        /// <summary>
        /// Adds an interceptor to the entity. Entity interceptors run after global interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactoryAsync">The factory method to create the interceptor.</param>
        IEntityBuilder<TTemplate> AddInterceptor(EntityInterceptorFactoryAsync interceptorFactoryAsync);

        /// <summary>
        /// Adds an interceptor to the entity. Entity interceptors run after global interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactory">The factory method to create the interceptor.</param>
        IEntityBuilder<TTemplate> AddInterceptor(EntityInterceptorFactory interceptorFactory)
            => AddInterceptor((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));

        /// <summary>
        /// Adds an interceptor to the entity. Entity interceptors run after global interceptors, in registration order.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
        IEntityBuilder<TTemplate> AddInterceptor<TInterceptor>()
            where TInterceptor : class, IEntityInterceptor;
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
