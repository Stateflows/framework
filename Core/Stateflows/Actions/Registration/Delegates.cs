using System.Threading.Tasks;

namespace Stateflows.Actions.Registration
{
    public delegate Task ActionDelegateAsync(IActionContext context);

    public delegate void ActionsBuildAction(IActionsRegister register);

    // public delegate IActivityObserver ActivityObserverFactory(IServiceProvider serviceProvider);
    //
    // public delegate IActivityInterceptor ActivityInterceptorFactory(IServiceProvider serviceProvider);
    //
    // public delegate IActivityExceptionHandler ActivityExceptionHandlerFactory(IServiceProvider serviceProvider);
}
