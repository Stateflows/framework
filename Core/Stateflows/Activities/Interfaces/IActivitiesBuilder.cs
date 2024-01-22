using System.Reflection;
using System.Collections.Generic;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivitiesBuilder
    {
        IActivitiesBuilder AddFromAssembly(Assembly assembly);
        IActivitiesBuilder AddFromAssemblies(IEnumerable<Assembly> assemblies);
        IActivitiesBuilder AddFromLoadedAssemblies();
        IActivitiesBuilder AddActivity(string activityName, ReactiveActivityBuilderAction buildAction);
        IActivitiesBuilder AddActivity(string activityName, int version, ReactiveActivityBuilderAction buildAction);
        IActivitiesBuilder AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : Activity;
        IActivitiesBuilder AddActivity<TActivity>(int version)
            where TActivity : Activity;
        IActivitiesBuilder AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;
        IActivitiesBuilder AddInterceptor(ActivityInterceptorFactory interceptorFactory);
        IActivitiesBuilder AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
        IActivitiesBuilder AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory);
        IActivitiesBuilder AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;
        IActivitiesBuilder AddObserver(ActivityObserverFactory observerFactory);
    }
}
