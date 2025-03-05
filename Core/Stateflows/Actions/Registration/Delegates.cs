using System;
using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions.Registration
{
    public delegate Task ActionDelegateAsync(IActionDelegateContext context);

    public delegate void ActionsBuildAction(IActionsBuilder register);

    public delegate Task<IActionInterceptor> ActionInterceptorFactoryAsync(IServiceProvider serviceProvider);
    
    public delegate Task<IActionExceptionHandler> ActionExceptionHandlerFactoryAsync(IServiceProvider serviceProvider);
}
