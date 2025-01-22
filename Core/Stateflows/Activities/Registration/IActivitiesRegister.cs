using System;
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
        
        void AddActivity(string activityName, int version, Type activityType);
        
        void AddActivity<TActivity>(string activityName = null, int version = 1)
            where TActivity : class, IActivity;

        #region Observability
        void AddInterceptor(ActivityInterceptorFactory interceptorFactory);

        void AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        void AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory);

        void AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;

        void AddObserver(ActivityObserverFactory observerFactory);

        void AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;

        #endregion
    }
}
