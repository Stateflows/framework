using System;
using System.Reflection;
using System.Collections.Generic;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivitiesBuilder
    {
        IActivitiesBuilder AddFromAssembly(Assembly assembly);
        IActivitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        [Obsolete("AddFromLoadedAssemblies() is deprecated, use AddFromAssembly(), AddFromAssemblies() or AddActivity() instead.")]
        IActivitiesBuilder AddFromLoadedAssemblies();
        IActivitiesBuilder AddActivity(string activityName, ReactiveActivityBuildAction buildAction);
        IActivitiesBuilder AddActivity(string activityName, int version, ReactiveActivityBuildAction buildAction);
        IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : class, IActivity;
        IActivitiesBuilder AddActivity<TActivity>(int version)
            where TActivity : class, IActivity;
        IActivitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;
        IActivitiesBuilder AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactoryAsync);
        public IActivitiesBuilder AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(async serviceProvider => interceptorFactory(serviceProvider));
        IActivitiesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
        IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        public IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(async serviceProvider => exceptionHandlerFactory(serviceProvider));
        IActivitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;
        IActivitiesBuilder AddObserver(ActivityObserverFactoryAsync observerFactoryAsync);
        public IActivitiesBuilder AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(async serviceProvider => observerFactory(serviceProvider));
    }
}
