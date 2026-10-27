using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    /// <summary>Registers activities and global activity observability services.</summary>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/activities">Activities documentation</seealso>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability">Observability documentation</seealso>
    public interface IActivitiesBuilder
    {
        /// <summary>Discovers activity implementations and their registrations in an assembly.</summary>
        /// <param name="assembly">The assembly to scan.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddFromAssembly(Assembly assembly);

        /// <summary>Discovers activity implementations and their registrations in multiple assemblies.</summary>
        /// <param name="assemblies">The assemblies to scan.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);

        /// <summary>Registers an activity with a custom graph-building delegate.</summary>
        /// <param name="activityName">The activity's registration name.</param>
        /// <param name="buildAction">The callback that declares the activity graph.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddActivity(string activityName, ReactiveActivityBuildAction buildAction);

        /// <summary>Registers a versioned activity with a custom graph-building delegate.</summary>
        /// <param name="activityName">The activity's registration name.</param>
        /// <param name="version">The activity definition version.</param>
        /// <param name="buildAction">The callback that declares the activity graph.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddActivity(string activityName, int version, ReactiveActivityBuildAction buildAction);

        /// <summary>Registers a typed activity implementation, optionally overriding its inferred name and version.</summary>
        /// <typeparam name="TActivity">The activity implementation type.</typeparam>
        /// <param name="activityName">An optional registration name; when omitted, the type's activity name is used.</param>
        /// <param name="version">The activity definition version.</param>
        /// <param name="buildAction">Optional configuration for the registered activity.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity;

        /// <summary>Registers a typed activity implementation with an explicit version.</summary>
        /// <typeparam name="TActivity">The activity implementation type.</typeparam>
        /// <param name="version">The activity definition version.</param>
        /// <param name="buildAction">Optional configuration for the registered activity.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddActivity<TActivity>(int version, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity;

        /// <summary>Registers an activity interceptor type for all activities in this registration scope.</summary>
        /// <typeparam name="TInterceptor">The interceptor implementation type.</typeparam>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        /// <summary>Registers an asynchronously created global activity interceptor.</summary>
        /// <param name="interceptorFactoryAsync">Factory invoked with the service provider for the current operation.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactoryAsync);

        /// <summary>Registers a synchronously created global activity interceptor.</summary>
        /// <param name="interceptorFactory">Factory invoked with the service provider for the current operation.</param>
        /// <returns>This builder for further configuration.</returns>
        public IActivitiesBuilder AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
        /// <summary>Registers an activity exception-handler type for all activities in this registration scope.</summary>
        /// <typeparam name="TExceptionHandler">The exception-handler implementation type.</typeparam>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
        /// <summary>Registers an asynchronously created global activity exception handler.</summary>
        /// <param name="exceptionHandlerFactoryAsync">Factory invoked with the service provider for the current operation.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>Registers a synchronously created global activity exception handler.</summary>
        /// <param name="exceptionHandlerFactory">Factory invoked with the service provider for the current operation.</param>
        /// <returns>This builder for further configuration.</returns>
        public IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));
        /// <summary>Registers an activity observer type for all activities in this registration scope.</summary>
        /// <typeparam name="TObserver">The observer implementation type.</typeparam>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;
        /// <summary>Registers an asynchronously created global activity observer.</summary>
        /// <param name="observerFactoryAsync">Factory invoked with the service provider for the current operation.</param>
        /// <returns>This builder for further configuration.</returns>
        IActivitiesBuilder AddObserver(ActivityObserverFactoryAsync observerFactoryAsync);

        /// <summary>Registers a synchronously created global activity observer.</summary>
        /// <param name="observerFactory">Factory invoked with the service provider for the current operation.</param>
        /// <returns>This builder for further configuration.</returns>
        public IActivitiesBuilder AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));
    }
}
