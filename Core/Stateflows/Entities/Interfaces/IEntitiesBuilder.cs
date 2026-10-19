using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    public interface IEntitiesBuilder
    {
        IEntitiesBuilder AddFromAssembly(Assembly assembly);
        IEntitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        
        IEntitiesBuilder AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity<TTemplate>(Entity<TTemplate>.Name, 1, buildAction);
        IEntitiesBuilder AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity<TTemplate>(entityName, 1, buildAction);
        IEntitiesBuilder AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;
        
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(Entity<TTemplate, TEntity>.Name, 1, buildAction);
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(entityName, 1, buildAction);
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(Entity<TTemplate, TEntity>.Name, version, buildAction);
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;

        /// <summary>
        /// Adds a global observer applied to all entities. Global observers run before entity observers, in registration order.
        /// </summary>
        /// <param name="observerFactoryAsync">The factory method to create the observer.</param>
        IEntitiesBuilder AddObserver(EntityObserverFactoryAsync observerFactoryAsync);

        /// <summary>
        /// Adds a global observer applied to all entities. Global observers run before entity observers, in registration order.
        /// </summary>
        /// <param name="observerFactory">The factory method to create the observer.</param>
        IEntitiesBuilder AddObserver(EntityObserverFactory observerFactory)
            => AddObserver((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds a global observer applied to all entities. Global observers run before entity observers, in registration order.
        /// </summary>
        /// <typeparam name="TObserver">The type of the observer.</typeparam>
        IEntitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IEntityObserver;

        /// <summary>
        /// Adds a global exception handler applied to all entities.
        /// </summary>
        /// <param name="exceptionHandlerFactoryAsync">The factory method to create the exception handler.</param>
        IEntitiesBuilder AddExceptionHandler(EntityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>
        /// Adds a global exception handler applied to all entities.
        /// </summary>
        /// <param name="exceptionHandlerFactory">The factory method to create the exception handler.</param>
        IEntitiesBuilder AddExceptionHandler(EntityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds a global exception handler applied to all entities.
        /// </summary>
        /// <typeparam name="THandler">The type of the exception handler.</typeparam>
        IEntitiesBuilder AddExceptionHandler<THandler>()
            where THandler : class, IEntityExceptionHandler;

        /// <summary>
        /// Adds a global interceptor applied to all entities. Global interceptors run before entity interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactoryAsync">The factory method to create the interceptor.</param>
        IEntitiesBuilder AddInterceptor(EntityInterceptorFactoryAsync interceptorFactoryAsync);

        /// <summary>
        /// Adds a global interceptor applied to all entities. Global interceptors run before entity interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactory">The factory method to create the interceptor.</param>
        IEntitiesBuilder AddInterceptor(EntityInterceptorFactory interceptorFactory)
            => AddInterceptor((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));

        /// <summary>
        /// Adds a global interceptor applied to all entities. Global interceptors run before entity interceptors, in registration order.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
        IEntitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IEntityInterceptor;
    }
}
