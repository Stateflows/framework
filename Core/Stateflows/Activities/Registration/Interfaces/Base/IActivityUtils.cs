using System.Threading.Tasks;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    public interface IActivityUtils<out TReturn>
    {
        TReturn AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory);

        TReturn AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));

        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        TReturn AddObserver(ActivityObserverFactoryAsync observerFactory);
        
        TReturn AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

        TReturn AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;

        TReturn AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory);

        TReturn AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
        
        TReturn SetResourceName(string resourceName);
    }
}
