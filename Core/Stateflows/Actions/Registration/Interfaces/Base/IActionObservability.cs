using System.Threading.Tasks;

namespace Stateflows.Actions.Registration.Interfaces.Base;

/// <summary>Configures action interceptors, observers, and exception handlers.</summary>
/// <typeparam name="TReturn">The builder type returned for fluent configuration.</typeparam>
/// <remarks>Global and action-local observability components are composed by the action runtime.</remarks>
/// <seealso href="https://github.com/Stateflows/framework/wiki/observability">Observability documentation</seealso>
public interface IActionObservability<out TReturn>
{
    /// <summary>Adds an interceptor using a synchronous factory.</summary>
    /// <param name="interceptorFactory">Factory that creates an interceptor from the current service provider.</param>
    /// <returns>The current builder.</returns>
    TReturn AddInterceptor(ActionInterceptorFactory interceptorFactory)
        => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));
    
    /// <summary>Adds an interceptor using an asynchronous factory.</summary>
    /// <param name="interceptorFactory">Asynchronous factory for the interceptor.</param>
    /// <returns>The current builder.</returns>
    TReturn AddInterceptor(ActionInterceptorFactoryAsync interceptorFactory);

    /// <summary>Adds an interceptor implementation type.</summary>
    /// <typeparam name="TInterceptor">The interceptor type.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddInterceptor<TInterceptor>()
        where TInterceptor : class, IActionInterceptor;

    /// <summary>Adds an observer using a synchronous factory.</summary>
    /// <param name="observerFactory">Factory that creates an observer from the current service provider.</param>
    /// <returns>The current builder.</returns>
    TReturn AddObserver(ActionObserverFactory observerFactory)
        => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

    /// <summary>Adds an observer using an asynchronous factory.</summary>
    /// <param name="observerFactory">Asynchronous factory for the observer.</param>
    /// <returns>The current builder.</returns>
    TReturn AddObserver(ActionObserverFactoryAsync observerFactory);

    /// <summary>Adds an observer implementation type.</summary>
    /// <typeparam name="TObserver">The observer type.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddObserver<TObserver>()
        where TObserver : class, IActionObserver;

    /// <summary>Adds an exception handler using a synchronous factory.</summary>
    /// <param name="exceptionHandlerFactory">Factory that creates a handler from the current service provider.</param>
    /// <returns>The current builder.</returns>
    TReturn AddExceptionHandler(ActionExceptionHandlerFactory exceptionHandlerFactory)
        => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

    /// <summary>Adds an exception handler using an asynchronous factory.</summary>
    /// <param name="exceptionHandlerFactoryAsync">Asynchronous factory for the handler.</param>
    /// <returns>The current builder.</returns>
    TReturn AddExceptionHandler(ActionExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

    /// <summary>Adds an exception-handler implementation type.</summary>
    /// <typeparam name="TExceptionHandler">The exception-handler type.</typeparam>
    /// <returns>The current builder.</returns>
    TReturn AddExceptionHandler<TExceptionHandler>()
        where TExceptionHandler : class, IActionExceptionHandler;
}
