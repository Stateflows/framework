using System;
using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;
namespace Stateflows.Activities
{
    /// <summary>Registers activity definitions and global activity observability components.</summary>
    /// <remarks>Application code should normally use <see cref="ActivitiesDependencyInjection"/> and its <c>AddActivities</c> extension rather than resolving this low-level registration contract directly.</remarks>
    public interface IActivitiesRegister
    {
        /// <summary>Registers a reactive activity under its default version.</summary>
        /// <param name="activityName">The activity's registration name.</param>
        /// <param name="buildAction">Callback that declares the activity graph.</param>
        void AddActivity(string activityName, ReactiveActivityBuildAction buildAction)
            => AddActivity(activityName, 1, buildAction);
        
        /// <summary>Registers a versioned reactive activity.</summary>
        /// <param name="activityName">The activity's registration name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">Callback that declares the activity graph.</param>
        void AddActivity(string activityName, int version, ReactiveActivityBuildAction buildAction);

        /// <summary>Registers an activity type under its default version.</summary>
        /// <param name="activityName">The activity's registration name.</param>
        /// <param name="activityType">The concrete activity type.</param>
        void AddActivity(string activityName, Type activityType)
            => AddActivity(activityName, 1, activityType);
        
        /// <summary>Registers an activity type and version, with optional post-build configuration.</summary>
        /// <param name="activityName">The activity's registration name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="activityType">The concrete activity type.</param>
        /// <param name="buildAction">Optional callback for activity-wide configuration.</param>
        void AddActivity(string activityName, int version, Type activityType, ActivityUtilsBuildAction buildAction = null);
        
        /// <summary>Registers a typed activity using its inferred name unless a name is supplied.</summary>
        /// <typeparam name="TActivity">The activity implementation type.</typeparam>
        /// <param name="activityName">Optional registration name.</param>
        /// <param name="version">The definition version.</param>
        /// <param name="buildAction">Optional callback for activity-wide configuration.</param>
        void AddActivity<TActivity>(string activityName = null, int version = 1, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity;
        
        /// <summary>Visits all registered activity definitions.</summary>
        /// <param name="visitor">The visitor that receives registration metadata.</param>
        Task VisitActivitiesAsync(IActivityVisitor visitor);

        #region Observability
        /// <summary>Registers an asynchronously constructed global activity interceptor.</summary>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        void AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory);

        /// <summary>Registers a synchronously constructed global activity interceptor.</summary>
        /// <param name="interceptorFactory">The interceptor factory.</param>
        void AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));

        /// <summary>Registers a global activity interceptor implementation type.</summary>
        /// <typeparam name="TInterceptor">The interceptor type.</typeparam>
        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        /// <summary>Registers an asynchronously constructed global activity exception handler.</summary>
        /// <param name="exceptionHandlerFactory">The handler factory.</param>
        void AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory);

        /// <summary>Registers a synchronously constructed global activity exception handler.</summary>
        /// <param name="exceptionHandlerFactory">The handler factory.</param>
        void AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

        /// <summary>Registers a global activity exception-handler implementation type.</summary>
        /// <typeparam name="TExceptionHandler">The exception-handler type.</typeparam>
        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;

        /// <summary>Registers an asynchronously constructed global activity observer.</summary>
        /// <param name="observerFactory">The observer factory.</param>
        void AddObserver(ActivityObserverFactoryAsync observerFactory);

        /// <summary>Registers a synchronously constructed global activity observer.</summary>
        /// <param name="observerFactory">The observer factory.</param>
        void AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

        /// <summary>Registers a global activity observer implementation type.</summary>
        /// <typeparam name="TObserver">The observer type.</typeparam>
        void AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;

        #endregion
    }
}
