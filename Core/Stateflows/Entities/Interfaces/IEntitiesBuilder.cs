using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using Stateflows.Entities.Registration.Interfaces;

namespace Stateflows.Entities
{
    /// <summary>Registers entity definitions and hooks in the Stateflows host.</summary>
    /// <remarks>See the <see href="https://github.com/Stateflows/framework/wiki/entities">entity documentation</see>.</remarks>
    public interface IEntitiesBuilder
    {
        /// <summary>Discovers and registers entity definitions from an assembly.</summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddFromAssembly(Assembly assembly);
        /// <summary>Discovers and registers entity definitions from multiple assemblies.</summary>
        /// <param name="assemblies">Assemblies to scan.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        
        /// <summary>Registers an entity template using its inferred name and version 1.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity<TTemplate>(Entity<TTemplate>.Name, 1, buildAction);
        /// <summary>Registers an entity template under a specified name and version 1.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            => AddEntity<TTemplate>(entityName, 1, buildAction);
        /// <summary>Registers an entity template under an explicit name and version.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="version">The entity definition version.</param>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class;
        
        /// <summary>Registers a template with its <see cref="IEntity{TTemplate}"/> implementation, inferred name, and version 1.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(Entity<TTemplate, TEntity>.Name, 1, buildAction);
        /// <summary>Registers a template and implementation under a specified name and version 1.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(string entityName, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(entityName, 1, buildAction);
        /// <summary>Registers a template and implementation using the inferred name and explicit version.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <param name="version">The entity definition version.</param>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>
            => AddEntity<TTemplate, TEntity>(Entity<TTemplate, TEntity>.Name, version, buildAction);
        /// <summary>Registers a template and implementation under an explicit name and version.</summary>
        /// <typeparam name="TTemplate">The entity template type.</typeparam>
        /// <typeparam name="TEntity">The entity implementation type.</typeparam>
        /// <param name="entityName">The registered entity name.</param>
        /// <param name="version">The entity definition version.</param>
        /// <param name="buildAction">Optional entity configuration callback.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddEntity<TTemplate, TEntity>(string entityName, int version, EntityBuildAction<TTemplate>? buildAction = null)
            where TTemplate : class
            where TEntity : class, IEntity<TTemplate>;

        /// <summary>
        /// Adds a global observer applied to all entities. Global observers run before entity observers, in registration order.
        /// </summary>
        /// <param name="observerFactoryAsync">The factory method to create the observer.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddObserver(EntityObserverFactoryAsync observerFactoryAsync);

        /// <summary>
        /// Adds a global observer applied to all entities. Global observers run before entity observers, in registration order.
        /// </summary>
        /// <param name="observerFactory">The factory method to create the observer.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddObserver(EntityObserverFactory observerFactory)
            => AddObserver((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds a global observer applied to all entities. Global observers run before entity observers, in registration order.
        /// </summary>
        /// <typeparam name="TObserver">The type of the observer.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IEntityObserver;

        /// <summary>
        /// Adds a global exception handler applied to all entities.
        /// </summary>
        /// <param name="exceptionHandlerFactoryAsync">The factory method to create the exception handler.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddExceptionHandler(EntityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>
        /// Adds a global exception handler applied to all entities.
        /// </summary>
        /// <param name="exceptionHandlerFactory">The factory method to create the exception handler.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddExceptionHandler(EntityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));

        /// <summary>
        /// Adds a global exception handler applied to all entities.
        /// </summary>
        /// <typeparam name="THandler">The type of the exception handler.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddExceptionHandler<THandler>()
            where THandler : class, IEntityExceptionHandler;

        /// <summary>
        /// Adds a global interceptor applied to all entities. Global interceptors run before entity interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactoryAsync">The factory method to create the interceptor.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddInterceptor(EntityInterceptorFactoryAsync interceptorFactoryAsync);

        /// <summary>
        /// Adds a global interceptor applied to all entities. Global interceptors run before entity interceptors, in registration order.
        /// </summary>
        /// <param name="interceptorFactory">The factory method to create the interceptor.</param>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddInterceptor(EntityInterceptorFactory interceptorFactory)
            => AddInterceptor((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));

        /// <summary>
        /// Adds a global interceptor applied to all entities. Global interceptors run before entity interceptors, in registration order.
        /// </summary>
        /// <typeparam name="TInterceptor">The type of the interceptor.</typeparam>
        /// <returns>This builder for fluent configuration.</returns>
        IEntitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IEntityInterceptor;
    }
}
