using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions.Registration
{
    /// <summary>Executes the body of a registered action.</summary>
    /// <param name="context">The action delegate context.</param>
    public delegate Task ActionDelegateAsync(IActionDelegateContext context);

    /// <summary>Configures action registrations.</summary>
    /// <param name="register">The action registration builder.</param>
    public delegate void ActionsBuildAction(IActionsBuilder register);

    /// <summary>Asynchronously creates an action interceptor.</summary>
    /// <param name="serviceProvider">The service provider available to the action runtime.</param>
    /// <returns>A task containing the interceptor.</returns>
    public delegate Task<IActionInterceptor> ActionInterceptorFactoryAsync(IServiceProvider serviceProvider);

    /// <summary>Creates an action interceptor.</summary>
    /// <param name="serviceProvider">The service provider available to the action runtime.</param>
    /// <returns>The interceptor instance.</returns>
    public delegate IActionInterceptor ActionInterceptorFactory(IServiceProvider serviceProvider);

    /// <summary>Asynchronously creates an action observer.</summary>
    /// <param name="serviceProvider">The service provider available to the action runtime.</param>
    /// <returns>A task containing the observer.</returns>
    public delegate Task<IActionObserver> ActionObserverFactoryAsync(IServiceProvider serviceProvider);

    /// <summary>Creates an action observer.</summary>
    /// <param name="serviceProvider">The service provider available to the action runtime.</param>
    /// <returns>The observer instance.</returns>
    public delegate IActionObserver ActionObserverFactory(IServiceProvider serviceProvider);
    
    /// <summary>Asynchronously creates an action exception handler.</summary>
    /// <param name="serviceProvider">The service provider available to the action runtime.</param>
    /// <returns>A task containing the exception handler.</returns>
    public delegate Task<IActionExceptionHandler> ActionExceptionHandlerFactoryAsync(IServiceProvider serviceProvider);
    
    /// <summary>Creates an action exception handler.</summary>
    /// <param name="serviceProvider">The service provider available to the action runtime.</param>
    /// <returns>The exception-handler instance.</returns>
    public delegate IActionExceptionHandler ActionExceptionHandlerFactory(IServiceProvider serviceProvider);
}
