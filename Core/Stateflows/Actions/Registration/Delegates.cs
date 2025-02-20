using System.Threading.Tasks;
using Stateflows.Actions.Context.Interfaces;

namespace Stateflows.Actions.Registration
{
    public delegate Task ActionDelegateAsync(IActionDelegateContext context);

    public delegate void ActionsBuildAction(IActionsBuilder register);

    // public delegate IActivityObserver ActivityObserverFactory(IServiceProvider serviceProvider);
    //
    // public delegate IActivityInterceptor ActivityInterceptorFactory(IServiceProvider serviceProvider);
    //
    // public delegate IActivityExceptionHandler ActivityExceptionHandlerFactory(IServiceProvider serviceProvider);
}
