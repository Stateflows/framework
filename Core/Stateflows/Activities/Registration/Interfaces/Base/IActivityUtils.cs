using System.Threading.Tasks;

namespace Stateflows.Activities.Registration.Interfaces.Base
{
    /// <summary>Configures cross-cutting activity behavior and resource metadata.</summary>
    /// <typeparam name="TReturn">Fluent builder type returned after this Activity element is configured.</typeparam>
    /// <remarks>Global settings apply to the activity definition being registered; they do not replace per-node
    /// configuration.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability">Observability documentation</seealso>
    public interface IActivityUtils<out TReturn>
    {
        /// <summary>Adds an asynchronously constructed event interceptor.</summary>
        /// <param name="interceptorFactory">Factory that creates the Activity-local component using the current execution's
        /// service provider.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddInterceptor(ActivityInterceptorFactoryAsync interceptorFactory);

        /// <summary>Adds a synchronously constructed event interceptor.</summary>
        /// <param name="interceptorFactory">Factory that creates the Activity-local component using the current execution's
        /// service provider.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddInterceptor(ActivityInterceptorFactory interceptorFactory)
            => AddInterceptor(serviceProvider => Task.FromResult(interceptorFactory(serviceProvider)));

        /// <summary>Adds an interceptor implementation type constructed through the Stateflows activation services.
        /// </summary>
        /// <typeparam name="TInterceptor">Typed Activity event interceptor implementing <see cref="IActivityInterceptor" />
        /// .</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IActivityInterceptor;

        /// <summary>Adds an asynchronously constructed activity observer.</summary>
        /// <param name="observerFactory">Factory that creates the Activity-local component using the current execution's
        /// service provider.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddObserver(ActivityObserverFactoryAsync observerFactory);
        
        /// <summary>Adds a synchronously constructed activity observer.</summary>
        /// <param name="observerFactory">Factory that creates the Activity-local component using the current execution's
        /// service provider.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddObserver(ActivityObserverFactory observerFactory)
            => AddObserver(serviceProvider => Task.FromResult(observerFactory(serviceProvider)));

        /// <summary>Adds an observer implementation type constructed through the Stateflows activation services.</summary>
        /// <typeparam name="TObserver">Typed Activity observer implementing <see cref="IActivityObserver" />.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddObserver<TObserver>()
            where TObserver : class, IActivityObserver;

        /// <summary>Registers a node- or scope-level handler for exceptions raised during Activity graph execution.
        /// </summary>
        /// <param name="exceptionHandlerFactory">Factory that creates the Activity-local component using the current
        /// execution's service provider.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddExceptionHandler(ActivityExceptionHandlerFactoryAsync exceptionHandlerFactory);

        /// <summary>Registers a node- or scope-level handler for exceptions raised during Activity graph execution.
        /// </summary>
        /// <param name="exceptionHandlerFactory">Factory that creates the Activity-local component using the current
        /// execution's service provider.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddExceptionHandler(ActivityExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler(serviceProvider => Task.FromResult(exceptionHandlerFactory(serviceProvider)));

        /// <summary>Registers a node- or scope-level handler for exceptions raised during Activity graph execution.
        /// </summary>
        /// <typeparam name="TExceptionHandler">Typed Activity exception handler implementing
        /// <see cref="IActivityExceptionHandler" />.</typeparam>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IActivityExceptionHandler;
        
        /// <summary>Sets the resource name associated with the activity definition.</summary>
        /// <param name="resourceName">Resource name used to group Activity executions for resource limits and
        /// observability.</param>
        /// <returns>The current builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn SetResourceName(string resourceName);
    }
}
