using System.Threading.Tasks;

namespace Stateflows.StateMachines.Registration.Interfaces.Base
{
    /// <summary>Configures local middleware, failure acknowledgement handlers, and resource identity for a machine.
    /// </summary>
    /// <typeparam name="TReturn">The machine builder returned after configuration.</typeparam>
    /// <remarks>Local middleware runs inside global middleware. Observers must await their continuations exactly once;
    /// interceptors may short-circuit. See <see href="https://github.com/Stateflows/framework/wiki/observability">
    /// observability</see>.</remarks>
    /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
    public interface IStateMachineUtils<out TReturn>
    {
        // /// <summary>
        // /// Sets default flag value for polymorphic triggers in State Machine's Transitions.<br/>
        // /// Default value of the flag is false unless Transition is triggered by an Exception descendant.
        // /// </summary>
        // /// <param name="polymorphicTriggers">Value of the polymorphic triggers flag.</param>
        // TReturn SetPolymorphicTriggers(bool polymorphicTriggers);
        
        /// <summary>Registers event-processing middleware for this State Machine definition.</summary>
        /// <param name="interceptorFactoryAsync">Asynchronous factory receiving the execution's service provider and
        /// machine action context and producing the local event-processing interceptor.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddInterceptor(StateMachineInterceptorFactoryAsync interceptorFactoryAsync);
        
        /// <summary>Registers event-processing middleware for this State Machine definition.</summary>
        /// <param name="interceptorFactory">Factory receiving the execution's service provider and machine action context
        /// and producing the local event-processing interceptor.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddInterceptor(StateMachineInterceptorFactory interceptorFactory)
            => AddInterceptor((serviceProvider, context) => Task.FromResult(interceptorFactory(serviceProvider, context)));
        
        /// <summary>Registers event-processing middleware for this State Machine definition.</summary>
        /// <typeparam name="TInterceptor">Typed machine-local event interceptor implementing
        /// <see cref="IStateMachineInterceptor" />.</typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddInterceptor<TInterceptor>()
            where TInterceptor : class, IStateMachineInterceptor;

        /// <summary>Registers an observer for this State Machine definition.</summary>
        /// <param name="observerFactoryAsync">Asynchronous factory receiving the execution's service provider and machine
        /// action context and producing the local observer.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddObserver(StateMachineObserverFactoryAsync observerFactoryAsync);

        /// <summary>Registers an observer for this State Machine definition.</summary>
        /// <param name="observerFactory">Factory receiving the execution's service provider and machine action context and
        /// producing the local observer.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddObserver(StateMachineObserverFactory observerFactory)
            => AddObserver((serviceProvider, context) => Task.FromResult(observerFactory(serviceProvider, context)));

        /// <summary>Registers an observer for this State Machine definition.</summary>
        /// <typeparam name="TObserver">Typed machine-local observer implementing <see cref="IStateMachineObserver" />.
        /// </typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddObserver<TObserver>()
            where TObserver : class, IStateMachineObserver;

        /// <summary>Registers an exception acknowledgement handler for this State Machine definition.</summary>
        /// <param name="exceptionHandlerFactoryAsync">Asynchronous factory receiving the execution's service provider and
        /// machine action context and producing the local exception acknowledgement handler.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddExceptionHandler(StateMachineExceptionHandlerFactoryAsync exceptionHandlerFactoryAsync);

        /// <summary>Registers an exception acknowledgement handler for this State Machine definition.</summary>
        /// <param name="exceptionHandlerFactory">Factory receiving the execution's service provider and machine action
        /// context and producing the local exception acknowledgement handler.</param>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddExceptionHandler(StateMachineExceptionHandlerFactory exceptionHandlerFactory)
            => AddExceptionHandler((serviceProvider, context) => Task.FromResult(exceptionHandlerFactory(serviceProvider, context)));

        /// <summary>Registers an exception acknowledgement handler for this State Machine definition.</summary>
        /// <typeparam name="TExceptionHandler">Typed machine-local exception acknowledgement handler implementing
        /// <see cref="IStateMachineExceptionHandler" />.</typeparam>
        /// <returns>The current machine builder for continued configuration.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/observability" />
        TReturn AddExceptionHandler<TExceptionHandler>()
            where TExceptionHandler : class, IStateMachineExceptionHandler;
        
        /// <summary>Sets the resource name associated with the machine definition.</summary>
        /// <param name="resourceName">The resource identifier exposed by the registered behavior class.</param>
        /// <returns>The current machine builder.</returns>
        /// <seealso href="https://github.com/Stateflows/framework/wiki/runtime" />
        TReturn SetResourceName(string resourceName);
    }
}
