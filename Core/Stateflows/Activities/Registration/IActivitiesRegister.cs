using System;
using System.Threading.Tasks;
using Stateflows.Activities.Registration.Interfaces;

namespace Stateflows.Activities
{
    public interface IActivitiesRegister
    {
        void AddActivity(string activityName, ReactiveActivityBuildAction buildAction)
            => AddActivity(activityName, 1, buildAction);
        
        void AddActivity(string activityName, int version, ReactiveActivityBuildAction buildAction);

        void AddActivity(string activityName, Type activityType)
            => AddActivity(activityName, 1, activityType);
        
        void AddActivity(string activityName, int version, Type activityType, ActivityUtilsBuildAction buildAction = null);
        
        void AddActivity<TActivity>(string activityName = null, int version = 1, ActivityUtilsBuildAction buildAction = null)
            where TActivity : class, IActivity;
        
        Task VisitActivitiesAsync(IActivityVisitor visitor);

        #region Observability
        void AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory);

        void AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));

        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        void AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory);

        void AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;

        void AddObserver(ActivityObserverFactoryAsync observerFactory);

        void AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

        void AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;

        #endregion
    }
}
