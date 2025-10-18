using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivitiesBuilder
    {
        IActivitiesBuilder AddFromAssembly(Assembly assembly);
        IActivitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
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
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
        IActivitiesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
        IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);
        public IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));
        IActivitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;
        IActivitiesBuilder AddObserver(ActivityObserverFactoryAsync observerFactoryAsync);
        public IActivitiesBuilder AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));
    }
}
