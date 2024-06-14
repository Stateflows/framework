namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityUtils<out TReturn>
    {
        TReturn AddInterceptor(ActivityInterceptorFactory interceptorFactory);

        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        TReturn AddObserver(ActivityObserverFactory observerFactory);

        TReturn AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;

        TReturn AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory);

        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
    }
}
