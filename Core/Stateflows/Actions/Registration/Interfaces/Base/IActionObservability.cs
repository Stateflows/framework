using System.Threading.Tasks;

namespace Stateflows.Actions.Registration.Interfaces.Base;

public interface IActionObservability<out TReturn>
{
    TReturn AddInterceptor(ActionInterceptorFactory interceptorFactory)
        => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
    
    TReturn AddInterceptor(ActionInterceptorFactoryAsync interceptorFactory);

    TReturn AddInterceptor<TInterceptor>()
        where TInterceptor : class, IActionInterceptor;

    TReturn AddObserver(ActionObserverFactory observerFactory)
        => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

    TReturn AddObserver(ActionObserverFactoryAsync observerFactory);

    TReturn AddObserver<TObserver>()
        where TObserver : class, IActionObserver;

    TReturn AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory)
        => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

    TReturn AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

    TReturn AddExceptionHandler<TExceptionHandler>()
        where TExceptionHandler : class, IActionExceptionHandler;
}
